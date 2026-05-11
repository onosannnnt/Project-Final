using UnityEngine;

[CreateAssetMenu(fileName = "MomentumBuff", menuName = "ScriptableObjects/Buff/MomentumBuff")]
public class MomentumBuff : Buff
{
    [Range(0f, 2f)] public float DamageMultiplier = 0.4f; // 40% bonus

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);
        MomentumModifier modifier = new MomentumModifier(owner, DamageMultiplier);
        buffState.CustomState["modifier"] = modifier;
        owner.AddModifier(modifier, EntityModifierType.Outgoing);
    }

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);
        if (buffState.CustomState.TryGetValue("modifier", out object modifierObj))
        {
            owner.RemoveModifier((MomentumModifier)modifierObj, EntityModifierType.Outgoing);
        }
    }

    private class MomentumModifier : IDamageModifier
    {
        private float multiplier;
        private Entity owner;

        public MomentumModifier(Entity owner, float multiplier)
        {
            this.owner = owner;
            this.multiplier = multiplier;
        }

        public void Modify(DamageCtx ctx)
        {
            // Apply bonus
            ctx.Damage.Amount *= (1f + multiplier);
            
            // Remove buff after one attack
            ActiveBuff momentum = owner.buffController.GetBuffByName("Momentum");
            if (momentum != null) owner.buffController.RemoveBuff(momentum);
        }
    }
}
