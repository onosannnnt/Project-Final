using UnityEngine;

public enum DamageType
{
    Physical,
    Fire,
    Cold,
    Lightning,
    Pure
}
public enum StatScale
{
    PhysicalAttack,
    MagicAttack,
    Health,
    SkillPoint,
    ActionSpeed
}

public enum DamageColor
{
    White,
    Red,
    Blue,
    Green,
    Yellow
}

public class Damage
{
    public DamageType Type;
    public float Amount;
    public bool IsCriticalHit;

    public Damage(DamageType type, float amount, bool isCriticalHit = false)
    {
        Type = type;
        Amount = amount;
        IsCriticalHit = isCriticalHit;
    }
}
public static class Utils
{
    public static StatType GetScalingStat(StatScale type)
    {
        return type switch
        {
            StatScale.PhysicalAttack => StatType.PhysicalAttack,
            StatScale.MagicAttack => StatType.MagicAttack,
            StatScale.Health => StatType.MaxHealth,
            StatScale.SkillPoint => StatType.MaxSkillPoint,
            StatScale.ActionSpeed => StatType.ActionSpeed,
            _ => StatType.None
        };
    }
    public static StatType GetDamageMultiplierStat(DamageType type)
    {
        return type switch
        {
            DamageType.Fire => StatType.FireDamageMultiplier,
            DamageType.Cold => StatType.ColdDamageMultiplier,
            DamageType.Lightning => StatType.LightningDamageMultiplier,
            _ => StatType.None
        };
    }
    public static Color GetDamageColor(DamageType type)
    {
        return type switch
        {
            DamageType.Physical => Color.white,
            DamageType.Fire => Color.red,
            DamageType.Cold => Color.blue,
            DamageType.Lightning => Color.yellow,
            _ => Color.white
        };
    }
}