using UnityEngine;

[CreateAssetMenu(fileName = "UnkillableLastStandBuff", menuName = "ScriptableObjects/Buff/UnkillableLastStandBuff")]
public class UnkillableLastStandBuff : Buff
{
    public float CorruptedHealthGainPercent = 0.3f;

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);
    }

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);
        if (owner == null) return;

        // Force HP to 1 by dealing damage equal to current - 1
        float hpToLose = owner.CurrentHealth - 1f;
        if (hpToLose > 0) {
            // Use None element to bypass resistances
            owner.TakeDamage(new Damage(hpToLose, DamageElement.None));
        }

        float maxHP = owner.GetStat(StatType.MaxHealth);
        owner.GainCorruptedHealth(maxHP * CorruptedHealthGainPercent);
    }
}
