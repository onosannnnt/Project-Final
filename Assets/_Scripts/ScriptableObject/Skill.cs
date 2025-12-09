using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "NewSkill", menuName = "ScriptableObjects/Skill")]
public class Skill : ScriptableObject
{
    [Tooltip("Name of the Skill")]
    public string SkillName;
    [Tooltip("Description of the Skill")]
    public string Description;
    [Tooltip("Sp cost of the Skill")]
    public int SkillPoint;
    [Tooltip("Sp restore of the Skill")]
    public int SkillPointRestore;
    [Tooltip("Strength Requirement to use the Skill")]
    public int StrengthRequirement;
    [Tooltip("Intelligence Requirement to use the Skill")]
    public int IntelligenceRequirement;
    [Tooltip("Agility Requirement to use the Skill")]
    public int AgilityRequirement;
    [Tooltip("Level Requirement to use the Skill")]
    public int LevelRequirement;
    [Tooltip("Target type of the Skill")]
    public TargetType TargetType;
    [Tooltip("Physical Damage of skill base on player physical attack")]
    public float PhysicalDamageMultiplier;
    [Tooltip("Fire Damage of skill base on player magic attack")]
    public float FireDamageMultiplier;
    [Tooltip("Cold Damage of skill base on player magic attack")]
    public float ColdDamageMultiplier;
    [Tooltip("Lightning Damage of skill base on player magic attack")]
    public float LightningDamageMultiplier;
    [Tooltip("Healing Amount of skill base on player max health divided by 100")]
    public float HealingAmountMultiplier;
    [Tooltip("Buffs applied by the Skill")]
    public List<StatusBuff> Buffs;

}