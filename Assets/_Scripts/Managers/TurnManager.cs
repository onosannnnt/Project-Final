using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : Singleton<TurnManager>
{
    [SerializeField] private Transform worldParent;
    [SerializeField] private float wave = 3;
    [SerializeField] private ActionQueueUI PlayerActionQueueUI;
    [SerializeField] private ActionQueueUI EnemyActionQueueUI;
    [Header("Skill Point Restoration per Phase")]
    [SerializeField] private int phase1SPRestore = 10; // Restore X SP per turn in Phase 1
    [SerializeField] private int phase2SPRestore = 20; // Restore Y SP per turn in Phase 2
    [SerializeField] private int phase3SPRestore = 30; // Restore Z SP per turn in Phase 3
    private TurnState currentState;
    private List<ActionQueue> actionQueue = new List<ActionQueue>();
    private PlayerCombat playerCombat;
    public int turnRound = 0;
    public int combatID = 0;
    public int currentWave = 1;
    private async void Start()
    {
        playerCombat = PlayerCombat.instance;
        ApplyPhaseStats();

        if (EnemyGenerator.Instance != null && EnemyGenerator.Instance.GetCurrentQuest() != null)
        {
            wave = EnemyGenerator.Instance.GetCurrentQuest().waves.Length;
        }

        SetState(TurnState.PlayerTurnState);
        combatID = await NetworkManager.GetLatestCombatID();
        List<SkillLogs> skillLogs = new List<SkillLogs>();
        foreach (var skill in playerCombat.skillManager.GetSkills())
        {
            skillLogs.Add(new SkillLogs
            {
                SkillID = skill.skillID,
                SkillName = skill.skillName
            });
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
                HandlePlayerTurn();
                break;
            case TurnState.SpeedCompareState:
                List<GameObject> enemies = GetAllEnemies();
                if (enemies.Count == 0) return;
                foreach (GameObject enemy in enemies)
                {
                    EnemyCombat enemyCombat = enemy.GetComponent<EnemyCombat>();
                    playerCombat.SetPlayerState(PlayerActionState.Action);
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
    private void HandlePlayerTurn()
    {
        playerCombat.SetEnemyTarget(GetFirstEnemy());
        TargetingPanel.instance.SetEnemyTargetPanel(playerCombat.GetEnemyTarget());
        ActionBarUI.Instance.gameObject.SetActive(true);
        turnRound++;
        List<GameObject> remainingEnemies = GetAllEnemies();
        remainingEnemies.RemoveAll(enemy => enemy.GetComponent<EnemyCombat>().IsDead());
        if (remainingEnemies.Count == 0)
        {
            Debug.Log("Generating new enemies for wave " + currentWave);
            EnemyGenerator.Instance.GenerateEnemy();
            currentWave += 1;
            ApplyPhaseStats();
        }
    }

    private void HandleSpeedComparison()
    {
        ActionBarUI.Instance.gameObject.SetActive(false);
        SkillPanelUI.Instance.gameObject.SetActive(false);
        ActionQueue playerAction = new ActionQueue(playerCombat, playerCombat.GetEnemyTarget(), playerCombat.GetSelectedSkill, playerCombat.GetStat(StatType.ActionSpeed));
        // {
        //     Caster = playerCombat,
        //     Target = playerCombat.GetEnemyTarget(),
        //     Skill = playerCombat.GetSelectedSkill,
        //     ActionSpeed = playerCombat.GetStat(StatType.ActionSpeed)
        // };
        actionQueue.Add(playerAction);
        List<GameObject> enemies = GetAllEnemies();
        enemies.RemoveAll(enemy => enemy.GetComponent<EnemyCombat>().IsDead());
        foreach (GameObject enemy in enemies)
        {
            Entity enemyEntity = enemy.GetComponent<EnemyCombat>();
            ActionQueue enemyAction = new ActionQueue(enemyEntity, playerCombat, enemyEntity.skillManager.RandomSkill(), enemyEntity.Stats.ActionSpeed);
            // {
            //     Caster = enemyEntity,
            //     Target = playerCombat,
            //     Skill = enemyEntity.skillManager.RandomSkill(),
            //     ActionSpeed = enemyEntity.Stats.ActionSpeed
            // };
            actionQueue.Add(enemyAction);
        }
        actionQueue.Sort((a, b) => b.ActionSpeed.CompareTo(a.ActionSpeed));
        SetState(TurnState.ActionState);
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
            // Debug.Log(entity.gameObject.name + " is taking action with " + currentAction.ActionSpeed + " speed.");
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
            log.AddEntityLog(new EntityStatData(playerCombat));
            foreach (var enemy in GetAllEnemies())
            {
                var enemyEntity = enemy.GetComponent<EnemyCombat>();
                if (enemyEntity)
                {
                    log.AddEntityLog(new EntityStatData(enemyEntity));
                }
            }
            if (entity.CompareTag("Player"))
            {
                playerCombat.buffController.OnTurnStart(playerCombat, log);
                if (entity.CanAction() == true) playerCombat.Action(log);
                playerCombat.buffController.OnTurnEnd(playerCombat);
                PlayerActionQueueUI.SetActionQueue(currentAction);
            }
            else if (entity.CompareTag("Enemy"))
            {
                EnemyCombat enemyCombat = entity.GetComponent<EnemyCombat>();
                if (enemyCombat != null) {
                    enemyCombat.buffController.OnTurnStart(enemyCombat, log);
                }
                // enemyCombat.buffController.OnTurnStart(enemyCombat, log);
                if (entity.CanAction() == true) enemyCombat.skillManager.UseSkill(skill, playerCombat, log);
                enemyCombat.buffController.OnTurnEnd(enemyCombat);
                EnemyActionQueueUI.SetActionQueue(currentAction);
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
        if (playerCombat.CurrentHealth <= 0)
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

        SetState(TurnState.PlayerTurnState);
        yield break;
    }

    private void Win()
    {
        Debug.Log("You Win!");
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
        if (playerCombat != null && playerCombat.GetUserData() != null)
        {
            return playerCombat.GetUserData().GamePhase;
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
            // Restore SP to player
            playerCombat.SetSP(spToRestore);
            Debug.Log($"[Phase {currentPhase}] Player restored {spToRestore} SP at end of turn");

            // Restore SP to all alive enemies
            foreach (GameObject enemyObj in GetAllEnemies())
            {
                EnemyCombat enemy = enemyObj.GetComponent<EnemyCombat>();
                if (enemy != null && !enemy.IsDead())
                {
                    enemy.SetSP(spToRestore);
                    Debug.Log($"[Phase {currentPhase}] {enemy.gameObject.name} restored {spToRestore} SP at end of turn");
                }
            }
        }
    }

    private void ApplyPhaseStats()
    {
        int phase = GetCurrentPhase();
        if (playerCombat == null || playerCombat.Stats == null) return;

        switch (phase)
        {
            case 1:
                playerCombat.Stats.MaxHealth = 1000;
                playerCombat.Stats.MaxSkillPoint = 100;
                break;
            case 2:
                playerCombat.Stats.MaxHealth = 2000;
                playerCombat.Stats.MaxSkillPoint = 200;
                break;
            case 3:
                playerCombat.Stats.MaxHealth = 3000;
                playerCombat.Stats.MaxSkillPoint = 300;
                break;
            default:
                playerCombat.Stats.MaxHealth = 3000;
                playerCombat.Stats.MaxSkillPoint = 300;
                break;
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

        // currentWave already got incremented in HandlePlayerTurn, so if it's wave 1, we haven't started.
        // Wait, HandlePlayerTurn also calls this AFTER currentWave += 1, so the new wave is currentWave.
        // E.g. at start currentWave is 1. We heal.
        // After wave 1 finishes, currentWave becomes 2. We heal if we want to heal between waves.
        bool isStartOfBattle = (currentWave == 1);
        
        if (isStartOfBattle || isTutorial)
        {
            playerCombat.Heal(playerCombat.Stats.MaxHealth);
            playerCombat.SetSP(playerCombat.Stats.MaxSkillPoint);
            Debug.Log($"[Phase {phase}] Player stats updated and healed -> Max HP: {playerCombat.Stats.MaxHealth}, Max SP: {playerCombat.Stats.MaxSkillPoint}");
        }
        else
        {
            Debug.Log($"[Phase {phase}] Player stats updated -> Max HP: {playerCombat.Stats.MaxHealth}, Max SP: {playerCombat.Stats.MaxSkillPoint}");
        }
    }
    // private void ShowLog(CombatActionLog log)
    // {
    //     string jsonData = JsonUtility.ToJson(log, true);
    //     Debug.Log("ข้อมูล JSON คือ:\n" + jsonData);
    // }
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