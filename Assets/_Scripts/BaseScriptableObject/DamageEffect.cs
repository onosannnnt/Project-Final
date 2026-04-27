using UnityEngine;

[CreateAssetMenu(fileName = "DamageEffect", menuName = "ScriptableObjects/SkillEffect/DamageEffect")]
public class DamageEffect : SkillEffect
{
    [SerializeField] public float BaseDamage;
    [SerializeField] public DamageElement Element;
    [SerializeField] public float Accuracy = 100f;
    [Range(0f, 100f)]
    [SerializeField] public float CriticalHitChance = 5f;

    public override bool IsElementalAttackEffect => true;
    public override bool IsDotElementSource => false;
    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        // 1. Accuracy Check
        if (Random.Range(0f, 100f) > Accuracy)
        {
            target.ShowDamage(0, Color.white);
            return false;
        }

        // 2. Base Damage Calculation with +- 15% variance
        float variance = Random.Range(0.85f, 1.15f);
        float finalDamage = BaseDamage * variance;

        // 3. Critical Hit Check
        bool isCrit = Random.Range(0f, 100f) <= CriticalHitChance;
        if (isCrit)
        {
            finalDamage *= 1.5f;
        }

        // ใน DamageEffect.cs
        if (target is PuzzleBossCombat puzzleBoss)
        {
            finalDamage *= puzzleBoss.GetVulnerabilityMultiplier();
        }

        Damage damage = new Damage(finalDamage, Element, isCrit);
        DamageCtx ctx = new DamageCtx(caster, target, damage);
        DamageSystem.Process(ctx, log);
        return true;
    }
}
