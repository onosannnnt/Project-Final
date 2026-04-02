using UnityEngine;

public enum DamageElement
{
    Physical,
    Fire,
    Frost,
    Lightning,
    Wind,
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
    public float Amount;
    public DamageElement Element;
    public bool IsCriticalHit;

    public Damage(float amount, DamageElement element = DamageElement.Physical, bool isCriticalHit = false)
    {
        Amount = amount;
        Element = element;
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

    public static Color GetDamageColor(DamageElement element)
    {
        return element switch
        {
            DamageElement.Physical => Color.white,
            DamageElement.Fire => Color.red,
            DamageElement.Frost => Color.cyan,
            DamageElement.Lightning => Color.yellow,
            DamageElement.Wind => Color.green,
            _ => Color.white
        };
    }
}