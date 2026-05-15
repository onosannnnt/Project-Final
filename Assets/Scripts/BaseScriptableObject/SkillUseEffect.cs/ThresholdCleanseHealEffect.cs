using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ThresholdCleanseHealEffect", menuName = "ScriptableObjects/SkillEffect/ThresholdCleanseHealEffect")]
public class ThresholdCleanseHealEffect : SkillEffect
{
    [SerializeField] private float baseHealAmount = 200f;
    [SerializeField] private float thresholdHealAmount = 600f;
    [SerializeField, Range(0f, 1f)] private float hpThreshold = 0.3f; // 30% HP

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        if (target == null) return false;

        float maxHP = target.GetStat(StatType.MaxHealth);
        if (maxHP <= 0) return false;

        float hpRatio = target.CurrentHealth / maxHP;
        float finalHeal = hpRatio <= hpThreshold ? thresholdHealAmount : baseHealAmount;

        // Cleanse negative effects if below threshold
        if (hpRatio <= hpThreshold)
        {
            List<ActiveBuff> negativeEffects = target.buffController.GetNegativeEffects();
            foreach (var effect in negativeEffects)
            {
                target.buffController.RemoveBuff(effect);
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
