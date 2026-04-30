using UnityEngine;

[CreateAssetMenu(fileName = "TeamDamageNoHealBuff", menuName = "ScriptableObjects/Buff/TeamDamageNoHealBuff")]
public class TeamDamageNoHealBuff : Buff
{
    [Header("Damage Bonus")]
    [Tooltip("Flat extra damage added to all outgoing damage while this buff is active.")]
    public float BonusDamage = 50f;

    private const string ModifierStateKey = "TeamDamageNoHealBuff_OutgoingModifier";

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);

        if (owner == null)
        {
            return;
        }

        if (buffState.CustomState.TryGetValue(ModifierStateKey, out object existingObj) && existingObj is ExtraDamageModifier)
        {
            return;
        }

        ExtraDamageModifier modifier = new ExtraDamageModifier(BonusDamage);
        buffState.CustomState[ModifierStateKey] = modifier;
        owner.AddModifier(modifier, EntityModifierType.Outgoing);
    }

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);

        if (owner == null)
        {
            return;
        }

        if (buffState.CustomState.TryGetValue(ModifierStateKey, out object modifierObj) && modifierObj is ExtraDamageModifier modifier)
        {
            owner.RemoveModifier(modifier, EntityModifierType.Outgoing);
            buffState.CustomState.Remove(ModifierStateKey);
        }
    }

    private class ExtraDamageModifier : IDamageModifier
    {
        private readonly float bonusDamage;

        public ExtraDamageModifier(float bonusDamage)
        {
            this.bonusDamage = bonusDamage;
        }

        public void Modify(DamageCtx ctx)
        {
            if (ctx == null)
            {
                return;
            }

            ctx.Damage.Amount += bonusDamage;
        }
    }
}