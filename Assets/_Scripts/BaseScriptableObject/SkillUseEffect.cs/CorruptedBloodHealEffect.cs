using UnityEngine;

[CreateAssetMenu(fileName = "CorruptedBloodHealEffect", menuName = "ScriptableObjects/SkillEffect/CorruptedBloodHealEffect")]
public class CorruptedBloodHealEffect : SkillEffect
{
    [SerializeField, Tooltip("How much to heal for each point of Corrupted Health consumed.")] 
    private float healMultiplier = 1.0f;
    
    [SerializeField, Tooltip("If true, consumes all current Corrupted Health. If false, uses Max Consume Amount.")]
    private bool consumeAll = true;
    
    [SerializeField, Tooltip("Maximum amount of Corrupted Health to consume if Consume All is false.")]
    private float maxConsumeAmount = 0f;

    [SerializeField, Tooltip("If true, the caster consumes their own Corrupted Health to heal the target.")]
    private bool consumeFromCaster = true;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        Entity source = consumeFromCaster ? caster : target;
        if (source == null || target == null) return false;

        float currentCorrupted = source.CorruptedHealth;
        if (currentCorrupted <= 0) return false;

        float amountToConsume = consumeAll ? currentCorrupted : Mathf.Min(currentCorrupted, maxConsumeAmount);
        
        if (amountToConsume > 0)
        {
            source.RemoveCorruptedHealth(amountToConsume);
            float healAmount = amountToConsume * healMultiplier;
            target.Heal(healAmount);

            log.AddHealEffectLog(new HealEffectLog()
            {
                AppliedTarget = target.Stats.EntityName,
                AppliedTargetID = target.GetEntityID(),
                HealAmount = healAmount
            });

            return true;
        }

        return false;
    }
}
