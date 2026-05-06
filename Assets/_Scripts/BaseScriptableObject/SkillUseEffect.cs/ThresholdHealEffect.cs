using UnityEngine;

[CreateAssetMenu(fileName = "ThresholdHealEffect", menuName = "ScriptableObjects/SkillEffect/ThresholdHealEffect")]
public class ThresholdHealEffect : SkillEffect
{
    [SerializeField] private float healAmount = 500f;
    [SerializeField, Range(0f, 1f)] private float hpThreshold = 0.3f; // 30% HP

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (target == null) return false;

        float maxHP = target.GetStat(StatType.MaxHealth);
        if (maxHP <= 0) return false;

        float hpRatio = target.CurrentHealth / maxHP;

        if (hpRatio <= hpThreshold)
        {
            target.Heal(healAmount);
            log.AddHealEffectLog(new HealEffectLog()
            {
                AppliedTarget = target.Stats.EntityName,
                AppliedTargetID = target.GetEntityID(),
                HealAmount = healAmount
            });
            return true;
        }

        // If threshold not met, we still return true but don't heal?
        // Or return false to indicate effect didn't trigger. 
        // Returning true because the "skill execution" was successful, just no effect applied.
        return true;
    }
}
