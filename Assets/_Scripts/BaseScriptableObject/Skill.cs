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
    public SkillType skillType;
    public int Tier;
    [Header("Cost")]
    public int SkillPoint;
    [Header("Attribute")]
    public TargetType TargetType;
    public TargetCount TargetCount;
    public bool reducesArmor = true; // Indicates if this skill reduces enemy armor
    public List<SkillEffect> SkillEffects;

    //Price by Tier (can be set to 0 for non-purchasable skills)
    public int GetSkillPrice()
    {
        //Tier 1: 100, Tier 2: 200, Tier 3: 300 (example pricing, can be adjusted as needed)
        switch (Tier)
        {
            case 1:
                return 10;
            case 2:
                return 30;
            case 3:
                return 80;
            default:
                return 0; // Default price for undefined tiers
        }
    }

    public DamageElement GetElement()
    {
        if (SkillEffects == null)
        {
            return DamageElement.None;
        }

        // Use the first effect that exposes a DamageElement as the skill element source.
        foreach (SkillEffect effect in SkillEffects)
        {
            if (effect == null)
            {
                continue;
            }

            if (!effect.IsElementalAttackEffect)
            {
                continue;
            }

            if (effect.TryGetElement(out DamageElement element))
            {
                if (element == DamageElement.Dot && !effect.IsDotElementSource)
                {
                    continue;
                }

                return element;
            }
        }

        // If this skill has no element-bearing effect, treat it as non-elemental.
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

    public bool TrySetElementForTesting(DamageElement newElement, out string reason)
    {
        if (SkillEffects == null || SkillEffects.Count == 0)
        {
            reason = "Skill has no effects.";
            return false;
        }

        bool foundAttackEffect = false;
        foreach (SkillEffect effect in SkillEffects)
        {
            if (effect == null || !effect.IsElementalAttackEffect)
            {
                continue;
            }

            foundAttackEffect = true;
            if (newElement == DamageElement.Dot && !effect.IsDotElementSource)
            {
                continue;
            }

            if (effect.TrySetElement(newElement))
            {
                reason = "Element updated.";
                return true;
            }
        }

        if (!foundAttackEffect)
        {
            reason = "Skill has no elemental attack effect.";
            return false;
        }

        if (newElement == DamageElement.Dot)
        {
            reason = "DoT element is allowed only for dedicated DoT element sources.";
            return false;
        }

        reason = "No writable element found on attack effects.";
        return false;
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
