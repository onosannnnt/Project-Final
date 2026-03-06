using UnityEngine;

[CreateAssetMenu(fileName = "DamageEffect", menuName = "ScriptableObjects/SkillEffect/DamageEffect")]
public class DamageEffect : SkillEffect
{
    [SerializeField] public DamageType DamageType;
    [Tooltip("Damage Multiplier is not be in percent (1 = 100%, 1.5 = 150%)")]
    [SerializeField] public float DamageMultiplier;
    [SerializeField] public StatScale statScale;

    public DamageEffect(DamageType damageType, float damageMultiplier)
    {
        DamageType = damageType;
        DamageMultiplier = damageMultiplier;
    }

    public override void Execute(Entity caster, Entity target, CombatActionLog log)
    {
        Debug.Log(caster.gameObject.name + " used " + DamageType + " DamageEffect on " + target.gameObject.name);

        StatType scalingStat = Utils.GetScalingStat(statScale);
        float baseStat = caster.GetStat(scalingStat);

        float damageMultiplierStat = 0f;
        StatType multiplierStatType = Utils.GetDamageMultiplierStat(DamageType);
        if (multiplierStatType != StatType.None)
            damageMultiplierStat = caster.GetStat(multiplierStatType);

        float finalDamage =
            DamageMultiplier *
            baseStat *
            (1 + damageMultiplierStat);
        Debug.Log($"Base Stat: {baseStat}, Damage Multiplier: {DamageMultiplier}, Damage Multiplier Stat: {damageMultiplierStat}, Final Damage before crit: {finalDamage}");

        Damage damage = new Damage(DamageType, finalDamage);
        DamageCtx ctx = new DamageCtx(caster, target, damage);
        DamageSystem.Process(ctx, log);
    }
}