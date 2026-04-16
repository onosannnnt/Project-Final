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

    [Header("Skill Point Restoration per Phase")]
    [SerializeField] private int phase1SPRestore = 10; // Restore X SP per turn in Phase 1
    [SerializeField] private int phase2SPRestore = 20; // Restore Y SP per turn in Phase 2
    [SerializeField] private int phase3SPRestore = 30; // Restore Z SP per turn in Phase 3
    [Header("Team Resource Settings")]
    [SerializeField] private bool useSharedPlayerSkillPointPool = true;
    private TurnState currentState;
    private List<ActionQueue> actionQueue = new List<ActionQueue>();
    public int turnRound = 0;
    public int combatID = 0;
    public int currentWave = 1;
    
    // --- Multiplayer Turn Variables ---
    private int currentTeamMemberIndex = 0;
    private bool initialWaveSpawned = false;
    private List<ActionQueue> pendingPlayerActions = new List<ActionQueue>();
    public PlayerEntity CurrentActivePlayer =>
        PlayerTeamManager.Instance != null ? PlayerTeamManager.Instance.GetMemberAt(currentTeamMemberIndex) : null;

    private async void Start()
    {
        EnsureTeamReady();

        ApplyPhaseStats();
        SyncSharedPlayerSkillPoints();

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
        combatID = await NetworkManager.GetLatestCombatID();
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
        
        await NetworkManager.SaveCombatSkillLoadoutLogs(new CombatSkillLoadoutLogs
        {
            CombatID = combatID,
            SkillLoadut = skillLogs
        });
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
                    enemyCombat.Highlight(Color.white);
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
                Loader.Load(Loader.Scenes.Overworld);
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
            enemy.Highlight(Color.white);
        }
        
        if (PlayerTeamManager.Instance != null)
        {
            foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
            {
                if (member != null) member.Highlight(Color.white);
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
                ApplyPhaseStats();
                SyncSharedPlayerSkillPoints();
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

    private Entity GetActualTargetForEnemy(Entity enemyEntity)
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
                return alivePlayers[Random.Range(0, alivePlayers.Count)];
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
                
                ActionPointUsed = skill?.SkillPoint ?? 0,
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
            if (entity is PlayerEntity)
            {
                entity.buffController.OnTurnStart(entity, log);
                if (entity.CanAction() == true)
                {
                    bool canUseSkill = useSharedPlayerSkillPointPool
                        ? TryConsumeSharedPlayerSkillPoints(skill.SkillPoint)
                        : entity.CurrentSP >= skill.SkillPoint;

                    if (canUseSkill)
                    {
                        if (!useSharedPlayerSkillPointPool)
                        {
                            entity.SetSP(-skill.SkillPoint);
                        }

                        switch (skill.TargetType)
                        {
                            case TargetType.Self:
                                entity.skillManager.UseSkill(skill, entity, log);
                                break;
                            case TargetType.Enemy:
                                if (skill.TargetCount == TargetCount.Single)
                                {
                                    if (target != null && target.CurrentHealth > 0)
                                    {
                                        entity.skillManager.UseSkill(skill, target, log);
                                    }
                                    else
                                    {
// // Debug.Log("Target is already dead. Action cancelled.");
                                    }
                                }
                                else if (skill.TargetCount == TargetCount.All)
                                {
                                    bool hasValidTarget = false;
                                    EnemyCombat primaryTarget = target as EnemyCombat;
                                    bool primaryAssignedFromFallback = false;

                                    foreach (var t in GetAllEnemies())
                                    {
                                        EnemyCombat enemyTarget = t.GetComponent<EnemyCombat>();
                                        if (enemyTarget != null && !enemyTarget.IsDead())
                                        {
                                            bool allowWindSpread = false;
                                            if (primaryTarget != null)
                                            {
                                                allowWindSpread = enemyTarget == primaryTarget;
                                            }
                                            else if (!primaryAssignedFromFallback)
                                            {
                                                // Fallback: if no explicit primary exists, allow only the first alive target.
                                                allowWindSpread = true;
                                                primaryAssignedFromFallback = true;
                                            }

                                            entity.skillManager.UseSkill(skill, enemyTarget, log, allowWindSpread);
                                            hasValidTarget = true;
                                        }
                                    }
                                    
                                    if (!hasValidTarget)
                                    {
// // Debug.Log("No valid targets left for AoE skill. Action cancelled.");
                                    }
                                }
                                break;
                            case TargetType.Ally:
                                if (skill.TargetCount == TargetCount.All)
                                {
                                    bool hasAllyTarget = false;
                                    if (PlayerTeamManager.Instance != null)
                                    {
                                        foreach (var ally in PlayerTeamManager.Instance.GetAliveMembers())
                                        {
                                            entity.skillManager.UseSkill(skill, ally, log);
                                            hasAllyTarget = true;
                                        }
                                    }

                                    if (!hasAllyTarget)
                                    {
                                        entity.skillManager.UseSkill(skill, entity, log);
                                    }
                                }
                                else
                                {
                                    if (target != null && target.CurrentHealth > 0)
                                    {
                                        entity.skillManager.UseSkill(skill, target, log);
                                    }
                                    else
                                    {
                                        entity.skillManager.UseSkill(skill, entity, log);
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
// // Debug.Log("Not enough SP to use " + skill.name);
                    }
                }
                entity.buffController.OnTurnEnd(entity);
                PlayerActionQueueUI?.SetActionQueue(currentAction);
            }
            else if (entity is EnemyCombat enemyCombat)
            {
                if (enemyCombat != null) {
                    enemyCombat.buffController.OnTurnStart(enemyCombat, log);
                }
                // enemyCombat.buffController.OnTurnStart(enemyCombat, log);
                if (entity.CanAction() == true)
                {
                    switch (skill.TargetType)
                    {
                        case TargetType.Self:
                            // Boss targets itself for heals/buffs
                            enemyCombat.skillManager.UseSkill(skill, enemyCombat, log);
                            break;
                        case TargetType.Enemy:
                            // Boss targets the player (or active player)
                            Entity enemyTarget = target;
                            if (enemyTarget == null || enemyTarget.CurrentHealth <= 0)
                            {
                                enemyTarget = GetActualTargetForEnemy(enemyCombat);
                            }

                            if (enemyTarget != null && enemyTarget.CurrentHealth > 0)
                            {
                                enemyCombat.skillManager.UseSkill(skill, enemyTarget, log);
                            }
                            else
                            {
// // Debug.Log("Player is dead. Enemy action cancelled.");
                            }
                            break;
                        case TargetType.Ally:
                            // Treat ally casts as self for now
                            enemyCombat.skillManager.UseSkill(skill, enemyCombat, log);
                            break;
                    }
                }
                enemyCombat.buffController.OnTurnEnd(enemyCombat);
                EnemyActionQueueUI?.SetActionQueue(currentAction);
            }
            // actionQueue.RemoveAt(0);
            actionQueue.Remove(currentAction);
            ShowLog(log);
            NetworkManager.SaveCombatActionLogs(log);
            // SaveLogJson(log);
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
        RestoreSkillPointsByPhase();

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
// // Debug.Log("You Win!");
        CombatResultUI.Instance.gameObject.SetActive(true);
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

    public int GetCurrentPhase()
    {
        // Determine phase based on global game progression instead of wave
        if (userData != null)
        {
            return userData.GamePhase;
        }
        return 1; // Default to Phase 1 if UserData is not assigned
    }

    public float GetMaxWave()
    {
        return wave;
    }

    private void RestoreSkillPointsByPhase()
    {
        int currentPhase = GetCurrentPhase();
        int spToRestore = 0;

        switch (currentPhase)
        {
            case 1:
                spToRestore = phase1SPRestore;
                break;
            case 2:
                spToRestore = phase2SPRestore;
                break;
            case 3:
                spToRestore = phase3SPRestore;
                break;
            default:
                spToRestore = 0;
                break;
        }

        if (spToRestore > 0)
        {
            if (useSharedPlayerSkillPointPool)
            {
                RestoreSharedPlayerSkillPoints(spToRestore);
            }
            else
            {
                // Restore SP to alive players
                if (PlayerTeamManager.Instance != null)
                {
                    foreach (var member in PlayerTeamManager.Instance.GetAliveMembers())
                    {
                        member.SetSP(spToRestore);
                    }
                }
            }

            // Restore SP to all alive enemies
            foreach (GameObject enemyObj in GetAllEnemies())
            {
                EnemyCombat enemy = enemyObj.GetComponent<EnemyCombat>();
                if (enemy != null && !enemy.IsDead())
                {
                    enemy.SetSP(spToRestore);
// // Debug.Log($"[Phase {currentPhase}] {enemy.gameObject.name} restored {spToRestore} SP at end of turn");
                }
            }
        }
    }

    private void EnsureTeamReady()
    {
        if (PlayerTeamManager.Instance != null && PlayerTeamManager.Instance.ActiveTeamMembers.Count == 0)
        {
            PlayerTeamManager.Instance.SpawnTeam();
        }
    }

    private void SyncSharedPlayerSkillPoints()
    {
        if (!useSharedPlayerSkillPointPool) return;
        int currentShared = GetSharedPlayerCurrentSkillPoints();
        SetSharedPlayerSkillPoints(currentShared);
    }

    private bool TryConsumeSharedPlayerSkillPoints(int cost)
    {
        if (!useSharedPlayerSkillPointPool) return false;
        int currentShared = GetSharedPlayerCurrentSkillPoints();
        if (currentShared < cost) return false;

        SetSharedPlayerSkillPoints(currentShared - cost);
        return true;
    }

    private void RestoreSharedPlayerSkillPoints(int amount)
    {
        int currentShared = GetSharedPlayerCurrentSkillPoints();
        SetSharedPlayerSkillPoints(currentShared + amount);
    }

    private int GetSharedPlayerCurrentSkillPoints()
    {
        if (PlayerTeamManager.Instance == null) return 0;

        foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
        {
            if (member != null)
            {
                return member.CurrentSP;
            }
        }

        return 0;
    }

    private int GetSharedPlayerMaxSkillPoints()
    {
        if (PlayerTeamManager.Instance == null) return 0;

        int minMaxSp = int.MaxValue;
        foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
        {
            if (member == null) continue;
            int memberMaxSp = Mathf.RoundToInt(member.GetStat(StatType.MaxSkillPoint));
            minMaxSp = Mathf.Min(minMaxSp, memberMaxSp);
        }

        return minMaxSp == int.MaxValue ? 0 : minMaxSp;
    }

    private void SetSharedPlayerSkillPoints(int value)
    {
        if (PlayerTeamManager.Instance == null) return;

        int maxShared = GetSharedPlayerMaxSkillPoints();
        int clampedShared = Mathf.Clamp(value, 0, maxShared);

        foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
        {
            if (member == null) continue;
            int delta = clampedShared - member.CurrentSP;
            member.SetSP(delta);
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

    private void ApplyPhaseStats()
    {
        int phase = GetCurrentPhase();
        if (PlayerTeamManager.Instance == null || PlayerTeamManager.Instance.ActiveTeamMembers.Count == 0) return;

        // Apply to all active players
        foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
        {
            if (member == null || member.Stats == null) continue;

            switch (phase)
            {
                case 1:
                    member.Stats.SetBase(StatType.MaxHealth, 1000);
                    member.Stats.SetBase(StatType.MaxSkillPoint, 100);
                    break;
                case 2:
                    member.Stats.SetBase(StatType.MaxHealth, 2000);
                    member.Stats.SetBase(StatType.MaxSkillPoint, 200);
                    break;
                case 3:
                    member.Stats.SetBase(StatType.MaxHealth, 3000);
                    member.Stats.SetBase(StatType.MaxSkillPoint, 300);
                    break;
                default:
                    member.Stats.SetBase(StatType.MaxHealth, 3000);
                    member.Stats.SetBase(StatType.MaxSkillPoint, 300);
                    break;
            }
        }

        // Heal to full HP/SP to reflect the new max values if it's the start of battle
        // Or if it's a tutorial quest and not the last wave
        bool isTutorial = false;
        if (EnemyGenerator.Instance != null)
        {
            QuestEnemies currentQuest = EnemyGenerator.Instance.GetCurrentQuest();
            if (currentQuest != null && currentQuest.isTutorial)
            {
                isTutorial = true;
            }
        }

        bool isStartOfBattle = (currentWave == 1);
        
        if (isStartOfBattle || isTutorial)
        {
            foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
            {
                if (member == null) continue;
                member.Heal(member.GetStat(StatType.MaxHealth));
                member.SetSP((int)member.GetStat(StatType.MaxSkillPoint));
            }
        }
    }
    private void ShowLog(CombatActionLog log)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // 1. หัวข้อ: [Turn X] ใคร ทำอะไร
        sb.Append($"<color=#4EC9B0>{log.CasterName}</color> ใช้ <b>{log.SkillName}</b> ➜ ");

        // 2. รวบรวมผลลัพธ์ (Damage, Heal, Buff) มาต่อท้ายในบรรทัดเดียว
        bool hasEffect = false;

        // เช็ค Damage
        if (log.DamageEffectLogs != null && log.DamageEffectLogs.Count > 0)
        {
            foreach (var dmg in log.DamageEffectLogs)
            {
                sb.Append($"[<color=red>{dmg.AppliedTarget} -{dmg.Damage}</color>] ");
            }
            hasEffect = true;
        }

        // เช็ค Heal
        if (log.HealEffectLogs != null && log.HealEffectLogs.Count > 0)
        {
            foreach (var heal in log.HealEffectLogs)
            {
                sb.Append($"[<color=green>{heal.AppliedTarget} +{heal.HealAmount}</color>] ");
            }
            hasEffect = true;
        }

        // เช็ค Buff/Debuff
        if (log.BuffEffectLogs != null && log.BuffEffectLogs.Count > 0)
        {
            foreach (var buff in log.BuffEffectLogs)
            {
                sb.Append($"[<color=yellow>{buff.AppliedTarget} {buff.Buff}</color>] ");
            }
            hasEffect = true;
        }

        if (!hasEffect) sb.Append("<i>(ไม่มีผลกระทบ)</i>");

        // ปริ้นท์ออก Console บรรทัดเดียวจบ!
        Debug.Log(sb.ToString());
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
