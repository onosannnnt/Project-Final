using UnityEngine;

[CreateAssetMenu(fileName = "CorruptedChunkScalingBuff", menuName = "ScriptableObjects/Buff/CorruptedChunkScalingBuff")]
public class CorruptedChunkScalingBuff : Buff
{
    [Header("Scaling")]
    [Tooltip("Percentage bonus damage gained for every 10% of Max HP that is Corrupted (e.g. 0.1 for 10% bonus).")]
    public float DamageBonusPer10PercentCorrupted = 0.1f;

    private const string ModifierKey = "CorruptedChunkScalingModifier";

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);
        if (owner == null) return;

        CorruptedChunkScalingModifier modifier = new CorruptedChunkScalingModifier(owner, this);
        buffState.CustomState[ModifierKey] = modifier;
        owner.AddModifier(modifier, EntityModifierType.Outgoing);
    }

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);
        if (owner == null) return;

        if (buffState.CustomState.TryGetValue(ModifierKey, out object obj) && obj is CorruptedChunkScalingModifier modifier)
        {
            owner.RemoveModifier(modifier, EntityModifierType.Outgoing);
            buffState.CustomState.Remove(ModifierKey);
        }
    }

    private class CorruptedChunkScalingModifier : IDamageModifier
    {
        private Entity owner;
        private CorruptedChunkScalingBuff data;

        public CorruptedChunkScalingModifier(Entity owner, CorruptedChunkScalingBuff data)
        {
            this.owner = owner;
            this.data = data;
        }

        public void Modify(DamageCtx ctx)
        {
            if (ctx.Caster != owner) return;

            float maxHP = owner.GetStat(StatType.MaxHealth);
            if (maxHP <= 0) return;

            float corruptedRatio = owner.CorruptedHealth / maxHP;
            int chunksOf10Percent = Mathf.FloorToInt(corruptedRatio * 10f);

            if (chunksOf10Percent > 0)
            {
                float multiplier = 1f + (chunksOf10Percent * data.DamageBonusPer10PercentCorrupted);
                ctx.Damage.Amount *= multiplier;
            }
        }
    }
}
