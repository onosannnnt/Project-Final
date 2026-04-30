using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableObjects/SkillEffect/CrimsonStrike", menuName = "ScriptableObjects/SkillEffect/BloodStrikeEffect")]
public class BloodStrikeEffect : SkillEffect
{
    [Tooltip("Percentage of Max HP to be sacrificed when using the skill (e.g., 0.10 for 10%)")]
    public float maxHpCostPercentage = 0.10f; // 10%

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        // 1. หักเลือดผู้ร่าย (10% ของ Max HP)
        int hpCost = Mathf.RoundToInt(caster.GetStat(StatType.MaxHealth) * maxHpCostPercentage);
        
        // ป้องกันไม่ให้ตายจากการใช้สกิล (เหลือ 1 HP)
        if (caster.CurrentHealth <= hpCost)
        {
            hpCost = Mathf.Max(0, Mathf.FloorToInt(caster.CurrentHealth - 1f));
        }

        if (hpCost > 0) 
        {
            Damage selfDamage = new Damage((float)hpCost, DamageElement.Physical);
            caster.TakeDamage(selfDamage); 
        }
        
        return true;
    }
}
