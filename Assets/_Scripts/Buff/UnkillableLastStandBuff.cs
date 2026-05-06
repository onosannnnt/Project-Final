using UnityEngine;

[CreateAssetMenu(fileName = "UnkillableLastStandBuff", menuName = "ScriptableObjects/Buff/UnkillableLastStandBuff")]
public class UnkillableLastStandBuff : Buff
{
    [Header("End of Effect")]
    [Tooltip("Target percentage of Max HP to drop to when the buff ends (e.g. 0.05 for 5%).")]
    public float HealthPercentAfter = 0.05f;
    
    [Tooltip("Amount of Corrupted Health to gain when the buff ends.")]
    public float CorruptedHealthGain = 500f;

    private void OnEnable()
    {
        // Buff definition prevents lethal damage while active
        preventLethalDamage = true;
    }

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);

        if (owner == null) return;

        // Gain Corrupted Blood
        owner.GainCorruptedHealth(CorruptedHealthGain);

        // Drop to critical HP
        float maxHP = owner.GetStat(StatType.MaxHealth);
        float targetHP = maxHP * HealthPercentAfter;
        
        // We deal damage to drop current health to targetHP
        // TakeDamage handles health clamping and events
        float currentHP = owner.CurrentHealth;
        if (currentHP > targetHP)
        {
            float damageToDeal = currentHP - targetHP;
            Damage dmg = new Damage(damageToDeal, DamageElement.None); 
            owner.TakeDamage(dmg);
        }
    }
}
