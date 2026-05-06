using UnityEngine;

[CreateAssetMenu(fileName = "StackScaledGlobalDamageBuff", menuName = "ScriptableObjects/Buff/StackScaledGlobalDamageBuff")]
public class StackScaledGlobalDamageBuff : Buff
{
    [Header("Scaling")]
    public Buff StackSourceBuff;
    public float BonusDamagePerStack = 10f;

    private const string ModifierKey = "GlobalStackDamageModifier";
    private const string SourceKey = "GlobalStackSource";

    public void SetSource(ActiveBuff state, Entity source)
    {
        state.CustomState[SourceKey] = source;
    }

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);
        
        GlobalStackModifier modifier = new GlobalStackModifier(owner, this, buffState);
        buffState.CustomState[ModifierKey] = modifier;
        owner.AddModifier(modifier, EntityModifierType.Outgoing);
    }

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);

        if (buffState.CustomState.TryGetValue(ModifierKey, out object obj) && obj is GlobalStackModifier modifier)
        {
            owner.RemoveModifier(modifier, EntityModifierType.Outgoing);
            buffState.CustomState.Remove(ModifierKey);
        }
    }

    private class GlobalStackModifier : IDamageModifier
    {
        private Entity owner;
        private StackScaledGlobalDamageBuff data;
        private ActiveBuff buffState;

        public GlobalStackModifier(Entity owner, StackScaledGlobalDamageBuff data, ActiveBuff buffState)
        {
            this.owner = owner;
            this.data = data;
            this.buffState = buffState;
        }

        public void Modify(DamageCtx ctx)
        {
            if (ctx.Caster != owner) return;

            Entity source = null;
            if (buffState.CustomState.TryGetValue(SourceKey, out object obj))
            {
                source = obj as Entity;
            }

            // Fallback to owner if no source (scales with own stacks)
            Entity finalSource = source != null ? source : owner;

            if (finalSource.buffController != null && data.StackSourceBuff != null)
            {
                ActiveBuff sourceBuff = finalSource.buffController.GetBuffByName(data.StackSourceBuff.BuffName);
                int stacks = sourceBuff != null ? sourceBuff.CurrentStack : 0;
                ctx.Damage.Amount += stacks * data.BonusDamagePerStack;
            }
        }
    }
}
