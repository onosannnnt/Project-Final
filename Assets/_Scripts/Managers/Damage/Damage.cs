using UnityEngine;
using System.Collections.Generic;

public enum DamageElement
{
    None,
    Physical,
    Fire,
    Frost,
    Lightning,
    Wind,
    Dot
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
    Yellow,
    Violet
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
    private static readonly Dictionary<StatScale, StatType> ScalingStatMap = new()
    {
        { StatScale.Health, StatType.MaxHealth },
        { StatScale.SkillPoint, StatType.MaxSkillPoint },
        { StatScale.ActionSpeed, StatType.ActionSpeed }
    };

    private static readonly Dictionary<DamageElement, Color> DamageColorMap = new()
    {
        { DamageElement.Physical, Color.white },
        { DamageElement.Fire, Color.red },
        { DamageElement.Frost, Color.cyan },
        { DamageElement.Lightning, Color.yellow },
        { DamageElement.Wind, new Color32(0x4F, 0xFA, 0xAA, 0xFF) }, // #4ffaaa Hex for Wind
        { DamageElement.Dot, new Color32(0xFF, 0x33, 0xFF, 0xFF) } // #FF33FF Hex for DoT
    };

    public static StatType GetScalingStat(StatScale type)
    {
        return ScalingStatMap.TryGetValue(type, out var stat) ? stat : StatType.None;
    }

    public static Color GetDamageColor(DamageElement element)
    {
        return DamageColorMap.TryGetValue(element, out var color) ? color : Color.white;
    }
}
