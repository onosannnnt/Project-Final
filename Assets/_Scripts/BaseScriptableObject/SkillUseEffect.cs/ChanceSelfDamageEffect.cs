using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableObjects/SkillEffect/ChanceSelfDamage", menuName = "ScriptableObjects/SkillEffect/ChanceSelfDamageEffect")]
public class ChanceSelfDamageEffect : SkillEffect
{
    public float chance = 0.20f; // 20% chance
    public int selfDamageAmount = 50;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        // 20% chance to trigger
        if (Random.value < chance)
        {
            Damage selfDamage = new Damage((float)selfDamageAmount, DamageElement.Physical);
            caster.TakeDamage(selfDamage); 
        }
        
        return true;
    }
}
