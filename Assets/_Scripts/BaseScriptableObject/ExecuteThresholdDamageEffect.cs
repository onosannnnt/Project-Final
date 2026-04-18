using UnityEngine;

[CreateAssetMenu(fileName = "ExecuteThresholdDamageEffect", menuName = "ScriptableObjects/SkillEffect/ExecuteThresholdDamageEffect")]
public class ExecuteThresholdDamageEffect : SkillEffect
{
    [SerializeField] public float NormalDamage = 600f;
    [SerializeField] public float ExecuteDamage = 2000f;
    [SerializeField, Range(0f, 1f)] public float ExecuteHpThreshold = 0.30f;
    [SerializeField] public DamageElement Element = DamageElement.Physical;
    [SerializeField] public float Accuracy = 100f;
    [Range(0f, 100f)]
    [SerializeField] public float CriticalHitChance = 5f;
    [SerializeField] public float CriticalDamageMultiplier = 1.5f;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (Random.Range(0f, 100f) > Accuracy)
        {
            target.ShowDamage(0, Color.white);
            return false;
        }

        float maxHealth = Mathf.Max(target.GetStat(StatType.MaxHealth), 1f);
        float hpRatio = target.CurrentHealth / maxHealth;
        float finalDamage = hpRatio <= ExecuteHpThreshold ? ExecuteDamage : NormalDamage;

        bool isCrit = Random.Range(0f, 100f) <= CriticalHitChance;
        if (isCrit)
        {
            finalDamage *= CriticalDamageMultiplier;
        }

        Damage damage = new Damage(finalDamage, Element, isCrit);
        DamageCtx ctx = new DamageCtx(caster, target, damage);
        DamageSystem.Process(ctx, log);
        return true;
    }
}
