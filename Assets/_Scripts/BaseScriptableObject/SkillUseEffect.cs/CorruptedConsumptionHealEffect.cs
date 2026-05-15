using UnityEngine;

[CreateAssetMenu(fileName = "CorruptedConsumptionHealEffect", menuName = "ScriptableObjects/SkillEffect/CorruptedConsumptionHealEffect")]
public class CorruptedConsumptionHealEffect : SkillEffect
{
    [SerializeField, Range(0f, 1f)] private float percentOfMaxHPToConsume = 0.2f;
    [SerializeField] private float baseHeal = 200f;
    [SerializeField] private float healPer10PercentConsumed = 80f;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        if (caster == null || target == null) return false;

        float maxHP = target.GetStat(StatType.MaxHealth);
        float amountToConsume = maxHP * percentOfMaxHPToConsume;
        
        float actualBefore = target.CorruptedHealth;
        // Consume up to the requested amount
        bool consumedAny = target.TryConsumeCorruptedHealth(Mathf.Min(amountToConsume, actualBefore));
        float actualConsumed = actualBefore - target.CorruptedHealth;

        // Calculate healing: Base + (80 * every 10% of Max HP consumed)
        float chunksConsumed = actualConsumed / (maxHP * 0.1f);
        float finalHeal = baseHeal + (chunksConsumed * healPer10PercentConsumed);

        if (finalHeal > 0)
        {
            target.Heal(finalHeal);
            log.AddHealEffectLog(new HealEffectLog()
            {
                AppliedTargetID = target.GetEntityID(),
                AppliedTarget = target.Stats.EntityName,
                HealAmount = finalHeal
            });
        }

        return true;
    }
}
