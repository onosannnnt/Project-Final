using UnityEngine;

[CreateAssetMenu(fileName = "DelayedPenaltyBuff", menuName = "ScriptableObjects/Buff/DelayedPenaltyBuff")]
public class DelayedPenaltyBuff : Buff
{
    [Header("Penalty")]
    [Tooltip("Percentage of CURRENT HP to lose when the buff ends (e.g. 0.2 for 20%).")]
    public float CurrentHpPenaltyPercent = 0.2f;

    [Header("Bonus")]
    [Tooltip("Percentage bonus to outgoing damage while active (e.g. 0.2 for 20%).")]
    public float OutgoingDamageBonusPercent = 0.2f;

    private const string ModifierKey = "DelayedPenaltyModifier";

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);
        if (owner == null) return;

        DelayedDamageModifier modifier = new DelayedDamageModifier(owner, this);
        buffState.CustomState[ModifierKey] = modifier;
        owner.AddModifier(modifier, EntityModifierType.Outgoing);
    }

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);
        if (owner == null) return;

        // 1. Remove the damage modifier
        if (buffState.CustomState.TryGetValue(ModifierKey, out object obj) && obj is DelayedDamageModifier modifier)
        {
            owner.RemoveModifier(modifier, EntityModifierType.Outgoing);
            buffState.CustomState.Remove(ModifierKey);
        }

        // 2. Apply HP penalty
        float penalty = owner.CurrentHealth * CurrentHpPenaltyPercent;
        if (penalty > 0)
        {
            owner.TakeDamage(new Damage(penalty, DamageElement.None));
        }
    }

    private class DelayedDamageModifier : IDamageModifier
    {
        private Entity owner;
        private DelayedPenaltyBuff data;

        public DelayedDamageModifier(Entity owner, DelayedPenaltyBuff data)
        {
            this.owner = owner;
            this.data = data;
        }

        public void Modify(DamageCtx ctx)
        {
            if (ctx.Caster != owner) return;
            ctx.Damage.Amount *= (1f + data.OutgoingDamageBonusPercent);
        }
    }
}
