using UnityEngine;

[CreateAssetMenu(fileName = "EntitiesStat", menuName = "ScriptableObjects/EntitiesStat", order = 1)]
public class EntitiesBaseStat : ScriptableObject
{
    [Tooltip("Level of the Player")]
    public int Level;
    [Tooltip("Experience Points of the Player")]
    public float ExperiencePoints;
    [Tooltip("Strength Attribute of the Player")]
    public int Strength;
    [Tooltip("Intelligence Attribute of the Player")]
    public int Intelligence;
    [Tooltip("Agility Attribute of the Player")]
    public int Agility;
    [Tooltip("Maximum Health of the Player")]
    public float MaxHealth;
    [Tooltip("Maximum Skill Points of the Player")]
    public int MaxSkillPoint;
    [Tooltip("Max skill slot of the Player")]
    public int MaxSkillSlot;
    [Tooltip("Physical Attack Power of the Player")]
    public float PhysicalAttack;
    [Tooltip("Magic Attack Power of the Player")]
    public float MagicAttack;
    [Tooltip("Fire Damage multiplier of the Player divided by 100")]
    public float FireDamageMultiplier;
    [Tooltip("Cold Damage multiplier of the Player divided by 100")]
    public float ColdDamageMultiplier;
    [Tooltip("Lightning Damage multiplier of the Player")]
    public float LightningDamageMultiplier;
    [Tooltip("Physical Defense of the Player")]
    public float Armour;
    [Tooltip("Fire Resistance of the Player divided by 100")]
    public float FireResistance;
    [Tooltip("Cold Resistance of the Player divided by 100")]
    public float ColdResistance;
    [Tooltip("Lightning Resistance of the Player divided by 100")]
    public float LightningResistance;
    [Tooltip("Action Speed of the Player")]
    public float ActionSpeed;
    [Tooltip("Critical Hit Chance of the Player divided by 100")]
    public float CriticalHitChance;
    [Tooltip("Critical Hit Damage Multiplier of the Player divided by 100")]
    public float CriticalHitDamageMultiplier;
    [Tooltip("Accuracy of the Player")]
    public float Accuracy;
    [Tooltip("Evasion Rate of the Player")]
    public float EvasionRate;
    [Tooltip("Resistance to Status Effects of the Player (in percentage)")]
    public float StatusEffectResistance;
    [Tooltip("Status Hit Chance of the Player (in percentage)")]
    public float StatusHitChance;
    public float GetBase(StatType stat)
    {
        switch (stat)
        {
            case StatType.Level:
                return Level;

            case StatType.ExperiencePoints:
                return ExperiencePoints;

            case StatType.MaxHealth:
                return MaxHealth;

            case StatType.MaxSkillPoint:
                return MaxSkillPoint;

            case StatType.PhysicalAttack:
                return PhysicalAttack;

            case StatType.MagicAttack:
                return MagicAttack;

            case StatType.Armour:
                return Armour;

            case StatType.FireResistance:
                return FireResistance;

            case StatType.ColdResistance:
                return ColdResistance;

            case StatType.LightningResistance:
                return LightningResistance;

            case StatType.FireDamageMultiplier:
                return FireDamageMultiplier;

            case StatType.ColdDamageMultiplier:
                return ColdDamageMultiplier;

            case StatType.LightningDamageMultiplier:
                return LightningDamageMultiplier;

            case StatType.ActionSpeed:
                return ActionSpeed;

            case StatType.CriticalHitChance:
                return CriticalHitChance;

            case StatType.CriticalDamageMultiplier:
                return CriticalHitDamageMultiplier;

            case StatType.Accuracy:
                return Accuracy;

            case StatType.EvasionRate:
                return EvasionRate;

            case StatType.StatusEffectResistance:
                return StatusEffectResistance;

            case StatType.StatusHitChance:
                return StatusHitChance;

            default:
                Debug.LogWarning($"Stat {stat} not handled");
                return 0f;
        }
    }
    public EntitiesBaseStat Clone()
    {
        EntitiesBaseStat clone = Instantiate(this);
        return clone;
    }

}
