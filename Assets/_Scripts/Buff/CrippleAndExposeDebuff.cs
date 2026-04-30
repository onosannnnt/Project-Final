using UnityEngine;

[CreateAssetMenu(fileName = "CrippleAndExposeDebuff", menuName = "ScriptableObjects/Buff/CrippleAndExposeDebuff")]
public class CrippleAndExposeDebuff : Buff
{
    [Header("Incoming Damage Bonus")]
    [Tooltip("Flat extra damage this target takes from all incoming hits.")]
    public float ExtraDamageTaken = 200f;

    private const string ModifierStateKey = "CrippleAndExposeDebuff_IncomingModifier";

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);
        AttachOrUpdateModifier(owner, buffState);
    }

    public override void OnRefresh(Entity owner, ActiveBuff buffState)
    {
        base.OnRefresh(owner, buffState);
        AttachOrUpdateModifier(owner, buffState);
    }

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);

        if (owner == null)
        {
            return;
        }

        if (buffState.CustomState.TryGetValue(ModifierStateKey, out object modifierObj) && modifierObj is IncomingBonusDamageModifier modifier)
        {
            owner.RemoveModifier(modifier, EntityModifierType.Incoming);
            buffState.CustomState.Remove(ModifierStateKey);
        }
    }

    private void AttachOrUpdateModifier(Entity owner, ActiveBuff buffState)
    {
        if (owner == null)
        {
            return;
        }

        if (buffState.CustomState.TryGetValue(ModifierStateKey, out object existingObj) && existingObj is IncomingBonusDamageModifier existingModifier)
        {
            existingModifier.SetExtraDamage(ExtraDamageTaken);
            return;
        }

        IncomingBonusDamageModifier modifier = new IncomingBonusDamageModifier(ExtraDamageTaken);
        buffState.CustomState[ModifierStateKey] = modifier;
        owner.AddModifier(modifier, EntityModifierType.Incoming);
    }

    private class IncomingBonusDamageModifier : IDamageModifier
    {
        private float extraDamage;

        public IncomingBonusDamageModifier(float extraDamage)
        {
            this.extraDamage = extraDamage;
        }

        public void SetExtraDamage(float value)
        {
            extraDamage = value;
        }

        public void Modify(DamageCtx ctx)
        {
            if (ctx == null)
            {
                return;
            }

            ctx.Damage.Amount += extraDamage;
        }
    }
}