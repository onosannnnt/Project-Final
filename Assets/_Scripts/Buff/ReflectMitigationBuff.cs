using UnityEngine;

[CreateAssetMenu(fileName = "ReflectMitigationBuff", menuName = "ScriptableObjects/Buff/ReflectMitigationBuff")]
public class ReflectMitigationBuff : Buff
{
    [Header("Reflect & Mitigation")]
    [Tooltip("Percentage of incoming damage to reflect back to the attacker (e.g. 0.5 for 50%).")]
    public float ReflectPercent = 0.5f;
    
    [Tooltip("Percentage reduction of incoming damage (e.g. 0.3 for 30%).")]
    public float DamageReductionPercent = 0.3f;

    private const string ModifierKey = "ReflectMitigationModifier";

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);
        if (owner == null) return;

        ReflectModifier modifier = new ReflectModifier(owner, this);
        buffState.CustomState[ModifierKey] = modifier;
        owner.AddModifier(modifier, EntityModifierType.Incoming);
    }

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);
        if (owner == null) return;

        if (buffState.CustomState.TryGetValue(ModifierKey, out object obj) && obj is ReflectModifier modifier)
        {
            owner.RemoveModifier(modifier, EntityModifierType.Incoming);
            buffState.CustomState.Remove(ModifierKey);
        }
    }

    private class ReflectModifier : IDamageModifier
    {
        private Entity owner;
        private ReflectMitigationBuff data;

        public ReflectModifier(Entity owner, ReflectMitigationBuff data)
        {
            this.owner = owner;
            this.data = data;
        }

        public void Modify(DamageCtx ctx)
        {
            if (ctx.Target != owner) return;

            // 1. Mitigate
            float originalAmount = ctx.Damage.Amount;
            ctx.Damage.Amount *= (1f - data.DamageReductionPercent);

            // 2. Reflect (calculated from original or mitigated? prompt says "reflect X% damage", usually implies incoming)
            if (data.ReflectPercent > 0 && ctx.Caster != null && ctx.Caster != owner)
            {
                float reflectedAmount = originalAmount * data.ReflectPercent;
                Damage reflectDmg = new Damage(reflectedAmount, DamageElement.None); // Neutral reflect
                ctx.Caster.TakeDamage(reflectDmg);
            }
        }
    }
}
