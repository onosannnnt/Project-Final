using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public enum Affixes
{
    Prefix,
    Suffix,
    Misc
}
public enum StatType
{
    Health,
    Attack,
    Defense,
    ActionSpeed,
    CriticalChance,
    CriticalDamage,
    LifeSteal,
    SkillPower
}

public enum DamageType
{
    Physical,
    Fire,
    Cold,
    Lightning
}

public class ItemStat
{
    public Affixes affix;
    public StatType statType;
    public float amount;
    public DamageType damageType;
    public bool isPercentage;
    public string Description;
    public List<Affixes> AffixesList = new List<Affixes>();
    public int MaxPrefix = 2;
    public int MaxSuffix = 2;
    public int MaxMisc = 1;
    public ItemStat Clone()
    {
        ItemStat cloneStat = new ItemStat();
        cloneStat.affix = this.affix;
        cloneStat.statType = this.statType;
        cloneStat.amount = this.amount;
        cloneStat.damageType = this.damageType;
        cloneStat.isPercentage = this.isPercentage;
        cloneStat.Description = this.Description;
        cloneStat.AffixesList = new List<Affixes>(this.AffixesList);
        cloneStat.MaxPrefix = this.MaxPrefix;
        cloneStat.MaxSuffix = this.MaxSuffix;
        cloneStat.MaxMisc = this.MaxMisc;
        return cloneStat;
    }
}


