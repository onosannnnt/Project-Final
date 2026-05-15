using UnityEngine;

[CreateAssetMenu(fileName = "CorruptedChunkReductionEffect", menuName = "ScriptableObjects/SkillEffect/CorruptedChunkReductionEffect")]
public class CorruptedChunkReductionEffect : SkillEffect
{
    [SerializeField, Range(0f, 1f)] private float percentOfCorruptedToReduce = 0.15f;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        if (caster == null || target == null) return false;

        float currentCorrupted = target.CorruptedHealth;
        if (currentCorrupted <= 0) return false;

        float amountToReduce = currentCorrupted * percentOfCorruptedToReduce;
        target.RemoveCorruptedHealth(amountToReduce);

        return true;
    }
}
