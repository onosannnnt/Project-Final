using UnityEngine;

[CreateAssetMenu(fileName = "CorruptedScalingDamageBuff", menuName = "ScriptableObjects/Buff/CorruptedScalingDamageBuff")]
public class CorruptedScalingDamageBuff : Buff
{
    [Header("Damage Scaling")]
    [Tooltip("How much flat damage is added for each point of Corrupted Health.")]
    public float DamagePerCorruptedHealth = 0.5f;

    private const string ModifierStateKey = "CorruptedScalingDamageBuff_Modifier";

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);

        if (owner == null) return;

        if (buffState.CustomState.TryGetValue(ModifierStateKey, out object existingObj) && existingObj is CorruptedScalingDamageModifier)
        {
            return;
        }

        CorruptedScalingDamageModifier modifier = new CorruptedScalingDamageModifier(owner, this);
        buffState.CustomState[ModifierStateKey] = modifier;
        owner.AddModifier(modifier, EntityModifierType.Outgoing);
    }

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);

        if (owner == null) return;

        if (buffState.CustomState.TryGetValue(ModifierStateKey, out object modifierObj) && modifierObj is CorruptedScalingDamageModifier modifier)
        {
            owner.RemoveModifier(modifier, EntityModifierType.Outgoing);
            buffState.CustomState.Remove(ModifierStateKey);
        }
    }

    private class CorruptedScalingDamageModifier : IDamageModifier
    {
        private readonly Entity owner;
        private readonly CorruptedScalingDamageBuff buffData;

        public CorruptedScalingDamageModifier(Entity owner, CorruptedScalingDamageBuff buffData)
        {
            this.owner = owner;
            this.buffData = buffData;
        }

        public void Modify(DamageCtx ctx)
        {
            if (ctx == null || buffData == null || owner == null) return;
            if (ctx.Caster != owner) return;

            float corruptedHealth = owner.CorruptedHealth;
            if (corruptedHealth <= 0) return;

            ctx.Damage.Amount += corruptedHealth * buffData.DamagePerCorruptedHealth;
        }
    }
}
