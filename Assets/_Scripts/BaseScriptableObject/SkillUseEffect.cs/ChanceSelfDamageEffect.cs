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
            // Use PreventDeath = true to ensure player doesn't suicide
            Damage selfDamage = new Damage((float)selfDamageAmount, DamageElement.Physical, false, 0, 1.5f, true);
            caster.TakeDamage(selfDamage); 
        }
        
        return true;
    }
}
