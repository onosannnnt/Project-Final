using UnityEngine;

[CreateAssetMenu(fileName = "DamageBoostBuff", menuName = "ScriptableObjects/Buff/DamageBoostBuff")]
public class DamageBoostBuff : Buff
{
    public float BonusDamage = 300f;

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);
        ExtraDamageModifier modifier = new ExtraDamageModifier(BonusDamage);
        buffState.CustomState["modifier"] = modifier;
        owner.AddModifier(modifier, EntityModifierType.Outgoing);
    }

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);
        if (buffState.CustomState.TryGetValue("modifier", out object modifierObj))
        {
            owner.RemoveModifier((ExtraDamageModifier)modifierObj, EntityModifierType.Outgoing);
        }
    }

    private class ExtraDamageModifier : IDamageModifier
    {
        private float bonus;

        public ExtraDamageModifier(float bonus)
        {
            this.bonus = bonus;
        }

        public void Modify(DamageCtx ctx)
        {
            ctx.Damage.Amount += bonus;
        }
    }
}
