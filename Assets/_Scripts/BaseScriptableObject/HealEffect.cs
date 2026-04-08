
using UnityEngine;

[CreateAssetMenu(fileName = "HealEffect", menuName = "ScriptableObjects/SkillEffect/HealEffect")]
public class HealEffect : SkillEffect
{
    [SerializeField] public float BaseHeal;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
// // Debug.Log(caster.gameObject.name + " used HealEffect on " + target.gameObject.name);
        
        float finalHeal = BaseHeal;

        // // Debug.Log($"Base Heal: {BaseHeal}, Final Heal: {finalHeal}");

        caster.Heal(finalHeal);
        log.AddHealEffectLog(new HealEffectLog()
        {
            AppliedTarget = caster.Stats.EntityName,
            AppliedTargetID = caster.GetEntityID(),
            HealAmount = finalHeal
        });
        return true;
    }
}
