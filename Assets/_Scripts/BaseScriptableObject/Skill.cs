using System.Collections.Generic;
using System.Linq;
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
    public string Name;
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

    public void Execute(Entity caster, Entity target)
    {
        float hitChance = caster.GetStat(StatType.Accuracy) / (caster.GetStat(StatType.Accuracy) + target.GetStat(StatType.EvasionRate));

        if (Random.Range(0f, 1f) > hitChance)
        {
            Debug.Log("Applied skill into " + target.gameObject.name + " but it missed!");
            return;
        }
        var orderedEffects = SkillEffects
        .OrderBy(e => e.Phase)
        .ToList();

        foreach (var effect in orderedEffects)
        {
            Debug.Log(effect.name + " effect is executed from skill " + Name);
            effect.Execute(caster, target);
        }

    }
}
