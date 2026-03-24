using System.Collections.Generic;
using UnityEngine;

public enum SkillTag
{
    Physical,
    Magical,
    Fire,
    Cold,
    Lightning,
    Heal,
    Buff,
    Debuff,
    SingleTarget,
    MultiTarget,
    DOT,
    CrowdControl,
}

[CreateAssetMenu(fileName = "NewSkill", menuName = "ScriptableObjects/Utils/Skill")]
public class Skill : ScriptableObject
{
    [Header("Skill Info")]
    public int skillID;
    public string skillName;
    [TextArea(3, 10)]
    public string description;
    public Sprite skillIcon;
    [Header("Requirement")]
    public int Level;
    public int Strength;
    public int Agility;
    public int Intelligence;
    [Header("Cost")]
    public int SkillPoint;
    public int SkillPointRestore;
    [Header("Attribute")]
    public TargetType TargetType;
    public List<SkillEffect> SkillEffects;

    public void Execute(Entity caster, Entity target, CombatActionLog log)
    {
        foreach (var effect in SkillEffects)
        {
            Debug.Log(effect.name + " effect is executed from skill " + skillName);
            effect.Execute(caster, target, log);
        }
    }
}
