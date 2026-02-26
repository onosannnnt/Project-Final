using UnityEngine;

public class CriticalModifier : IDamageModifier
{
    public void Modify(DamageCtx ctx)
    {
        float critChance = ctx.Caster.GetStat(StatType.CriticalHitChance);

        if (Random.value < critChance || ctx.IsCriticalHit)
        {
            ctx.IsCriticalHit = true;
            ctx.Damage.IsCriticalHit = true;

            float critMulti = ctx.Caster.GetStat(StatType.CriticalDamageMultiplier);
            ctx.Damage.Amount *= 1f + critMulti;
        }
    }
}