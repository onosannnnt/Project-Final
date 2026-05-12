using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableObjects/SkillEffect/CrimsonStrike", menuName = "ScriptableObjects/SkillEffect/BloodStrikeEffect")]
public class BloodStrikeEffect : SkillEffect
{
    [Tooltip("Percentage of Max HP to be sacrificed when using the skill (e.g., 0.10 for 10%)")]
    public float maxHpCostPercentage = 0.10f; // 10%

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        // 1. หักเลือดผู้ร่าย (10% ของ Max HP)
        int hpCost = Mathf.RoundToInt(caster.GetStat(StatType.MaxHealth) * maxHpCostPercentage);
        
        if (hpCost > 0) 
        {
            // Use PreventDeath = true to ensure player doesn't suicide
            Damage selfDamage = new Damage((float)hpCost, DamageElement.Physical, false, 0, 1.5f, true);
            caster.TakeDamage(selfDamage); 
        }
        
        return true;
    }
}
