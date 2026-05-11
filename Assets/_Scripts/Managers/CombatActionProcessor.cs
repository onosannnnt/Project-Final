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

        bool executed = false;

        if (entity is PlayerEntity)
        {
            executed = ExecutePlayerAction(entity, target, skill, log, consumeCost: true);
        }
        else if (entity is EnemyCombat enemy)
        {
            executed = ExecuteEnemyAction(enemy, target, skill, log, consumeCost: true);
        }

        if (executed)
        {
            TryRepeatActionFromBuff(entity, target, skill, log);
        }
    }

    private bool ExecutePlayerAction(Entity entity, Entity target, Skill skill, CombatActionLog log, bool consumeCost)
    {
        if (entity == null || skill == null)
        {
            return false;
        }

        int actualCost = turnManager.GetSkillCost(skill);
        bool canUseSkill = !consumeCost ||
            (turnManager.UseSharedPlayerSkillPointPool
                ? turnManager.ResourceManager.TryConsumeSharedPlayerSkillPoints(actualCost)
                : entity.CurrentSP >= actualCost);

        if (!canUseSkill) return false;

        if (consumeCost && !turnManager.UseSharedPlayerSkillPointPool)
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

        return true;
    }

    private bool ExecuteEnemyAction(EnemyCombat enemy, Entity target, Skill skill, CombatActionLog log, bool consumeCost)
    {
        if (enemy == null || skill == null)
        {
            return false;
        }

        int actualCost = turnManager.GetSkillCost(skill);
        if (consumeCost && enemy.CurrentSP < actualCost)
        {
            Debug.Log($"{enemy.gameObject.name} not enough SP for {skill.skillName}");
            return false;
        }

        if (consumeCost)
        {
            enemy.SetSP(-actualCost);
        }

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

        return true;
    }

    private void TryRepeatActionFromBuff(Entity caster, Entity target, Skill skill, CombatActionLog log)
    {
        if (caster == null || skill == null || caster.buffController == null)
        {
            return;
        }

        List<ActiveBuff> buffs = caster.buffController.GetAllBuffs();
        if (buffs == null || buffs.Count == 0)
        {
            return;
        }

        for (int i = 0; i < buffs.Count; i++)
        {
            ActiveBuff activeBuff = buffs[i];
            if (activeBuff?.Data is not RepeatActionSkillBuff repeatBuff)
            {
                continue;
            }

            if (!repeatBuff.CanRepeatSkill(skill))
            {
                continue;
            }

            if (!repeatBuff.ShouldRepeat(activeBuff))
            {
                continue;
            }

            repeatBuff.OnRepeatTriggered(caster, activeBuff);

            if (caster is PlayerEntity)
            {
                ExecutePlayerAction(caster, target, skill, log, consumeCost: false);
            }
            else if (caster is EnemyCombat enemy)
            {
                ExecuteEnemyAction(enemy, target, skill, log, consumeCost: false);
            }

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
