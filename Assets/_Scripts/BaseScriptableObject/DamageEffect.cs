using UnityEngine;

[CreateAssetMenu(fileName = "DamageEffect", menuName = "ScriptableObjects/SkillEffect/DamageEffect")]
public class DamageEffect : SkillEffect
{
    [SerializeField] public float BaseDamage;
    [SerializeField] public DamageType DamageType;
    [Range(0f, 100f)]
    [SerializeField] public float Accuracy = 100f;
    [Range(0f, 100f)]
    [SerializeField] public float CriticalHitChance = 5f;
    [Tooltip("Critical Damage Multiplier (e.g. 1.5 = 150% damage)")]
    [SerializeField] public float CriticalDamageMultiplier = 1.5f;

    public override void Execute(Entity caster, Entity target, CombatActionLog log)
    {
        Debug.Log(caster.gameObject.name + " used " + DamageType + " DamageEffect on " + target.gameObject.name);

        // 1. Accuracy Check
        if (Random.Range(0f, 100f) > Accuracy)
        {
            Debug.Log($"{caster.gameObject.name}'s attack missed {target.gameObject.name}!");
            // Optional: you can show a "Miss" text here if you have a method for it.
            return; 
        }

        // 2. Base Damage Calculation with +- 15% variance
        float variance = Random.Range(0.85f, 1.15f);
        float finalDamage = BaseDamage * variance;

        // 3. Critical Hit Check
        bool isCrit = Random.Range(0f, 100f) <= CriticalHitChance;
        if (isCrit)
        {
            finalDamage *= CriticalDamageMultiplier;
            Debug.Log($"Critical Hit! Damage Multiplied by {CriticalDamageMultiplier}");
        }

        Debug.Log($"Base Damage: {BaseDamage}, Variance: {variance}, Final Damage: {finalDamage}");

        Damage damage = new Damage(DamageType, finalDamage);
        DamageCtx ctx = new DamageCtx(caster, target, damage);
        DamageSystem.Process(ctx, log);
    }
}