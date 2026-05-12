using UnityEngine;

[CreateAssetMenu(fileName = "CorruptedChunkHealEffect", menuName = "ScriptableObjects/SkillEffect/CorruptedChunkHealEffect")]
public class CorruptedChunkHealEffect : SkillEffect
{
    [SerializeField, Range(0f, 1f)] private float corruptedReductionPercent = 1.0f;
    [SerializeField] private float baseHeal = 200f;
    [SerializeField] private float healPer10PercentCorruptedReduced = 100f;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        if (caster == null || target == null) return false;

        float currentCorrupted = caster.CorruptedHealth;
        if (currentCorrupted <= 0) return false;

        float maxHP = caster.GetStat(StatType.MaxHealth);
        float corruptedToReduce = currentCorrupted * corruptedReductionPercent;
        
        // Calculate chunks of 10% relative to Max HP
        int chunksOf10Percent = Mathf.FloorToInt((corruptedToReduce / maxHP) * 10f);

        caster.RemoveCorruptedHealth(corruptedToReduce);

        float finalHeal = baseHeal + (chunksOf10Percent * healPer10PercentCorruptedReduced);
        target.Heal(finalHeal);

        log.AddHealEffectLog(new HealEffectLog()
        {
            AppliedTarget = target.Stats.EntityName,
            AppliedTargetID = target.GetEntityID(),
            HealAmount = finalHeal
        });

        return true;
    }
}
