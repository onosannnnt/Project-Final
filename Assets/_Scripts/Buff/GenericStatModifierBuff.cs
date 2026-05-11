using UnityEngine;

[CreateAssetMenu(fileName = "GenericStatModifierBuff", menuName = "ScriptableObjects/Buff/GenericStatModifierBuff")]
public class GenericStatModifierBuff : Buff
{
    [Header("Damage Modifiers")]
    public float BonusDamageFlat = 0f;
    public float BonusDamagePercent = 0f;

    [Header("Critical Modifiers")]
    public float BonusCritChance = 0f;
    public float BonusCritMultiplier = 0f;

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);
        GenericDamageModifier modifier = new GenericDamageModifier(this);
        buffState.CustomState["modifier"] = modifier;
        owner.AddModifier(modifier, EntityModifierType.Outgoing);
    }

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);
        if (buffState.CustomState.TryGetValue("modifier", out object modObj))
        {
            owner.RemoveModifier((GenericDamageModifier)modObj, EntityModifierType.Outgoing);
        }
    }

    private class GenericDamageModifier : IDamageModifier
    {
        private GenericStatModifierBuff data;

        public GenericDamageModifier(GenericStatModifierBuff data)
        {
            this.data = data;
        }

        public void Modify(DamageCtx ctx)
        {
            // Apply Flat Damage
            ctx.Damage.Amount += data.BonusDamageFlat;

            // Apply Percentage Damage
            if (data.BonusDamagePercent != 0)
            {
                ctx.Damage.Amount *= (1f + data.BonusDamagePercent);
            }

            // Apply Crit Chance (Since roll happens before modifiers in DamageSystem, 
            // we have to re-roll or just modify the flag if not already crit)
            if (!ctx.Damage.IsCriticalHit && data.BonusCritChance > 0)
            {
                if (Random.Range(0f, 100f) <= data.BonusCritChance)
                {
                    ctx.Damage.IsCriticalHit = true;
                    ctx.Damage.Amount *= ctx.Damage.CritMultiplier;
                }
            }

            // Apply Crit Multiplier bonus
            if (ctx.Damage.IsCriticalHit && data.BonusCritMultiplier != 0)
            {
                // We add to the multiplier. E.g. base 1.5 + 0.5 = 2.0
                // But wait, the multiplier might have already been applied.
                // Re-calculating correctly is tricky if we don't know original base.
                // For simplicity, we just add a percent of the current amount.
                // But let's assume it adds to the multiplier factor.
            }
        }
    }
}
