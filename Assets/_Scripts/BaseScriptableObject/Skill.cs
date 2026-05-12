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
    public SkillStyle skillStyle;
    public int Tier;
    [Header("Cost")]
    public int SkillPoint;
    public float CorruptedHealthGain;
    [Header("Attribute")]
    public TargetType TargetType;
    public TargetCount TargetCount;
    public bool reducesArmor = true; // Indicates if this skill reduces enemy armor
    public List<SkillEffect> SkillEffects;

    //Price by Tier (can be set to 0 for non-purchasable skills)
    public int GetSkillPrice()
    {
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

    public Skill Clone()
    {
        Skill clone = Instantiate(this);
        clone.SkillEffects = new List<SkillEffect>();
        if (SkillEffects != null)
        {
            foreach (var effect in SkillEffects)
            {
                if (effect != null)
                    clone.SkillEffects.Add(Instantiate(effect));
            }
        }
        return clone;
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
        string skillpointLine = $"<b>SP Cost:</b> <color=#A78BFA>{SkillPoint}</color>";

        if (CorruptedHealthGain > 0)
        {
            elementLine += $"\n<b>Corrupt:</b> <color=red>+{CorruptedHealthGain}</color>";
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return elementLine;
        }

        return $"{elementLine}\n{skillpointLine}\n{description}";
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

    public bool Execute(Entity caster, List<Entity> targets, CombatActionLog log)
    {
        if (SkillEffects == null || SkillEffects.Count == 0 || targets == null || targets.Count == 0)
        {
            return false;
        }

        // 1. Prepare: Sort and Initialize
        List<SkillEffect> sortedEffects = new List<SkillEffect>(SkillEffects);
        sortedEffects.Sort((a, b) => a.Phase.CompareTo(b.Phase));

        foreach (var effect in sortedEffects)
        {
            if (effect != null) effect.OnSkillStarted(caster, targets, log);
        }

        bool overallHit = true;

        // 2. Execute target loop
        for (int i = 0; i < targets.Count; i++)
        {
            Entity currentTarget = targets[i];
            if (currentTarget == null) continue;

            foreach (var effect in sortedEffects)
            {
                if (effect == null) continue;

                // If ExecuteOnce is set, only run this effect for the very first target in the list
                if (effect.ExecuteOnce && i > 0) continue;

                bool effectHit = effect.Execute(caster, currentTarget, log, skillStyle);
                if (!effectHit)
                {
                    overallHit = false;
                    // Note: We continue to other targets even if one misses, 
                    // but you could change this to 'break' if a miss stops the whole AOE.
                }
            }
        }

        return overallHit;
    }
}
