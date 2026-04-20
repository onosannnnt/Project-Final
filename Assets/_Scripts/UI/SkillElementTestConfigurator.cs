using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillElementTestConfigurator : MonoBehaviour
{
    [Header("Data Sources")]
    [SerializeField] private SkillListManager skillListManager;
    [SerializeField] private List<Skill> fallbackSkills = new();

    [Header("UI")]
    [SerializeField] private TMP_Dropdown skillDropdown;
    [SerializeField] private TMP_Dropdown elementDropdown;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("Behavior")]
    [SerializeField] private bool includeDotInDropdown = true;

    private readonly List<Skill> availableSkills = new();
    private readonly List<DamageElement> availableElements = new();

    private void Start()
    {
        if (applyButton != null)
        {
            applyButton.onClick.AddListener(ApplySelectedElement);
        }

        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(RefreshOptions);
        }

        RefreshOptions();
    }

    private void OnDestroy()
    {
        if (applyButton != null)
        {
            applyButton.onClick.RemoveListener(ApplySelectedElement);
        }

        if (refreshButton != null)
        {
            refreshButton.onClick.RemoveListener(RefreshOptions);
        }
    }

    public void RefreshOptions()
    {
        RebuildSkillOptions();
        RebuildElementOptions();
        SetResult("Ready");
    }

    public void ApplySelectedElement()
    {
        if (!TryGetSelectedSkill(out Skill selectedSkill))
        {
            SetResult("No valid skill selected.");
            return;
        }

        if (!TryGetSelectedElement(out DamageElement selectedElement))
        {
            SetResult("No valid element selected.");
            return;
        }

        if (!selectedSkill.TrySetElementForTesting(selectedElement, out string reason))
        {
            SetResult(reason);
            return;
        }

        RefreshSkillVisuals(selectedSkill);
        SetResult($"Success");
    }

    private bool TryGetSelectedSkill(out Skill selectedSkill)
    {
        selectedSkill = null;
        if (skillDropdown == null)
        {
            return false;
        }

        int index = skillDropdown.value;
        if (index < 0 || index >= availableSkills.Count)
        {
            return false;
        }

        selectedSkill = availableSkills[index];
        return selectedSkill != null;
    }

    private bool TryGetSelectedElement(out DamageElement selectedElement)
    {
        selectedElement = DamageElement.None;
        if (elementDropdown == null)
        {
            return false;
        }

        int index = elementDropdown.value;
        if (index < 0 || index >= availableElements.Count)
        {
            return false;
        }

        selectedElement = availableElements[index];
        return true;
    }

    private void RebuildSkillOptions()
    {
        availableSkills.Clear();

        if (skillListManager != null && skillListManager.allSkills != null)
        {
            for (int i = 0; i < skillListManager.allSkills.Count; i++)
            {
                Skill skill = skillListManager.allSkills[i];
                if (skill != null && !availableSkills.Contains(skill))
                {
                    availableSkills.Add(skill);
                }
            }
        }

        for (int i = 0; i < fallbackSkills.Count; i++)
        {
            Skill skill = fallbackSkills[i];
            if (skill != null && !availableSkills.Contains(skill))
            {
                availableSkills.Add(skill);
            }
        }

        if (skillDropdown == null)
        {
            return;
        }

        skillDropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> options = new();
        for (int i = 0; i < availableSkills.Count; i++)
        {
            Skill skill = availableSkills[i];
            options.Add(new TMP_Dropdown.OptionData(skill.skillName));
        }

        skillDropdown.AddOptions(options);
        if (options.Count > 0)
        {
            skillDropdown.value = 0;
            skillDropdown.RefreshShownValue();
        }
    }

    private void RebuildElementOptions()
    {
        availableElements.Clear();

        Array values = Enum.GetValues(typeof(DamageElement));
        foreach (object raw in values)
        {
            DamageElement element = (DamageElement)raw;
            if (!includeDotInDropdown && element == DamageElement.Dot)
            {
                continue;
            }

            availableElements.Add(element);
        }

        if (elementDropdown == null)
        {
            return;
        }

        elementDropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> options = new();
        for (int i = 0; i < availableElements.Count; i++)
        {
            DamageElement element = availableElements[i];
            string display = element == DamageElement.Dot ? "DoT" : element.ToString();
            options.Add(new TMP_Dropdown.OptionData(display));
        }

        elementDropdown.AddOptions(options);
        if (options.Count > 0)
        {
            elementDropdown.value = 0;
            elementDropdown.RefreshShownValue();
        }
    }

    private void RefreshSkillVisuals(Skill changedSkill)
    {
        SkillHoverHandler[] handlers = FindObjectsByType<SkillHoverHandler>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < handlers.Length; i++)
        {
            SkillHoverHandler handler = handlers[i];
            if (handler != null && handler.skillData == changedSkill)
            {
                handler.SetupData(changedSkill);
            }
        }
    }

    private void SetResult(string message)
    {
        if (resultText != null)
        {
            resultText.text = message;
        }
    }
}
