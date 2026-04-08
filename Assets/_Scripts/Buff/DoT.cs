using System.Runtime.InteropServices;
using UnityEngine;

[CreateAssetMenu(fileName = "DOT", menuName = "ScriptableObjects/Buff/DOT")]

public class DOT : Buff
{
    [Tooltip("Base Damage to be dealt each turn")]
    public float BaseDamage;

    public override void OnTurnStart(Entity owner, CombatActionLog log, ActiveBuff buffState)
    {
        float totalDamage = 0f;

        if (StackCalculationType == StackMultiplierType.Linear)
        {
            totalDamage = BaseDamage * buffState.CurrentStack;
        }
        else if (StackCalculationType == StackMultiplierType.DiminishingReturn)
        {
            float effectiveStack = buffState.CurrentStack / (buffState.CurrentStack + Mathf.Max(Threshold, 1f));
            totalDamage = BaseDamage * effectiveStack;
        }

        // Add +- 15% variance per tick!
        float variance = Random.Range(0.85f, 1.15f);
        totalDamage *= variance;

        owner.StartCoroutine(ApplyDotDamageDelayed(owner, totalDamage, log));
    }

    private System.Collections.IEnumerator ApplyDotDamageDelayed(Entity owner, float totalDamage, CombatActionLog log)
    {
        yield return new WaitForSeconds(0.5f);

// // Debug.Log($"{owner.gameObject.name} takes {totalDamage} damage from {name} DOT.");
        Damage damage = new Damage(totalDamage, DamageElement.Dot);
        DamageCtx ctx = new DamageCtx(owner, owner, damage);
        DamageSystem.Process(ctx, log);
    }
}
