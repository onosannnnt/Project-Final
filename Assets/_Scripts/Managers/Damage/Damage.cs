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
[System.Serializable]
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
            StatScale.Health => StatType.MaxHealth,
            StatScale.SkillPoint => StatType.MaxSkillPoint,
            StatScale.ActionSpeed => StatType.ActionSpeed,
            _ => StatType.None
        };
    }
    public static StatType GetDamageMultiplierStat(DamageType type)
    {
        return StatType.None;
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