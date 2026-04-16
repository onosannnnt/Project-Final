using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "ScriptableObjects/Utils/Skill")]
public class Skill : ScriptableObject
{
    [Header("Skill Info")]
    public int skillID;
    public string skillName;
    [TextArea(3, 10)]
    public string description;
    public Sprite skillIcon;
    [Header("Cost")]
    public int SkillPoint;
    [Header("Attribute")]
    public TargetType TargetType;
    public TargetCount TargetCount;
    public bool reducesArmor = true; // Indicates if this skill reduces enemy armor
    public List<SkillEffect> SkillEffects;

    public DamageElement GetElement()
    {
        if (SkillEffects == null)
        {
            return DamageElement.None;
        }

        // Use the first direct damage effect as the skill element source.
        foreach (SkillEffect effect in SkillEffects)
        {
            if (effect is DamageEffect damageEffect)
            {
                return damageEffect.Element;
            }
        }

        // If this skill has no DamageEffect, treat it as non-elemental.
        return DamageElement.None;
    }

    public string GetElementDisplayName()
    {
        DamageElement element = GetElement();
        return element == DamageElement.Dot ? "DoT" : element.ToString();
    }

    public string GetTooltipDescription()
    {
        DamageElement element = GetElement();
        Color elementColor = Utils.GetDamageColor(element);
        string elementHex = ColorUtility.ToHtmlStringRGB(elementColor);
        string elementLine = $"<b>Element:</b> <color=#{elementHex}>{GetElementDisplayName()}</color>";

        if (string.IsNullOrWhiteSpace(description))
        {
            return elementLine;
        }

        return $"{elementLine}\n{description}";
    }

    public bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        bool hit = true;
        foreach (var effect in SkillEffects)
        {
// // Debug.Log(effect.name + " effect is executed from skill " + skillName);
            bool effectHit = effect.Execute(caster, target, log);
            if (!effectHit)
            {
                hit = false;
                break; // Stop executing further effects if one misses
            }
        }
        return hit;
    }
}
