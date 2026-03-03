using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "NewSkill", menuName = "ScriptableObjects/Skill")]
public class Skill : ScriptableObject
{
    [Tooltip("ID of the Skill")]
    public string skillID;
    [Tooltip("Skill Icon")]
    public Sprite skillIcon;
    [Tooltip("Name of the Skill")]
    public string skillName;
    [Tooltip("Description of the Skill")]
    [TextArea(3, 10)] // (จำนวนบรรทัดเริ่มต้น, จำนวนบรรทัดสูงสุด)
    public string description;
    [Tooltip("Sp cost of the Skill")]
    public float spCost;
    [Tooltip("Strength Requirement to use the Skill")]
    public int StrengthRequirement;
    [Tooltip("Intelligence Requirement to use the Skill")]
    public int IntelligenceRequirement;
    [Tooltip("Agility Requirement to use the Skill")]
    public int AgilityRequirement;
    [Tooltip("Level Requirement to use the Skill")]
    public int levelRequirement;
    [Tooltip("Target type of the Skill")]
    public TargetType targetType;
    [Tooltip("Physical Damage of skill base on player physical attack")]
    public float physicalDamageMultiplier;
    [Tooltip("Magic Damage of skill base on player magic attack")]
    public float magicDamageMultiplier;
    [Tooltip("Healing Amount of skill base on player max health divided by 100")]
    public float healingAmountMultiplier;
    [Tooltip("Buffs applied by the Skill")]
    public List<StatusBuff> buffs;

}