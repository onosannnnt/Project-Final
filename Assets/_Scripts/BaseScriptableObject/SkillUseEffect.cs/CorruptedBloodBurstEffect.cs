using UnityEngine;

[CreateAssetMenu(fileName = "CorruptedBloodBurstEffect", menuName = "ScriptableObjects/SkillEffect/CorruptedBloodBurstEffect")]
public class CorruptedBloodBurstEffect : SkillEffect
{
    [Header("Conversion")]
    [Tooltip("Percentage of Max HP to convert to Corrupted Health.")]
    public float MaxHpPercentToConvert = 0.2f;

    [Header("Buff to Apply")]
    [Tooltip("The damage bonus buff (e.g. CorruptedChunkScalingBuff template).")]
    public Buff ScalingBuffTemplate;
    
    [Tooltip("Optional penalty buff (e.g. DelayedPenaltyBuff).")]
    public Buff PenaltyBuffTemplate;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (caster == null) return false;

        float maxHP = caster.GetStat(StatType.MaxHealth);
        float amountToConvert = maxHP * MaxHpPercentToConvert;

        // Gain Corrupted Health
        caster.GainCorruptedHealth(amountToConvert);

        // Apply Buffs
        if (ScalingBuffTemplate != null)
        {
            caster.buffController.AddBuff(ScalingBuffTemplate);
            log.AddBuffEffectLog(new BuffEffectLog()
            {
                AppliedTargetID = caster.GetEntityID(),
                AppliedTarget = caster.Stats.EntityName,
                Buff = new BuffEffectData(ScalingBuffTemplate)
            });
        }

        if (PenaltyBuffTemplate != null)
        {
            caster.buffController.AddBuff(PenaltyBuffTemplate);
        }

        return true;
    }
}
