using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatActionProcessor : MonoBehaviour
{
    private TurnManager turnManager;

    public void Initialize(TurnManager manager)
    {
        turnManager = manager;
    }

    public void ExecuteAction(ActionQueue action, CombatActionLog log)
    {
        Entity entity = action.Caster;
        Skill skill = action.Skill;
        Entity target = action.Target;

        if (entity is PlayerEntity)
        {
            ExecutePlayerAction(entity, target, skill, log);
        }
        else if (entity is EnemyCombat enemy)
        {
            ExecuteEnemyAction(enemy, target, skill, log);
        }
    }

    private void ExecutePlayerAction(Entity entity, Entity target, Skill skill, CombatActionLog log)
    {
        int actualCost = turnManager.GetSkillCost(skill);
        bool canAfford = turnManager.UseSharedPlayerSkillPointPool
            ? turnManager.ResourceManager.GetSharedPlayerCurrentSkillPoints() >= actualCost
            : entity.CurrentSP >= actualCost;

        if (!canAfford) return;

        // Verify we have at least one valid target before spending resources
        if (!HasAnyValidTarget(entity, target, skill)) return;

        // Consume Resources
        if (turnManager.UseSharedPlayerSkillPointPool)
            turnManager.ResourceManager.TryConsumeSharedPlayerSkillPoints(actualCost);
        else
            entity.SetSP(-actualCost);

        // Log Recovery
        log.ActionPointRecovery = 0;

        if (skill.CorruptedHealthGain > 0)
        {
            entity.GainCorruptedHealth(skill.CorruptedHealthGain);
        }

        switch (skill.TargetType)
        {
            case TargetType.Self:
                entity.skillManager.UseSkill(skill, entity, log);
                break;
            case TargetType.Enemy:
                ApplySkillToEnemies(entity, target, skill, log);
                break;
            case TargetType.Ally:
                ApplySkillToAllies(entity, target, skill, log);
                break;
        }
    }

    private void ExecuteEnemyAction(EnemyCombat enemy, Entity target, Skill skill, CombatActionLog log)
    {
        int actualCost = turnManager.GetSkillCost(skill);
        if (enemy.CurrentSP < actualCost)
        {
            Debug.Log($"{enemy.gameObject.name} not enough SP for {skill.skillName}");
            return;
        }

        // Verify target
        if (!HasAnyValidTarget(enemy, target, skill)) return;

        enemy.SetSP(-actualCost);
        log.ActionPointRecovery = 0;

        if (skill.CorruptedHealthGain > 0) enemy.GainCorruptedHealth(skill.CorruptedHealthGain);

        switch (skill.TargetType)
        {
            case TargetType.Self:
                enemy.skillManager.UseSkill(skill, enemy, log);
                break;
            case TargetType.Enemy:
                ApplyEnemySkillToPlayers(enemy, target, skill, log);
                break;
            case TargetType.Ally:
                enemy.skillManager.UseSkill(skill, enemy, log);
                break;
        }
    }

    private bool HasAnyValidTarget(Entity caster, Entity primaryTarget, Skill skill)
    {
        if (skill.TargetType == TargetType.Self) return true;

        if (skill.TargetCount == TargetCount.Single)
        {
            return primaryTarget != null && primaryTarget.CurrentHealth > 0;
        }
        else if (skill.TargetCount == TargetCount.All)
        {
            if (skill.TargetType == TargetType.Enemy)
            {
                if (caster is PlayerEntity)
                    return turnManager.GetAliveEnemiesInBattleOrder().Count > 0;
                else
                    return PlayerTeamManager.Instance != null && PlayerTeamManager.Instance.GetAliveMembers().Count > 0;
            }
            else if (skill.TargetType == TargetType.Ally)
            {
                if (caster is PlayerEntity)
                    return PlayerTeamManager.Instance != null && PlayerTeamManager.Instance.GetAliveMembers().Count > 0;
                else
                    return turnManager.GetAliveEnemiesInBattleOrder().Count > 0;
            }
        }
        return false;
    }

    private void ApplySkillToEnemies(Entity caster, Entity target, Skill skill, CombatActionLog log)
    {
        if (skill.TargetCount == TargetCount.Single)
        {
            if (target != null && target.CurrentHealth > 0)
            {
                caster.skillManager.UseSkill(skill, target, log);
            }
        }
        else if (skill.TargetCount == TargetCount.All)
        {
            // Fix 5: Cache targets to stabilize AOE loop
            List<EnemyCombat> targetList = turnManager.GetAliveEnemiesInBattleOrder();
            EnemyCombat primaryTarget = target as EnemyCombat;
            bool primaryAssignedFromFallback = false;

            foreach (var enemyObj in targetList)
            {
                if (enemyObj == null || enemyObj.CurrentHealth <= 0) continue;

                bool allowWindSpread = false;
                if (primaryTarget != null)
                {
                    allowWindSpread = enemyObj == primaryTarget;
                }
                else if (!primaryAssignedFromFallback)
                {
                    allowWindSpread = true;
                    primaryAssignedFromFallback = true;
                }

                caster.skillManager.UseSkill(skill, enemyObj, log, allowWindSpread);
            }
        }
    }

    private void ApplySkillToAllies(Entity caster, Entity target, Skill skill, CombatActionLog log)
    {
        if (skill.TargetCount == TargetCount.All)
        {
            if (PlayerTeamManager.Instance != null)
            {
                // Fix 5: Cache targets
                List<Entity> targetList = PlayerTeamManager.Instance.GetAliveMembers().ConvertAll(p => (Entity)p);
                if (targetList.Count > 0)
                {
                    foreach (var ally in targetList)
                    {
                        if (ally != null && ally.CurrentHealth > 0)
                            caster.skillManager.UseSkill(skill, ally, log);
                    }
                }
                else
                {
                    caster.skillManager.UseSkill(skill, caster, log);
                }
            }
        }
        else
        {
            if (target != null && target.CurrentHealth > 0)
            {
                caster.skillManager.UseSkill(skill, target, log);
            }
            else
            {
                caster.skillManager.UseSkill(skill, caster, log);
            }
        }
    }

    private void ApplyEnemySkillToPlayers(EnemyCombat enemy, Entity target, Skill skill, CombatActionLog log)
    {
        Entity actualTarget = target;
        if (actualTarget == null || actualTarget.CurrentHealth <= 0)
        {
            actualTarget = turnManager.GetActualTargetForEnemy(enemy);
        }

        if (actualTarget == null || actualTarget.CurrentHealth <= 0) return;

        if (skill.TargetCount == TargetCount.Single)
        {
            enemy.skillManager.UseSkill(skill, actualTarget, log);
        }
        else if (skill.TargetCount == TargetCount.All)
        {
            if (PlayerTeamManager.Instance != null)
            {
                // Fix 5: Cache targets
                List<Entity> targetList = PlayerTeamManager.Instance.GetAliveMembers().ConvertAll(p => (Entity)p);
                foreach (var player in targetList)
                {
                    if (player != null && player.CurrentHealth > 0)
                        enemy.skillManager.UseSkill(skill, player, log);
                }
            }
        }
    }
}
