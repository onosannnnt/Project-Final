using UnityEngine;

[CreateAssetMenu(fileName = "CorruptedBonusDamageEffect", menuName = "ScriptableObjects/SkillEffect/CorruptedBonusDamageEffect")]
public class CorruptedBonusDamageEffect : SkillEffect
{
    [SerializeField] public float BaseDamage = 500f;
    [SerializeField] public float BonusDamage = 500f;
    [SerializeField] public float CorruptedHpThreshold = 500f;
    [SerializeField] public bool usePercentThreshold = false;
    [SerializeField, Range(0f, 1f)] public float CorruptedHpPercentThreshold = 0.2f;
    
    [SerializeField] public DamageElement Element = DamageElement.Physical;
    [SerializeField] public float Accuracy = 100f;
    [Range(0f, 100f)]
    [SerializeField] public float CriticalHitChance = 5f;

    public override bool IsElementalAttackEffect => true;
    public override bool IsDotElementSource => false;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (Random.Range(0f, 100f) > Accuracy)
        {
            target.ShowDamage(0, Color.white);
            return false;
        }

        bool conditionMet = false;
        if (usePercentThreshold)
        {
            float maxHp = caster.GetStat(StatType.MaxHealth);
            conditionMet = (caster.CorruptedHealth / maxHp) >= CorruptedHpPercentThreshold;
        }
        else
        {
            conditionMet = caster.CorruptedHealth >= CorruptedHpThreshold;
        }

        float finalDamage = BaseDamage;
        if (conditionMet)
        {
            finalDamage += BonusDamage;
            // // Debug.Log($"Bonus damage activated! Corrupted HP: {caster.CorruptedHealth}");
        }

        // Variance
        float variance = Random.Range(0.85f, 1.15f);
        finalDamage *= variance;

        Damage damage = new Damage(finalDamage, Element, false, CriticalHitChance, 1.5f);
        DamageCtx ctx = new DamageCtx(caster, target, damage);
        DamageSystem.Process(ctx, log);
        return true;
    }
}
