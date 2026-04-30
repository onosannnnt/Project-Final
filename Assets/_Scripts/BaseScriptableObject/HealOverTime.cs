using UnityEngine;

[CreateAssetMenu(fileName = "HealOverTimeEffect", menuName = "ScriptableObjects/Buff/HealEffectOverTime")]

public class HealOverTimeEffect : Buff
{
    [Tooltip("Base Heal amount applied each turn")]
    public float BaseHeal;

    public override void OnTurnStart(Entity owner, CombatActionLog log, ActiveBuff buffState)
    {
        float totalHeal = 0f;

        if (StackCalculationType == StackMultiplierType.Linear)
        {
            totalHeal = BaseHeal * buffState.CurrentStack;
        }
        else if (StackCalculationType == StackMultiplierType.DiminishingReturn)
        {
            float effectiveStack = (float)buffState.CurrentStack / (buffState.CurrentStack + Mathf.Max(Threshold, 1));
            totalHeal = BaseHeal * effectiveStack;
        }
        
        // Add +- 15% variance per tick!
        float variance = Random.Range(0.85f, 1.15f);
        totalHeal *= variance;

// // Debug.Log($"{owner.gameObject.name} heals {totalHeal} HP from {name} HOT.");
        log.AddHealEffectLog(new HealEffectLog
        {
            AppliedTargetID = owner.GetEntityID(),
            AppliedTarget = owner.Stats.EntityName,
            HealAmount = totalHeal
        });
        owner.Heal(totalHeal);
    }
}
