using UnityEngine;

[CreateAssetMenu(fileName = "DamageBoostBuff", menuName = "ScriptableObjects/Buff/DamageBoostBuff")]
public class DamageBoostBuff : Buff
{
    public float BonusDamage = 300f;
    private ExtraDamageModifier modifier;

    public override void OnApply(Entity owner)
    {
        base.OnApply(owner);
        modifier = new ExtraDamageModifier(BonusDamage);
        owner.AddModifier(modifier, EntityModifierType.Outgoing);
    }

    public override void OnRemove(Entity owner)
    {
        base.OnRemove(owner);
        if (modifier != null)
        {
            owner.RemoveModifier(modifier, EntityModifierType.Outgoing);
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
