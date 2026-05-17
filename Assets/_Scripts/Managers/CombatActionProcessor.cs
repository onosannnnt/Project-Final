using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatActionProcessor : MonoBehaviour
{
    private TurnManager turnManager;
    [SerializeField] private Buff momentumBuffTemplate;

    public void Initialize(TurnManager manager)
    {
        turnManager = manager;
    }

    public void ExecuteAction(ActionQueue action, CombatActionLog log)
    {
        Entity entity = action.Caster;
        Skill skill = action.Skill;
        Entity target = action.Target;

        // --- Season Matching & Momentum Logic ---
        if (WeatherManager.Instance != null)
        {
            WeatherType current = WeatherManager.Instance.CurrentWeather;
            bool matches = false;

            // Check if any effect in the skill matches the current weather
            foreach (var effect in skill.SkillEffects)
            {
                if (effect is SeasonalDamageEffect sde && sde.RequiredWeather == current)
                {
                    matches = true;
                    break;
                }
                if (effect is SeasonalSupportEffect sse && sse.RequiredWeather == current)
                {
                    matches = true;
                    break;
                }
            }

            if (matches)
            {
                entity.SeasonMatchStreak++;
                Debug.Log($"[Weather Match] {entity.gameObject.name} streak: {entity.SeasonMatchStreak}/3 (Weather: {current})");

                if (entity.SeasonMatchStreak >= 3)
                {
                    if (momentumBuffTemplate != null)
                    {
                        Debug.Log($"[MOMENTUM GAINED] {entity.gameObject.name} has reached a streak of 3 and gained Momentum!");
                        entity.buffController.AddBuff(momentumBuffTemplate);
                        log.AddBuffEffectLog(new BuffEffectLog()
                        {
                            AppliedTargetID = entity.GetEntityID(),
                            AppliedTarget = entity.Stats.EntityName,
                            Buff = new BuffEffectData(momentumBuffTemplate)
                        });
                    }
                    entity.SeasonMatchStreak = 0; // Reset after granting
                }
            }
            else
            {
                if (entity.SeasonMatchStreak > 0)
                {
                    Debug.Log($"[Streak Broken] {entity.gameObject.name} failed to match weather. Streak reset to 0.");
                }
                entity.SeasonMatchStreak = 0;
            }
        }

        if (entity is PlayerEntity)
        {
            ExecutePlayerAction(entity, target, skill, log);
        }
        else if (entity is EnemyCombat enemy)
        {
            ExecuteEnemyAction(enemy, target, skill, log);
        }

        // --- Repeat Action AFM Logic ---
        CheckAndHandleRepeatAction(entity, target, skill, log);
    }

    private void CheckAndHandleRepeatAction(Entity entity, Entity target, Skill skill, CombatActionLog log)
    {
        if (entity == null || entity.buffController == null || skill == null) return;

        // Find the RepeatActionSkillBuff among active buffs
        foreach (var activeBuff in entity.buffController.GetAllBuffs())
        {
            if (activeBuff.Data is RepeatActionSkillBuff repeatBuff)
            {
                // Check if the skill is allowed to repeat and if the chance rolls successfully
                if (repeatBuff.CanRepeatSkill(skill) && repeatBuff.ShouldRepeat(activeBuff))
                {
                    Debug.Log($"[AFM] Repeat Action triggered for {entity.gameObject.name} using {skill.skillName}!");
                    
                    // Execute the skill again
                    // We call the inner execution methods directly to bypass momentum/streak logic for the repeat
                    if (entity is PlayerEntity)
                    {
                        // Note: We don't consume resources for the repeat action
                        List<Entity> targets = GetTargets(entity, target, skill);
                        if (targets.Count > 0)
                        {
                            entity.skillManager.UseSkill(skill, targets, log);
                        }
                    }
                    else if (entity is EnemyCombat enemy)
                    {
                        List<Entity> targets = GetTargets(enemy, target, skill);
                        if (targets.Count > 0)
                        {
                            enemy.skillManager.UseSkill(skill, targets, log);
                        }
                    }

                    // Handle buff consumption/removal if configured
                    repeatBuff.OnRepeatTriggered(entity, activeBuff);
                    
                    // We only allow one repeat per action to prevent potential infinite loops 
                    // (though ShouldRepeat is usually a roll, safety first)
                    break;
                }
            }
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
        List<Entity> targets = GetTargets(entity, target, skill);
        if (targets.Count == 0) return;

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

        entity.skillManager.UseSkill(skill, targets, log);
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
        List<Entity> targets = GetTargets(enemy, target, skill);
        if (targets.Count == 0) return;

        enemy.SetSP(-actualCost);
        log.ActionPointRecovery = 0;

        if (skill.CorruptedHealthGain > 0) enemy.GainCorruptedHealth(skill.CorruptedHealthGain);

        enemy.skillManager.UseSkill(skill, targets, log);
    }

    private List<Entity> GetTargets(Entity caster, Entity primaryTarget, Skill skill)
    {
        List<Entity> targets = new List<Entity>();

        switch (skill.TargetType)
        {
            case TargetType.Self:
                targets.Add(caster);
                break;

            case TargetType.Enemy:
                if (skill.TargetCount == TargetCount.Single)
                {
                    Entity actualTarget = primaryTarget;
                    if (caster is EnemyCombat enemy && (actualTarget == null || actualTarget.CurrentHealth <= 0))
                    {
                        actualTarget = turnManager.GetActualTargetForEnemy(enemy);
                    }
                    if (actualTarget != null && actualTarget.CurrentHealth > 0) targets.Add(actualTarget);
                }
                else
                {
                    if (caster is PlayerEntity)
                        targets.AddRange(turnManager.GetAliveEnemiesInBattleOrder());
                    else
                        targets.AddRange(PlayerTeamManager.Instance != null ? PlayerTeamManager.Instance.GetAliveMembers().ConvertAll(p => (Entity)p) : new List<Entity>());
                }
                break;

            case TargetType.Ally:
                if (skill.TargetCount == TargetCount.Single)
                {
                    Entity actualTarget = primaryTarget != null && primaryTarget.CurrentHealth > 0 ? primaryTarget : caster;
                    targets.Add(actualTarget);
                }
                else
                {
                    if (caster is PlayerEntity)
                        targets.AddRange(PlayerTeamManager.Instance != null ? PlayerTeamManager.Instance.GetAliveMembers().ConvertAll(p => (Entity)p) : new List<Entity>());
                    else
                        targets.AddRange(turnManager.GetAliveEnemiesInBattleOrder());
                }
                break;
        }

        return targets;
    }

    private bool HasAnyValidTarget(Entity caster, Entity primaryTarget, Skill skill)
    {
        return GetTargets(caster, primaryTarget, skill).Count > 0;
    }
}
