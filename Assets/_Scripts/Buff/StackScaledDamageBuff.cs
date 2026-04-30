using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StackScaledDamageBuff", menuName = "ScriptableObjects/Buff/StackScaledDamageBuff")]
public class StackScaledDamageBuff : Buff
{
    [Header("Stack Source")]
    [Tooltip("Optional source buff asset. If assigned, its BuffName is used to read stacks.")]
    public Buff StackSourceBuff;

    [Tooltip("Fallback source buff name when StackSourceBuff is not assigned.")]
    public string StackSourceBuffName = "Frenzy";

    [Header("Damage Scaling")]
    [Tooltip("Every X stacks grants one bonus step.")]
    [Min(1)] public int StacksPerStep = 1;

    [Tooltip("Flat bonus damage granted per step.")]
    public float BonusDamagePerStep = 50f;

    private const string ModifierStateKey = "StackScaledDamageBuff_Modifier";

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);

        if (owner == null)
        {
            return;
        }

        if (buffState.CustomState.TryGetValue(ModifierStateKey, out object existingObj) && existingObj is StackScaledDamageModifier)
        {
            return;
        }

        StackScaledDamageModifier modifier = new StackScaledDamageModifier(owner, this);
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

        if (buffState.CustomState.TryGetValue(ModifierStateKey, out object modifierObj) && modifierObj is StackScaledDamageModifier modifier)
        {
            owner.RemoveModifier(modifier, EntityModifierType.Outgoing);
            buffState.CustomState.Remove(ModifierStateKey);
        }
    }

    private ActiveBuff GetSourceBuff(Entity owner)
    {
        if (owner == null || owner.buffController == null)
        {
            return null;
        }

        string sourceName = null;
        if (StackSourceBuff != null && !string.IsNullOrWhiteSpace(StackSourceBuff.BuffName))
        {
            sourceName = StackSourceBuff.BuffName;
        }
        else if (!string.IsNullOrWhiteSpace(StackSourceBuffName))
        {
            sourceName = StackSourceBuffName;
        }

        if (string.IsNullOrWhiteSpace(sourceName))
        {
            return null;
        }

        return owner.buffController.GetBuffByName(sourceName);
    }

    private int GetBonusStepCount(Entity owner)
    {
        ActiveBuff sourceBuff = GetSourceBuff(owner);
        if (sourceBuff == null || sourceBuff.CurrentStack <= 0)
        {
            return 0;
        }

        int stacksPerStep = Mathf.Max(1, StacksPerStep);
        return sourceBuff.CurrentStack / stacksPerStep;
    }

    private class StackScaledDamageModifier : IDamageModifier
    {
        private readonly Entity owner;
        private readonly StackScaledDamageBuff buffData;

        public StackScaledDamageModifier(Entity owner, StackScaledDamageBuff buffData)
        {
            this.owner = owner;
            this.buffData = buffData;
        }

        public void Modify(DamageCtx ctx)
        {
            if (ctx == null || buffData == null || owner == null)
            {
                return;
            }

            if (ctx.Caster != owner)
            {
                return;
            }

            int steps = buffData.GetBonusStepCount(owner);
            if (steps <= 0)
            {
                return;
            }

            ctx.Damage.Amount += buffData.BonusDamagePerStep * steps;
        }
    }
}