using UnityEngine;

[CreateAssetMenu(fileName = "CorruptedConsumptionHealEffect", menuName = "ScriptableObjects/SkillEffect/CorruptedConsumptionHealEffect")]
public class CorruptedConsumptionHealEffect : SkillEffect
{
    [SerializeField] private float percentOfMaxHPToConsume = 20f;
    [SerializeField] private float baseHeal = 200f;
    [SerializeField] private float healPer10PercentConsumed = 80f;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (caster == null || target == null) return false;

        float maxHP = caster.GetStat(StatType.MaxHealth);
        float amountToConsume = maxHP * (percentOfMaxHPToConsume / 100f);
        
        float actualBefore = caster.CorruptedHealth;
        // Consume up to the requested amount
        bool consumedAny = caster.TryConsumeCorruptedHealth(Mathf.Min(amountToConsume, actualBefore));
        float actualConsumed = actualBefore - caster.CorruptedHealth;

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
