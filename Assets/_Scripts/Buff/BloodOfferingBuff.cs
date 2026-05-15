using UnityEngine;

[CreateAssetMenu(fileName = "BloodOfferingBuff", menuName = "ScriptableObjects/Buff/BloodOfferingBuff")]
public class BloodOfferingBuff : Buff
{
    public float PassiveDamagePer10Percent = 0.03f;
    public float ConsumedBonusDamage = 0.3f;
    public float IncomingDamagePenalty = 0.2f;

    private const string ModifierKey = "BloodOfferingMod";
    private const string DidConsumeKey = "DidConsume";

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);
        BloodOfferingModifier modifier = new BloodOfferingModifier(owner, this, buffState);
        buffState.CustomState[ModifierKey] = modifier;
        owner.AddModifier(modifier, EntityModifierType.Outgoing);
        owner.AddModifier(modifier, EntityModifierType.Incoming);
    }

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);
        if (buffState.CustomState.TryGetValue(ModifierKey, out object mod) && mod is BloodOfferingModifier modifier)
        {
            owner.RemoveModifier(modifier, EntityModifierType.Outgoing);
            owner.RemoveModifier(modifier, EntityModifierType.Incoming);
            
            if (buffState.CustomState.TryGetValue("CasterEntity", out object casterObj) && casterObj is Entity caster && caster != owner)
            {
                caster.RemoveModifier(modifier, EntityModifierType.Incoming);
            }
            
            buffState.CustomState.Remove(ModifierKey);
        }
    }

    private class BloodOfferingModifier : IDamageModifier
    {
        private Entity owner;
        private BloodOfferingBuff data;
        private ActiveBuff state;

        public BloodOfferingModifier(Entity owner, BloodOfferingBuff data, ActiveBuff state) {
            this.owner = owner; this.data = data; this.state = state;
        }

        public void Modify(DamageCtx ctx)
        {
            if (ctx.Caster == owner) { // Outgoing
                float maxHP = owner.GetStat(StatType.MaxHealth);
                float ratio = owner.CorruptedHealth / maxHP;
                float multiplier = 1f + (Mathf.FloorToInt(ratio * 10f) * data.PassiveDamagePer10Percent);
                
                if (state.CustomState.ContainsKey(DidConsumeKey)) multiplier += data.ConsumedBonusDamage;
                ctx.Damage.Amount *= multiplier;
            }
            
            Entity casterEntity = owner;
            if (state.CustomState.TryGetValue("CasterEntity", out object casterObj) && casterObj is Entity caster)
            {
                casterEntity = caster;
            }
            
            if (ctx.Target == casterEntity) { // Incoming
                ctx.Damage.Amount *= (1f + data.IncomingDamagePenalty);
            }
        }
    }
}
