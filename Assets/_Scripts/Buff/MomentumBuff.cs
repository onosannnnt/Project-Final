using UnityEngine;

[CreateAssetMenu(fileName = "MomentumBuff", menuName = "ScriptableObjects/Buff/MomentumBuff")]
public class MomentumBuff : Buff
{
    [Range(0f, 2f)] public float DamageMultiplier = 0.4f; // 40% bonus

    private void Awake()
    {
        BuffName = "Momentum";
        isStackable = false;
        MaxStack = 1;
        isStealable = false;
        isPermanent = true; // Stays until consumed
    }

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
            // Only CE attack skills can consume Momentum to gain more damage and bonus effect.
            if (ctx.Style != SkillStyle.CE) return;

            // Apply bonus
            ctx.Damage.Amount *= (1f + multiplier);
        }
    }
}
