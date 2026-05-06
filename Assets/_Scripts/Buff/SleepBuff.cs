using UnityEngine;

[CreateAssetMenu(fileName = "SleepBuff", menuName = "ScriptableObjects/Buff/SleepBuff")]
public class SleepBuff : Buff
{
    private void OnEnable()
    {
        buffType = BuffType.CrowdControl;
    }

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);
        // Sleep usually breaks on damage, but we'll keep it simple for now
        // or we could hook into OnTakeDamage if the system supports it.
    }
}
