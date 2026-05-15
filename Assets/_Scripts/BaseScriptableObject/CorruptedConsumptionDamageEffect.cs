using UnityEngine;

[CreateAssetMenu(fileName = "CorruptedConsumptionDamageEffect", menuName = "ScriptableObjects/SkillEffect/CorruptedConsumptionDamageEffect")]
public class CorruptedConsumptionDamageEffect : SkillEffect
{
    [SerializeField] public float BaseDamage = 500f;
    [SerializeField] public float DamagePerConsumedHp = 2f;
    [SerializeField] public float MaxHpToConsume = 500f;
    [SerializeField] public bool consumeAllCorruptedHp = false;
    
    [SerializeField] public DamageElement Element = DamageElement.Physical;
    [SerializeField, Range(0f, 1f)] public float Accuracy = 1f;
    [Range(0f, 1f)]
    [SerializeField] public float CriticalHitChance = 0.05f;

    public override bool IsElementalAttackEffect => true;
    public override bool IsDotElementSource => false;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        if (Random.value > Accuracy)
        {
            target.ShowDamage(0, Color.white);
            return false;
        }

        float hpToConsume = consumeAllCorruptedHp ? caster.CorruptedHealth : Mathf.Min(caster.CorruptedHealth, MaxHpToConsume);
        float bonusDamage = 0f;

        if (hpToConsume > 0)
        {
            caster.RemoveCorruptedHealth(hpToConsume);
            bonusDamage = hpToConsume * DamagePerConsumedHp;
            // // Debug.Log($"Consumed {hpToConsume} Corrupted HP for {bonusDamage} bonus damage!");
        }

        float finalDamage = BaseDamage + bonusDamage;

        // Variance
        float variance = Random.Range(0.85f, 1.15f);
        finalDamage *= variance;

        Damage damage = new Damage(finalDamage, Element, false, CriticalHitChance, 1.5f);
        DamageCtx ctx = new DamageCtx(caster, target, damage, style, log);
        DamageSystem.Process(ctx, log);
        return true;
    }
}
