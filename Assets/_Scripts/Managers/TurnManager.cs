using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : Singleton<TurnManager>
{
    [SerializeField] private Transform worldParent;
    [SerializeField] private float wave = 3;
    [SerializeField] private ActionQueueUI PlayerActionQueueUI;
    [SerializeField] private ActionQueueUI EnemyActionQueueUI;
    [Header("Phase Information")]
    [SerializeField] private UserData userData;
    public UserData UserData => userData;

    [Header("Playerstyle Logging")]
    [SerializeField] private bool enablePlayerstyleLogging = true;
    [SerializeField] private StageCompleteSender stageCompleteSender;
    [SerializeField] private bool sendPlayerstyleOnWin = true;
    [SerializeField] private bool sendPlayerstyleOnLose = false;

    private CombatLogger combatLogger;
    private CombatActionProcessor actionProcessor;
    public CombatResourceManager ResourceManager => resourceManager;
    private CombatResourceManager resourceManager;
    public bool UseSharedPlayerSkillPointPool => resourceManager.UseSharedPlayerSkillPointPool;
    private TurnState currentState;
    private List<ActionQueue> actionQueue = new List<ActionQueue>();
    public int turnRound = 0;
    public int combatID = 0;
    public int currentWave = 1;
    private bool combatResultResolved;

    private readonly List<CombatLog> stageLogs = new List<CombatLog>();
    private string playerstyleSessionId;

    // --- Multiplayer Turn Variables ---
    private int currentTeamMemberIndex = 0;
    private bool initialWaveSpawned = false;
    private List<ActionQueue> pendingPlayerActions = new List<ActionQueue>();
    public PlayerEntity CurrentActivePlayer =>
        PlayerTeamManager.Instance != null ? PlayerTeamManager.Instance.GetMemberAt(currentTeamMemberIndex) : null;

    private async void Start()
    {
        resourceManager = GetComponent<CombatResourceManager>();
        combatLogger = GetComponent<CombatLogger>();
        actionProcessor = GetComponent<CombatActionProcessor>();

        ApplyUserDataFallbackFromPrefs();

        if (stageCompleteSender == null)
        {
            stageCompleteSender = GetComponent<StageCompleteSender>();
        }

        playerstyleSessionId = Guid.NewGuid().ToString();
        stageLogs.Clear();

        resourceManager.Initialize(userData);
        actionProcessor.Initialize(this);

        combatResultResolved = false;
        EnsureTeamReady();

        resourceManager.ApplyPhaseStats(currentWave);
        resourceManager.SyncSharedPlayerSkillPoints();

        if (EnemyGenerator.Instance != null && EnemyGenerator.Instance.GetCurrentQuest() != null)
        {
            wave = EnemyGenerator.Instance.GetCurrentQuest().waves.Length;
        }

        if (GetAllEnemies().Count > 0)
        {
            initialWaveSpawned = true;
        }
        else if (EnemyGenerator.Instance != null)
        {
            EnemyGenerator.Instance.GenerateInitialEnemy();
            initialWaveSpawned = true;
        }

        SetState(TurnState.PlayerTurnState);
        // combatID = await NetworkManager.GetLatestCombatID();
        List<SkillLogs> skillLogs = new List<SkillLogs>();

        if (PlayerTeamManager.Instance != null && PlayerTeamManager.Instance.ActiveTeamMembers.Count > 0)
        {
            foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
            {
                if (member == null) continue;
                foreach (var skill in member.skillManager.GetSkills())
                {
                    skillLogs.Add(new SkillLogs
                    {
                        SkillID = skill.skillID,
                        SkillName = skill.skillName
                    });
                }
            }
        }

        // await NetworkManager.SaveCombatSkillLoadoutLogs(new CombatSkillLoadoutLogs
        // {
        //     CombatID = combatID,
        //     SkillLoadut = skillLogs
        // });
    }
    public void SetState(TurnState newState)
    {
        currentState = newState;
        switch (currentState)
        {
            case TurnState.PlayerTurnState:
                PlayerActionQueueUI.SetActionQueue(null);
                EnemyActionQueueUI.SetActionQueue(null);
                currentTeamMemberIndex = 0;
                AdvanceToNextAlivePlayer();
                pendingPlayerActions.Clear();
                HandlePlayerTurn();
                break;
            case TurnState.SpeedCompareState:
                List<GameObject> enemies = GetAllEnemies();
                if (enemies.Count == 0)
                {
                    SetState(TurnState.PlayerTurnState);
                    return;
                }

                if (PlayerTeamManager.Instance != null)
                {
                    foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
                    {
                        if (member != null && member.CurrentHealth > 0) member.SetPlayerState(PlayerActionState.Action);
                    }
                }

                foreach (GameObject enemy in enemies)
                {
                    EnemyCombat enemyCombat = enemy.GetComponent<EnemyCombat>();
                    enemyCombat.SetTargetIndicator(false);
                }
                HandleSpeedComparison();
                break;
            case TurnState.ActionState:
                StartCoroutine(HandleActionExecution());
                break;
            case TurnState.Win:
                Win();
                break;
            case TurnState.Lose:
                Lose();
                break;
        }
    }
    public void SubmitPlayerAction(Entity caster, Entity target, Skill skill)
    {
        // Calculate dynamic speed.
        float actionSpeed = caster.GetStat(StatType.ActionSpeed); // Use Getter
        pendingPlayerActions.Add(new ActionQueue(caster, target, skill, actionSpeed));

        currentTeamMemberIndex++;
        AdvanceToNextAlivePlayer();

        // Reset UI stuff
        if (caster is PlayerEntity ce)
        {
            ce.SetSelectedSkill(null);
            ce.SetEnemyTarget(null);
            ce.SetPlayerState(PlayerActionState.Idle);
        }
        TargetingPanel.instance.SetActivePanel(false);
        SkillPanelUI.Instance.gameObject.SetActive(false);

        foreach (var enemy in FindObjectsOfType<EnemyCombat>())
        {
            enemy.SetTargetIndicator(false);
        }

        if (PlayerTeamManager.Instance != null)
        {
            foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
            {
                if (member != null) member.SetTargetIndicator(false);
            }
        }

        if (PlayerTeamManager.Instance == null || currentTeamMemberIndex >= PlayerTeamManager.Instance.ActiveTeamMembers.Count)
        {
            // Everyone has chosen, move to sorting phase
            SetState(TurnState.SpeedCompareState);
        }
        else
        {
            // More characters need to choose
            HandlePlayerTurn();
        }
    }

    private void HandlePlayerTurn()
    {
        if (!HasAlivePlayers())
        {
            SetState(TurnState.Lose);
            return;
        }

        PlayerEntity activePlayer = CurrentActivePlayer;
        if (activePlayer == null)
        {
            SetState(TurnState.SpeedCompareState);
            return;
        }

        // // Debug.Log($"[TurnManager] Waiting for {activePlayer.Stats.EntityName} to choose an action.");

        activePlayer.SetEnemyTarget(GetFirstEnemy());

        // Default ally target to someone else if possible
        PlayerEntity defaultAlly = activePlayer;
        if (PlayerTeamManager.Instance != null && PlayerTeamManager.Instance.ActiveTeamMembers.Count > 1)
        {
            foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
            {
                if (member != null && member != activePlayer && member.CurrentHealth > 0)
                {
                    defaultAlly = member;
                    break;
                }
            }
        }
        activePlayer.SetAllyTarget(defaultAlly);

        TargetingPanel.instance.SetEnemyTargetPanel(activePlayer.GetEnemyTarget());
        ActionBarUI.Instance.gameObject.SetActive(true);

        if (currentTeamMemberIndex == 0) // Only increment turnRound once per cycle
        {
            turnRound++;
        }
        List<GameObject> remainingEnemies = GetAllEnemies();
        remainingEnemies.RemoveAll(enemy => enemy.GetComponent<EnemyCombat>().IsDead());
        if (remainingEnemies.Count == 0)
        {
            if (!initialWaveSpawned)
            {
                EnemyGenerator.Instance?.GenerateInitialEnemy();
                initialWaveSpawned = true;
            }
            else if (currentWave < wave)
            {
                currentWave += 1;
                EnemyGenerator.Instance?.GenerateEnemy();
                resourceManager.ApplyPhaseStats(currentWave);
                resourceManager.SyncSharedPlayerSkillPoints();
            }
            else
            {
                SetState(TurnState.Win);
            }
        }
    }

    private void HandleSpeedComparison()
    {
        ActionBarUI.Instance.gameObject.SetActive(false);
        SkillPanelUI.Instance.gameObject.SetActive(false);

        // Add all confirmed player actions to the main queue
        actionQueue.AddRange(pendingPlayerActions);
        pendingPlayerActions.Clear();

        List<GameObject> enemies = GetAllEnemies();
        enemies.RemoveAll(enemy => enemy.GetComponent<EnemyCombat>().IsDead());
        foreach (GameObject enemy in enemies)
        {
            Entity enemyEntity = enemy.GetComponent<EnemyCombat>();

            // Skip entities that don't take turns (like Elemental Pillars)
            if (!enemyEntity.CanTakeTurn()) continue;

            Skill chosenSkill;

            // Check against the base BossCombat class, not a specific boss
            if (enemyEntity is BossCombat boss)
            {
                chosenSkill = boss.ChooseNextSkill();
            }
            else
            {
                chosenSkill = enemyEntity.skillManager.RandomSkill();
            }

            Entity validTarget = GetActualTargetForEnemy(enemyEntity);

            ActionQueue enemyAction = new ActionQueue(enemyEntity, validTarget, chosenSkill, enemyEntity.GetStat(StatType.ActionSpeed));
            actionQueue.Add(enemyAction);
        }
        actionQueue.Sort((a, b) => b.ActionSpeed.CompareTo(a.ActionSpeed));
        SetState(TurnState.ActionState);
    }

    public Entity GetActualTargetForEnemy(Entity enemyEntity)
    {
        // 1. Check if the enemy is taunted
        if (enemyEntity.buffController != null)
        {
            var tauntBuffs = enemyEntity.buffController.GetBuffsByType(BuffType.Debuff);
            foreach (var activeBuff in tauntBuffs)
            {
                if (activeBuff.Data is TauntBuff taunt && taunt.Taunter != null && taunt.Taunter.CurrentHealth > 0)
                {
                    return taunt.Taunter;
                }
            }
        }

        // 2. Otherwise pick a random alive player
        if (PlayerTeamManager.Instance != null && PlayerTeamManager.Instance.ActiveTeamMembers.Count > 0)
        {
            var alivePlayers = PlayerTeamManager.Instance.GetAliveMembers();
            if (alivePlayers.Count > 0)
            {
                return alivePlayers[UnityEngine.Random.Range(0, alivePlayers.Count)];
            }
        }

        // Fallback for safety
        if (PlayerTeamManager.Instance != null && PlayerTeamManager.Instance.ActiveTeamMembers.Count > 0)
            return PlayerTeamManager.Instance.ActiveTeamMembers[0];

        return null;
    }

    private IEnumerator HandleActionExecution()
    {
        int actionID = 0;
        yield return new WaitForSeconds(1f);
        while (actionQueue.Count > 0)
        {
            ActionQueue currentAction = actionQueue[0];
            Entity entity = currentAction.Caster;
            Skill skill = currentAction.Skill;
            Entity target = currentAction.Target;

            if (entity == null || entity.CurrentHealth <= 0)
            {
                actionQueue.RemoveAt(0);
                continue;
            }

            // // Debug.Log(entity.gameObject.name + " is taking action with " + currentAction.ActionSpeed + " speed.");
            // PlayerActionQueueUI.SetActionQueue(null);
            // EnemyActionQueueUI.SetActionQueue(null);
            PlayerActionQueueUI?.SetActionQueue(null);
            EnemyActionQueueUI?.SetActionQueue(null);
            // CombatActionLog log = new()
            // {
            //     CombatID = combatID,
            //     TurnID = turnRound,
            //     ActionID = actionID++,
            //     CasterID = entity.GetEntityID(),
            //     CasterName = entity.Stats.EntityName,
            //     TargetID = target.GetEntityID(),
            //     TargetName = target.Stats.EntityName,
            //     SkillID = skill.skillID,
            //     SkillName = skill.skillName,
            //     ActionPointUsed = skill.SkillPoint,
            //     ActionPointRecovery = skill.SkillPointRestore,
            //     ActionSpeed = currentAction.ActionSpeed,
            //     DamageEffectLogs = new(),
            //     BuffEffectLogs = new(),
            //     HealEffectLogs = new(),
            //     EntityLogs = new()
            // };
            CombatActionLog log = new()
            {
                CombatID = combatID,
                TurnID = turnRound,
                ActionID = actionID++,
                CasterID = entity != null ? entity.GetEntityID() : -1,
                CasterName = entity?.Stats?.EntityName ?? "Unknown Caster", // ถ้า entity หรือ stats เป็น null จะใช้ชื่อนี้แทน

                TargetID = target != null ? target.GetEntityID() : -1,
                TargetName = target?.Stats?.EntityName ?? "No Target", // ป้องกันกรณีเป้าหมายไม่มี/ตายไปแล้ว

                SkillID = skill != null ? skill.skillID : -1,
                SkillName = skill?.skillName ?? "Unknown Skill",

                ActionPointUsed = GetSkillCost(skill),
                ActionPointRecovery = 0,
                ActionSpeed = currentAction.ActionSpeed,
                DamageEffectLogs = new(),
                BuffEffectLogs = new(),
                HealEffectLogs = new(),
                EntityLogs = new()
            };

            // Log stats for all alive players
            if (PlayerTeamManager.Instance != null)
            {
                foreach (var member in PlayerTeamManager.Instance.GetAliveMembers())
                {
                    log.AddEntityLog(new EntityStatData(member));
                }
            }

            foreach (var enemy in GetAllEnemies())
            {
                var enemyEntity = enemy.GetComponent<EnemyCombat>();
                if (enemyEntity)
                {
                    log.AddEntityLog(new EntityStatData(enemyEntity));
                }
            }

            bool hadMomentumBefore = HasMomentum(entity);

            // --- BUFF TURN START ---
            entity.buffController.OnTurnStart(entity, log);

            // --- IMMEDIATE LOSE CHECK ---
            if (!HasAlivePlayers())
            {
                SetState(TurnState.Lose);
                yield break;
            }

            // --- EXECUTE SKILL (DELEGATED) ---
            if (entity.CanAction())
            {
                actionProcessor.ExecuteAction(currentAction, log);
            }

            // --- BUFF TURN END ---
            entity.buffController.OnTurnEnd(entity, log);

            bool hasMomentumAfter = HasMomentum(entity);
            TryRecordPlayerstyleLog(currentAction, log, hadMomentumBefore, hasMomentumAfter);

            // --- UPDATE UI AND REMOVE FROM QUEUE ---
            if (entity is PlayerEntity) PlayerActionQueueUI?.SetActionQueue(currentAction);
            else EnemyActionQueueUI?.SetActionQueue(currentAction);

            actionQueue.Remove(currentAction);
            combatLogger.ShowLog(log);

            List<GameObject> remainingEnemies = GetAllEnemies();
            remainingEnemies.RemoveAll(enemy => enemy.GetComponent<EnemyCombat>().IsDead());
            if (remainingEnemies.Count == 0 && currentWave >= wave)
            {
                SetState(TurnState.Win);
                yield break;
            }
            yield return new WaitForSeconds(1f);
        }

        bool allPlayersDead = true;
        if (PlayerTeamManager.Instance != null)
        {
            foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
            {
                if (member != null && member.CurrentHealth > 0)
                {
                    allPlayersDead = false;
                    break;
                }
            }
        }

        if (allPlayersDead)
        {
            SetState(TurnState.Lose);
            yield break;
        }

        // Progress enemy break states at the end of the round
        foreach (GameObject enemy in GetAllEnemies())
        {
            var combat = enemy.GetComponent<EnemyCombat>();
            if (combat != null && !combat.IsDead())
            {
                combat.AdvanceBreakState();
            }
        }

        // Restore skill points based on current phase
        resourceManager.RestoreSkillPointsByPhase(GetAllEnemies());

        // Advance the weather at the end of the round, right before the next starts
        if (WeatherManager.Instance != null)
        {
            WeatherManager.Instance.AdvanceWeather();
        }

        SetState(TurnState.PlayerTurnState);
        yield break;
    }

    private void Win()
    {
        if (combatResultResolved)
        {
            return;
        }

        combatResultResolved = true;
        TrySendPlayerstyleLogs(true);

        if (userData != null)
        {
            bool didAdvancePhase;
            bool didFinishGame;
            int earnedCoins;

            bool completed = userData.TryCompleteSelectedQuest(out didAdvancePhase, out didFinishGame, out earnedCoins);
            if (!completed)
            {
                Debug.LogWarning("Quest completion was skipped because selected quest is not in a completable state.");
            }
            else
            {
                Debug.Log("Quest reward received: " + earnedCoins + " coins. Total coins: " + userData.TotalCoins);

                if (didFinishGame)
                {
                    Debug.Log("Quest 3 completed. Game progression finished.");
                }
                else if (didAdvancePhase)
                {
                    Debug.Log("Quest completed. Advanced to phase " + userData.GamePhase + ".");
                }
            }
        }
        else
        {
            Debug.LogWarning("UserData is missing in TurnManager. Progression was not updated.");
        }

        if (CombatResultUI.Instance != null)
        {
            CombatResultUI.Instance.gameObject.SetActive(true);
        }
        else
        {
            Loader.Load(Loader.Scenes.Overworld);
        }
    }

    private void Lose()
    {
        if (combatResultResolved)
        {
            return;
        }

        combatResultResolved = true;
        TrySendPlayerstyleLogs(false);
        Loader.Load(Loader.Scenes.Overworld);
    }

    private void TryRecordPlayerstyleLog(ActionQueue action, CombatActionLog log, bool hadMomentumBefore, bool hasMomentumAfter)
    {
        if (!ShouldCollectPlayerstyleData() || action == null || log == null) return;
        if (action.Caster is not PlayerEntity) return;

        CombatLog entry = BuildCombatLog(action, log, hadMomentumBefore, hasMomentumAfter);
        if (entry != null)
        {
            stageLogs.Add(entry);
        }
    }

    private CombatLog BuildCombatLog(ActionQueue action, CombatActionLog log, bool hadMomentumBefore, bool hasMomentumAfter)
    {
        Entity caster = action.Caster;
        Skill skill = action.Skill;
        Entity target = action.Target;

        int casterId = ResolveCharacterId(caster);
        float damageDealt = 0f;
        float damageReceived = 0f;

        if (log.DamageEffectLogs != null)
        {
            foreach (var dmg in log.DamageEffectLogs)
            {
                if (dmg == null || dmg.Damage == null) continue;
                float amount = dmg.Damage.Amount;
                if (dmg.AppliedTargetID == casterId) damageReceived += amount;
                else damageDealt += amount;
            }
        }

        float healAmount = 0f;
        if (log.HealEffectLogs != null)
        {
            foreach (var heal in log.HealEffectLogs)
            {
                if (heal == null) continue;
                healAmount += heal.HealAmount;
            }
        }

        int momentumGain = 0;
        if (log.BuffEffectLogs != null)
        {
            foreach (var buff in log.BuffEffectLogs)
            {
                if (buff == null || buff.Buff == null) continue;
                if (buff.AppliedTargetID != casterId) continue;
                if (buff.Buff.BuffName == "Momentum")
                {
                    momentumGain += 1;
                }
            }
        }

        int frenzyStacks = 0;
        if (caster != null && caster.buffController != null)
        {
            ActiveBuff frenzy = caster.buffController.GetBuffByName("Frenzy");
            if (frenzy != null) frenzyStacks = frenzy.CurrentStack;
        }

        float casterMaxHp = caster != null ? caster.GetStat(StatType.MaxHealth) : 0f;
        float casterCorrupted = caster != null ? caster.CorruptedHealth : 0f;
        float corruptPercent = casterMaxHp > 0f ? (casterCorrupted / casterMaxHp) * 100f : 0f;

        int casterCurrentSp = 0;
        if (caster != null)
        {
            casterCurrentSp = UseSharedPlayerSkillPointPool
                ? resourceManager.GetSharedPlayerCurrentSkillPoints()
                : caster.CurrentSP;
        }

        string weather = "sunny";
        if (WeatherManager.Instance != null)
        {
            weather = MapWeatherForApi(WeatherManager.Instance.CurrentWeather);
        }

        return new CombatLog
        {
            session_id = playerstyleSessionId,
            player_id = userData != null ? userData.ID : -1,
            character_id = casterId,
            wave_number = currentWave,
            turn_index = log.TurnID,
            skill_id = skill != null ? skill.skillID : -1,
            skill_target_id = target != null ? target.GetEntityID() : -1,
            target_max_hp = target != null ? target.GetStat(StatType.MaxHealth) : 0f,
            target_current_hp = target != null ? target.CurrentHealth : 0f,
            damage_dealt = damageDealt,
            damage_recieve = damageReceived,
            caster_current_sp = casterCurrentSp,
            caster_current_hp = caster != null ? caster.CurrentHealth : 0f,
            caster_max_hp = casterMaxHp,
            current_frenzy_stack = frenzyStacks,
            heal_amount = healAmount,
            current_corrupt_blood_gain = casterCorrupted,
            corrupt_blood_by_max_hp = corruptPercent,
            weather = weather,
            momentum_gain = momentumGain,
            momentum_used = hadMomentumBefore && !hasMomentumAfter ? 1 : 0
        };
    }

    private bool HasMomentum(Entity entity)
    {
        if (entity == null || entity.buffController == null) return false;
        ActiveBuff momentum = entity.buffController.GetBuffByName("Momentum");
        return momentum != null;
    }

    private int ResolveCharacterId(Entity caster)
    {
        if (caster is PlayerEntity player)
        {
            int resolved = player.GetEntityID();
            if (resolved <= 0 && PlayerTeamManager.Instance != null)
            {
                int index = PlayerTeamManager.Instance.IndexOfMember(player);
                if (index >= 0) resolved = index + 1;
            }

            if (resolved <= 0) resolved = 1;
            return Mathf.Clamp(resolved, 1, 2);
        }

        int fallback = caster != null ? caster.GetEntityID() : 1;
        if (fallback <= 0) fallback = 1;
        return Mathf.Clamp(fallback, 1, 2);
    }

    private void ApplyUserDataFallbackFromPrefs()
    {
        if (userData == null) return;

        if (string.IsNullOrEmpty(userData.Username))
        {
            string savedName = PlayerPrefs.GetString("PlayerName", string.Empty);
            if (!string.IsNullOrEmpty(savedName))
            {
                userData.Username = savedName;
            }
        }

        if (userData.ID <= 0)
        {
            int savedId = PlayerPrefs.GetInt("UserId", -1);
            if (savedId > 0)
            {
                userData.ID = savedId;
            }
        }
    }

    private static string MapWeatherForApi(WeatherType weather)
    {
        switch (weather)
        {
            case WeatherType.Sunny:
                return "sunny";
            case WeatherType.Rainy:
                return "rainy";
            case WeatherType.Windy:
                return "autumn";
            default:
                return "sunny";
        }
    }

    private bool ShouldCollectPlayerstyleData()
    {
        if (!enablePlayerstyleLogging) return false;

        if (userData != null)
        {
            if (userData.SelectedQuestIndex == UserData.TutorialQuestIndex) return false;
        }

        QuestEnemies quest = EnemyGenerator.Instance != null ? EnemyGenerator.Instance.GetCurrentQuest() : null;
        if (quest != null && quest.isTutorial) return false;

        return true;
    }

    private void TrySendPlayerstyleLogs(bool isWin)
    {
        if (!ShouldCollectPlayerstyleData()) return;
        if (stageLogs.Count == 0) return;

        if (isWin && !sendPlayerstyleOnWin) return;
        if (!isWin && !sendPlayerstyleOnLose) return;

        if (stageCompleteSender == null)
        {
            Debug.LogWarning("StageCompleteSender is not assigned. Playerstyle logs were not sent.");
            return;
        }

        CombatLogBatchRequest request = new CombatLogBatchRequest
        {
            items = new List<CombatLog>(stageLogs)
        };

        StartCoroutine(stageCompleteSender.SendStageComplete(request));
        stageLogs.Clear();
    }
    private List<GameObject> GetAllEnemies()
    {
        List<GameObject> enemies = new List<GameObject>();
        foreach (Transform child in worldParent)
        {
            if (child.CompareTag("Enemy"))
            {
                enemies.Add(child.gameObject);
            }
        }
        return enemies;
    }

    public List<EnemyCombat> GetAliveEnemiesInBattleOrder()
    {
        List<EnemyCombat> aliveEnemies = new List<EnemyCombat>();
        foreach (GameObject enemyObj in GetAllEnemies())
        {
            EnemyCombat enemyCombat = enemyObj.GetComponent<EnemyCombat>();
            if (enemyCombat != null && !enemyCombat.IsDead())
            {
                aliveEnemies.Add(enemyCombat);
            }
        }

        // Sort by world X so "adjacent" follows battlefield layout.
        aliveEnemies.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        return aliveEnemies;
    }
    public void RemoveEntityFromActionQueue(Entity entity)
    {
        actionQueue.RemoveAll(a => a.Caster == entity);
    }
    private EnemyCombat GetFirstEnemy()
    {
        var enemies = FindObjectsOfType<EnemyCombat>();
        if (enemies.Length == 0) return null;
        // Return the first alive enemy
        foreach (var enemy in enemies)
        {
            if (!enemy.IsDead())
                return enemy;
        }
        return null;
    }
    public TurnState GetTurnState()
    {
        return currentState;
    }

    public int GetSkillCost(Skill skill)
    {
        if (skill == null) return 0;
        return skill.SkillPoint;
    }

    public int GetProjectedAvailableSP(Entity entity)
    {
        int currentSP = UseSharedPlayerSkillPointPool ? resourceManager.GetSharedPlayerCurrentSkillPoints() : (int)entity.CurrentSP;
        int reservedSP = 0;

        foreach (var action in pendingPlayerActions)
        {
            int actionCost = GetSkillCost(action.Skill);
            if (UseSharedPlayerSkillPointPool)
            {
                // In shared pool, all pending player actions consume from the same pool
                if (action.Caster is PlayerEntity)
                {
                    reservedSP += actionCost;
                }
            }
            else
            {
                // In individual pool, only actions from this specific entity matter
                if (action.Caster == entity)
                {
                    reservedSP += actionCost;
                }
            }
        }

        return currentSP - reservedSP;
    }

    public int GetCurrentPhase()
    {
        return resourceManager.GetCurrentPhase();
    }

    public float GetMaxWave()
    {
        return wave;
    }

    private void EnsureTeamReady()
    {
        if (PlayerTeamManager.Instance != null && PlayerTeamManager.Instance.ActiveTeamMembers.Count == 0)
        {
            PlayerTeamManager.Instance.SpawnTeam();
        }
    }

    private bool HasAlivePlayers()
    {
        return PlayerTeamManager.Instance != null && PlayerTeamManager.Instance.GetAliveMembers().Count > 0;
    }

    private void AdvanceToNextAlivePlayer()
    {
        if (PlayerTeamManager.Instance == null) return;

        while (currentTeamMemberIndex < PlayerTeamManager.Instance.ActiveTeamMembers.Count)
        {
            PlayerEntity member = PlayerTeamManager.Instance.ActiveTeamMembers[currentTeamMemberIndex];
            if (member != null && member.CurrentHealth > 0)
            {
                break;
            }
            currentTeamMemberIndex++;
        }
    }
    public void RemoveActionQueue(Entity entity)
    {
        actionQueue.RemoveAll(a => a.Caster == entity);
    }
    public List<ActionQueue> GetActionQueues()
    {
        return actionQueue;
    }
}
