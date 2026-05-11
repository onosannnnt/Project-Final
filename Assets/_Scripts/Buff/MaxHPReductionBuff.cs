using UnityEngine;

[CreateAssetMenu(fileName = "MaxHPReductionBuff", menuName = "ScriptableObjects/Buff/MaxHPReductionBuff")]
public class MaxHPReductionBuff : Buff
{
    [Range(0f, 1f)] public float ReductionPercent = 0.05f;

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);
        // We can use the existing StatModifier system or a custom modifier.
        // Since StatModifier in Buff is a List, we can just add a Percent modifier.
        // But to make it cleaner and potentially dynamic:
    }
}
