using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StackScaledHealEffect", menuName = "ScriptableObjects/SkillEffect/StackScaledHealEffect")]
public class StackScaledHealEffect : SkillEffect
{
    [Header("Scaling Settings")]
    [SerializeField] private Buff stackSourceBuff;
    [SerializeField] private float baseHeal = 100f;
    [SerializeField] private float healPerStack = 50f;
    [SerializeField, Tooltip("0 means no limit")] private int maxScalingStacks = 0;
    
    [Header("Targeting")]
    [SerializeField] private bool healParty = false;
    
    [Header("Threshold Bonus")]
    [SerializeField] private bool useThresholdBonus = false;
    [SerializeField, Range(0f, 1f)] private float hpThreshold = 0.3f;
    [SerializeField] private float bonusMultiplier = 1.5f;

    [Header("Consumption")]
    [SerializeField] private bool consumeStacks = false;
    [SerializeField] private int stacksToConsume = 0; // 0 means all

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (caster == null || stackSourceBuff == null) return false;

        ActiveBuff activeStacks = caster.buffController.GetBuffByName(stackSourceBuff.BuffName);
        int stackCount = activeStacks != null ? activeStacks.CurrentStack : 0;

        int scalingStacks = maxScalingStacks > 0 ? Mathf.Min(stackCount, maxScalingStacks) : stackCount;
        float calculatedHeal = baseHeal + (scalingStacks * healPerStack);

        if (healParty)
        {
            List<Entity> allies = GetAliveAllies(caster);
            foreach (var ally in allies)
            {
                ApplyHeal(caster, ally, calculatedHeal, log);
            }
        }
        else
        {
            if (target == null) return false;
            ApplyHeal(caster, target, calculatedHeal, log);
        }

        if (consumeStacks && activeStacks != null)
        {
            int toConsume = stacksToConsume == 0 ? activeStacks.CurrentStack : stacksToConsume;
            caster.buffController.ConsumeBuffStack(activeStacks, toConsume);
        }

        return true;
    }

    private void ApplyHeal(Entity caster, Entity target, float amount, CombatActionLog log)
    {
        float finalHeal = amount;
        if (useThresholdBonus)
        {
            float maxHP = target.GetStat(StatType.MaxHealth);
            if (maxHP > 0 && (target.CurrentHealth / maxHP) <= hpThreshold)
            {
                finalHeal *= bonusMultiplier;
            }
        }

        target.Heal(finalHeal);
        log.AddHealEffectLog(new HealEffectLog()
        {
            AppliedTarget = target.Stats.EntityName,
            AppliedTargetID = target.GetEntityID(),
            HealAmount = finalHeal
        });
    }

    private List<Entity> GetAliveAllies(Entity caster)
    {
        if (caster is PlayerEntity)
        {
            return PlayerTeamManager.Instance != null ? PlayerTeamManager.Instance.GetAliveMembers().ConvertAll(p => (Entity)p) : new List<Entity> { caster };
        }
        // Enemy logic if needed, but usually for players
        return new List<Entity> { caster };
    }
}
