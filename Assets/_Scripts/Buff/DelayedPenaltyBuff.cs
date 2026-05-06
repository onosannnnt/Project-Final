using UnityEngine;

[CreateAssetMenu(fileName = "DelayedPenaltyBuff", menuName = "ScriptableObjects/Buff/DelayedPenaltyBuff")]
public class DelayedPenaltyBuff : Buff
{
    [Header("Penalty")]
    [Tooltip("Percentage of CURRENT HP to lose when the buff ends (e.g. 0.2 for 20%).")]
    public float CurrentHpPenaltyPercent = 0.2f;

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);
        if (owner == null) return;

        float penalty = owner.CurrentHealth * CurrentHpPenaltyPercent;
        if (penalty > 0)
        {
            // Direct HP loss that doesn't trigger usual damage effects if possible, 
            // but using TakeDamage for consistency with UI/Log.
            Damage dmg = new Damage(penalty, DamageElement.None);
            owner.TakeDamage(dmg);
        }
    }
}
