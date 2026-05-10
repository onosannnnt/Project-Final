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
    
    [Header("Threshold Bonus")]
    [SerializeField] private bool useThresholdBonus = false;
    [SerializeField, Range(0f, 1f)] private float hpThreshold = 0.3f;
    [SerializeField] private float bonusMultiplier = 1.5f;

    [Header("Consumption")]
    [SerializeField] private bool consumeStacks = false;
    [SerializeField] private int stacksToConsume = 0; // 0 means all

    private float _precalculatedHeal;

    public override void OnSkillStarted(Entity caster, List<Entity> targets, CombatActionLog log)
    {
        if (caster == null || stackSourceBuff == null) return;

        ActiveBuff activeStacks = caster.buffController.GetBuffByName(stackSourceBuff.BuffName);
        int stackCount = activeStacks != null ? activeStacks.CurrentStack : 0;

        int scalingStacks = maxScalingStacks > 0 ? Mathf.Min(stackCount, maxScalingStacks) : stackCount;
        _precalculatedHeal = baseHeal + (scalingStacks * healPerStack);

        if (consumeStacks && activeStacks != null)
        {
            int toConsume = stacksToConsume == 0 ? activeStacks.CurrentStack : stacksToConsume;
            caster.buffController.ConsumeBuffStack(activeStacks, toConsume);
        }
    }

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (target == null) return false;

        float finalHeal = _precalculatedHeal;
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

        return true;
    }
}
