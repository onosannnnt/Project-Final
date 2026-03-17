using UnityEngine;

public class ResistanceModifier : IDamageModifier
{
    public void Modify(DamageCtx ctx)
    {
        float resist = ctx.Damage.Type switch
        {
            DamageType.Physical => ctx.Target.GetStat(StatType.Armour),
            DamageType.Fire => ctx.Target.GetStat(StatType.FireResistance),
            DamageType.Cold => ctx.Target.GetStat(StatType.ColdResistance),
            DamageType.Lightning => ctx.Target.GetStat(StatType.LightningResistance),
            _ => 0f
        };

        resist = Mathf.Clamp(resist, -1f, 0.70f);

        ctx.Damage.Amount *= 1f - resist;
    }
}