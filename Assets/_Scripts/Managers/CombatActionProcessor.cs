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
        bool canUseSkill = turnManager.UseSharedPlayerSkillPointPool
            ? turnManager.ResourceManager.TryConsumeSharedPlayerSkillPoints(actualCost)
            : entity.CurrentSP >= actualCost;

        if (!canUseSkill) return;

        if (!turnManager.UseSharedPlayerSkillPointPool)
        {
            entity.SetSP(-actualCost);
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

        enemy.SetSP(-actualCost);

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
            bool hasValidTarget = false;
            EnemyCombat primaryTarget = target as EnemyCombat;
            bool primaryAssignedFromFallback = false;

            foreach (var enemyObj in turnManager.GetAliveEnemiesInBattleOrder())
            {
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
                hasValidTarget = true;
            }
        }
    }

    private void ApplySkillToAllies(Entity caster, Entity target, Skill skill, CombatActionLog log)
    {
        if (skill.TargetCount == TargetCount.All)
        {
            if (PlayerTeamManager.Instance != null)
            {
                var aliveMembers = PlayerTeamManager.Instance.GetAliveMembers();
                if (aliveMembers.Count > 0)
                {
                    foreach (var ally in aliveMembers)
                    {
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
                foreach (var player in PlayerTeamManager.Instance.GetAliveMembers())
                {
                    enemy.skillManager.UseSkill(skill, player, log);
                }
            }
        }
    }
}
