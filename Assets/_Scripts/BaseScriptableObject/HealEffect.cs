
using UnityEngine;

[CreateAssetMenu(fileName = "HealEffect", menuName = "ScriptableObjects/SkillEffect/HealEffect")]
public class HealEffect : SkillEffect
{
    [SerializeField] public float BaseHeal;

    public override void Execute(Entity caster, Entity target, CombatActionLog log)
    {
        Debug.Log(caster.gameObject.name + " used HealEffect on " + target.gameObject.name);
        
        // Add +- 15% variance
        float variance = Random.Range(0.85f, 1.15f);
        float finalHeal = BaseHeal * variance;

        Debug.Log($"Base Heal: {BaseHeal}, Variance: {variance}, Final Heal: {finalHeal}");

        caster.Heal(finalHeal);
        log.AddHealEffectLog(new HealEffectLog()
        {
            AppliedTarget = caster.Stats.EntityName,
            AppliedTargetID = caster.GetEntityID(),
            HealAmount = finalHeal
        });
    }
}