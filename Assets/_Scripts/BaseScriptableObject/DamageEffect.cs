using UnityEngine;

[CreateAssetMenu(fileName = "DamageEffect", menuName = "ScriptableObjects/SkillEffect/DamageEffect")]
public class DamageEffect : SkillEffect
{
    [SerializeField] public float BaseDamage;
    [SerializeField] public DamageElement Element;
    [SerializeField] public float Accuracy = 100f;
    [Range(0f, 100f)]
    [SerializeField] public float CriticalHitChance = 5f;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        Debug.Log(caster.gameObject.name + " dealt damage on " + target.gameObject.name);

        // 1. Accuracy Check
        if (Random.Range(0f, 100f) > Accuracy)
        {
            Debug.Log($"{caster.gameObject.name}'s attack but missed!");
            // Optional: you can show a "Miss" text here if you have a method for it.
            return false; 
        }

        // 2. Base Damage Calculation with +- 15% variance
        float variance = Random.Range(0.85f, 1.15f);
        float finalDamage = BaseDamage * variance;

        // 3. Critical Hit Check
        bool isCrit = Random.Range(0f, 100f) <= CriticalHitChance;
        if (isCrit)
        {
            finalDamage *= 2f;
            Debug.Log($"Critical Hit!");
        }

        Debug.Log($"Base Damage: {BaseDamage}, Variance: {variance}, Final Damage: {finalDamage}");

        Damage damage = new Damage(finalDamage, Element, isCrit);
        DamageCtx ctx = new DamageCtx(caster, target, damage);
        DamageSystem.Process(ctx, log);
        return true;
    }
}