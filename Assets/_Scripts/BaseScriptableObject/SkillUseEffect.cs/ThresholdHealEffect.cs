using UnityEngine;

[CreateAssetMenu(fileName = "ThresholdHealEffect", menuName = "ScriptableObjects/SkillEffect/ThresholdHealEffect")]
public class ThresholdHealEffect : SkillEffect
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
