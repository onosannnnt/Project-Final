using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewSkill", menuName = "ScriptableObjects/SkillEffect/DamageEffect")]
public class DamageEffect : SkillEffect
{
    [SerializeField] public DamageType DamageType;
    [Tooltip("Damage Multiplier is not be in percent (1 = 100%, 1.5 = 150%)")]
    [SerializeField] public float DamageMultiplier;
    [SerializeField] public DamageScale DamageScale;
    [SerializeField] public List<Buff> Buffs;
    public override SkillEffectPhase Phase => SkillEffectPhase.Damage;

    public DamageEffect(DamageType damageType, float damageMultiplier)
    {
        DamageType = damageType;
        DamageMultiplier = damageMultiplier;
    }

    public override void Execute(Entity caster, Entity target)
    {
        Debug.Log(caster.gameObject.name + " used " + DamageType + " DamageEffect on " + target.gameObject.name);

        StatType scalingStat = Damage.GetScalingStat(DamageScale);
        float baseStat = caster.GetStat(scalingStat);

        float damageMultiplierStat = 0f;
        StatType multiplierStatType = Damage.GetDamageMultiplierStat(DamageType);
        if (multiplierStatType != StatType.None)
            damageMultiplierStat = caster.GetStat(multiplierStatType);

        float finalDamage =
            DamageMultiplier *
            baseStat *
            (1 + damageMultiplierStat);
        Debug.Log($"Base Stat: {baseStat}, Damage Multiplier: {DamageMultiplier}, Damage Multiplier Stat: {damageMultiplierStat}, Final Damage before crit: {finalDamage}");

        bool isCrit = Random.Range(0f, 100f) < caster.GetStat(StatType.CriticalHitChance);
        if (isCrit)
        {
            finalDamage *= 1 + caster.GetStat(StatType.CriticalDamageMultiplier);
        }

        Damage damage = new Damage(DamageType, finalDamage);
        target.TakeDamage(damage);
    }
}