using UnityEngine;

[CreateAssetMenu(fileName = "SharedPainDebuff", menuName = "ScriptableObjects/Buff/SharedPainDebuff")]
public class SharedPainDebuff : Buff
{
    // The debuff target will take a % of damage this entity receives.
    [HideInInspector] public Entity LinkedCaster;

    // Stored as ratio (0.3 = 30%).
    [HideInInspector] public float RedirectRatio = 0.3f;

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);

        if (LinkedCaster == null)
        {
            return;
        }

        // Prevent duplicate modifiers on reapply/refresh.
        if (buffState.CustomState.TryGetValue("linkedCaster", out object oldCasterObj) &&
            buffState.CustomState.TryGetValue("modifier", out object oldModifierObj) &&
            oldCasterObj is Entity oldCaster && oldModifierObj is DamageShareModifier oldModifier)
        {
            oldCaster.RemoveModifier(oldModifier, EntityModifierType.Incoming);
        }

        DamageShareModifier modifier = new DamageShareModifier(owner, Mathf.Clamp01(RedirectRatio));
        buffState.CustomState["modifier"] = modifier;
        buffState.CustomState["linkedCaster"] = LinkedCaster;

        LinkedCaster.AddModifier(modifier, EntityModifierType.Incoming);
    }

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);

        if (buffState.CustomState.TryGetValue("linkedCaster", out object casterObj) &&
            buffState.CustomState.TryGetValue("modifier", out object modifierObj) &&
            casterObj is Entity linkedCaster && modifierObj is DamageShareModifier modifier)
        {
            linkedCaster.RemoveModifier(modifier, EntityModifierType.Incoming);
        }
    }

    private class DamageShareModifier : IDamageModifier
    {
        private readonly Entity mirrorTarget;
        private readonly float redirectRatio;

        public DamageShareModifier(Entity mirrorTarget, float redirectRatio)
        {
            this.mirrorTarget = mirrorTarget;
            this.redirectRatio = redirectRatio;
        }

        public void Modify(DamageCtx ctx)
        {
            if (mirrorTarget == null || ctx == null || ctx.Damage == null)
            {
                return;
            }

            // If mirror target is the same entity receiving hit, skip.
            if (ctx.Target == mirrorTarget)
            {
                return;
            }

            float mirroredDamage = ctx.Damage.Amount * redirectRatio;
            if (mirroredDamage <= 0f)
            {
                return;
            }

            // Apply mirrored damage directly to the debuffed target.
            Damage mirror = new Damage(mirroredDamage, ctx.Damage.Element, false);
            mirrorTarget.TakeDamage(mirror);
        }
    }
}
