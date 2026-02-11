
public enum DamageType
{
    Physical,
    Fire,
    Cold,
    Lightning,
    Pure
}
public enum DamageScale
{
    PhysicalAttack,
    MagicAttack,
    Health,
    SkillPoint,
    ActionSpeed


}



public class Damage
{
    public DamageType Type;
    public float Amount;

    public Damage(DamageType type, float amount)
    {
        Type = type;
        Amount = amount;
    }
    public static StatType GetScalingStat(DamageScale type)
    {
        return type switch
        {
            DamageScale.PhysicalAttack => StatType.PhysicalAttack,
            DamageScale.MagicAttack => StatType.MagicAttack,
            DamageScale.Health => StatType.MaxHealth,
            DamageScale.SkillPoint => StatType.MaxSkillPoint,
            DamageScale.ActionSpeed => StatType.ActionSpeed,
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
}