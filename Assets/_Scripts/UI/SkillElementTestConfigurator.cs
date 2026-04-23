using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillElementTestConfigurator : MonoBehaviour
{
    [Serializable]
    private class ElementButtonRuntime
    {
        public DamageElement element;
        public Button button;
        public TMP_Text label;
        public Image icon;
        public Image backgroundImage;
        public GameObject selectedVisual;
        public GameObject normalVisual;
        public GameObject rootObject;
        public Color originalBackgroundColor = Color.white;
    }

    [Header("Data Sources")]
    [SerializeField] private SkillListManager skillListManager;
    [SerializeField] private List<Skill> fallbackSkills = new();

    [Header("Active Skill")]
    [SerializeField] private TMP_Text activeSkillText;
    [SerializeField] private bool autoSelectFirstSkillOnRefresh = true;
    [SerializeField] private int preferredActiveSkillIndex;

    [Header("UI Reference")]
    [SerializeField] private TMP_Dropdown skillDropdown; // ลาก Dropdown มาใส่ในช่องนี้

    [Header("Element Buttons")]
    [SerializeField] private Transform elementButtonParent;
    [SerializeField] private GameObject elementButtonPrefab;
    [SerializeField] private string elementNameChildName = "ElementName";
    [SerializeField] private string elementIconChildName = "ElementIcon";
    [SerializeField] private string selectedVisualChildName = "SelectElementBox";
    [SerializeField] private string normalVisualChildName = "ElementBox";


    [Header("Element Button Fallback Visual")]
    [SerializeField] private bool tintBackgroundWhenSelected = true;
    [SerializeField] private Color selectedBackgroundColor = new Color(1f, 0.92f, 0.55f, 1f);

    [Header("Element Icons (Optional)")]
    [SerializeField] private Sprite noneElementSprite;
    [SerializeField] private Sprite physicalElementSprite;
    [SerializeField] private Sprite fireElementSprite;
    [SerializeField] private Sprite frostElementSprite;
    [SerializeField] private Sprite lightningElementSprite;
    [SerializeField] private Sprite windElementSprite;
    [SerializeField] private Sprite dotElementSprite;
    [SerializeField] private bool hideElementIconWhenMissingSprite = true;

    [Header("Behavior")]
    [SerializeField] private bool includeDotElement = true;
    [SerializeField] private bool clickSameElementToSetNone = true;

    private readonly List<Skill> availableSkills = new();
    private readonly List<DamageElement> availableElements = new();
    private readonly List<ElementButtonRuntime> elementButtons = new();

    private Skill activeSkill;

    private void Start()
    {
        RefreshOptions();
    }

    private void OnDestroy()
    {
        ClearGeneratedElementButtons();
    }

    public void RefreshOptions()
    {
        RebuildSkillOptions();
        UpdateDropdownOptions();
        RebuildElementOptions();
        EnsureActiveSkillSelection();
        RebuildElementButtons();
        RefreshActiveSkillText();
        RefreshElementButtonVisuals();
    }

    public void SetActiveSkill(Skill skill)
    {
        activeSkill = IsSkillSelectable(skill) ? skill : null;
        RefreshActiveSkillText();
        RefreshElementButtonVisuals();
    }

    public void SetActiveSkillByIndex(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= availableSkills.Count)
        {
            activeSkill = null;
            RefreshActiveSkillText();
            RefreshElementButtonVisuals();
            return;
        }

        SetActiveSkill(availableSkills[skillIndex]);
    }

    public void SetActiveSkillByLoadoutSlot(int slotIndex)
    {
        if (skillListManager == null || skillListManager.selectedSlots == null)
        {
            activeSkill = null;
            RefreshActiveSkillText();
            RefreshElementButtonVisuals();
            return;
        }

        if (slotIndex < 0 || slotIndex >= skillListManager.selectedSlots.Length)
        {
            activeSkill = null;
            RefreshActiveSkillText();
            RefreshElementButtonVisuals();
            return;
        }

        SelectedSlot slot = skillListManager.selectedSlots[slotIndex];
        SetActiveSkill(slot != null ? slot.skillData : null);
    }

    private void EnsureActiveSkillSelection()
    {
        if (IsSkillSelectable(activeSkill))
        {
            return;
        }

        if (!autoSelectFirstSkillOnRefresh || availableSkills.Count == 0)
        {
            activeSkill = null;
            return;
        }

        int index = Mathf.Clamp(preferredActiveSkillIndex, 0, availableSkills.Count - 1);
        activeSkill = availableSkills[index];
    }

    private bool IsSkillSelectable(Skill skill)
    {
        return skill != null && availableSkills.Contains(skill);
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

    }

    private void RebuildElementOptions()
    {
        availableElements.Clear();

        Array values = Enum.GetValues(typeof(DamageElement));
        foreach (object raw in values)
        {
            DamageElement element = (DamageElement)raw;
            if (element == DamageElement.None)
            {
                continue;
            }

            if (!includeDotElement && element == DamageElement.Dot)
            {
                continue;
            }

            availableElements.Add(element);
        }
    }

    private void UpdateDropdownOptions()
    {
        if (skillDropdown == null) return;

        // 1. ล้างค่าเก่าออกก่อน
        skillDropdown.ClearOptions();

        // 2. สร้าง List ของข้อความชื่อสกิล
        List<string> options = new List<string>();
        foreach (Skill skill in availableSkills)
        {
            if (skill != null)
            {
                options.Add(skill.skillName);
            }
        }

        // 3. ใส่ข้อมูลลงใน Dropdown
        skillDropdown.AddOptions(options);
    }

    private void RebuildElementButtons()
    {
        ClearGeneratedElementButtons();

        if (elementButtonParent == null || elementButtonPrefab == null)
        {
            return;
        }

        for (int i = 0; i < availableElements.Count; i++)
        {
            DamageElement element = availableElements[i];
            GameObject buttonObject = Instantiate(elementButtonPrefab, elementButtonParent);
            buttonObject.SetActive(true);

            Button button = buttonObject.GetComponent<Button>();
            if (button == null)
            {
                button = buttonObject.GetComponentInChildren<Button>(true);
            }

            if (button == null)
            {
                Destroy(buttonObject);
                continue;
            }

            ElementButtonRuntime runtime = new ElementButtonRuntime
            {
                element = element,
                button = button,
                rootObject = buttonObject,
                label = FindTextByName(buttonObject.transform, elementNameChildName),
                icon = FindImageByName(buttonObject.transform, elementIconChildName),
                selectedVisual = FindChildRecursive(buttonObject.transform, selectedVisualChildName)?.gameObject,
                normalVisual = FindChildRecursive(buttonObject.transform, normalVisualChildName)?.gameObject
            };

            if (runtime.label == null)
            {
                runtime.label = buttonObject.GetComponentInChildren<TMP_Text>(true);
            }

            if (runtime.label != null)
            {
                runtime.label.text = GetElementDisplayName(element);
            }

            ApplyElementIcon(runtime.icon, element);

            runtime.backgroundImage = button.targetGraphic as Image;
            if (runtime.backgroundImage == null)
            {
                runtime.backgroundImage = buttonObject.GetComponent<Image>();
            }

            if (runtime.backgroundImage != null)
            {
                runtime.originalBackgroundColor = runtime.backgroundImage.color;
            }

            if (runtime.selectedVisual != null)
            {
                runtime.selectedVisual.SetActive(false);
            }

            DamageElement capturedElement = element;
            runtime.button.onClick.AddListener(() => OnElementButtonClicked(capturedElement));

            elementButtons.Add(runtime);
        }
    }

    private void ClearGeneratedElementButtons()
    {
        for (int i = 0; i < elementButtons.Count; i++)
        {
            ElementButtonRuntime runtime = elementButtons[i];
            if (runtime != null && runtime.rootObject != null)
            {
                Destroy(runtime.rootObject);
            }
        }

        elementButtons.Clear();
    }

    private void OnElementButtonClicked(DamageElement clickedElement)
    {
        if (activeSkill == null)
        {
            Debug.LogWarning("No active skill selected for element change.");
            return;
        }

        DamageElement currentElement = activeSkill.GetElement();
        DamageElement targetElement = clickedElement;

        if (clickSameElementToSetNone && currentElement == clickedElement)
        {
            targetElement = DamageElement.None;
        }

        if (!activeSkill.TrySetElementForTesting(targetElement, out string reason))
        {
            Debug.LogWarning(reason);
            return;
        }

        RefreshSkillVisuals(activeSkill);
        RefreshActiveSkillText();
        RefreshElementButtonVisuals();
    }

    private void RefreshActiveSkillText()
    {
        if (activeSkillText == null)
        {
            return;
        }

        if (activeSkill == null)
        {
            activeSkillText.text = "None";
            return;
        }

        activeSkillText.text = activeSkill.skillName;
    }

    private void RefreshElementButtonVisuals()
    {
        DamageElement selectedElement = activeSkill != null ? activeSkill.GetElement() : DamageElement.None;
        bool hasActiveSkill = activeSkill != null;

        for (int i = 0; i < elementButtons.Count; i++)
        {
            ElementButtonRuntime runtime = elementButtons[i];
            bool isSelected = hasActiveSkill && selectedElement == runtime.element;

            if (runtime.button != null)
            {
                runtime.button.interactable = hasActiveSkill;
            }

            if (runtime.selectedVisual != null)
            {
                runtime.selectedVisual.SetActive(isSelected);
            }

            if (runtime.normalVisual != null)
            {
                runtime.normalVisual.SetActive(!isSelected);
            }

            if (tintBackgroundWhenSelected && runtime.backgroundImage != null)
            {
                if (runtime.selectedVisual == null)
                {
                    runtime.backgroundImage.color = isSelected ? selectedBackgroundColor : runtime.originalBackgroundColor;
                }
                else
                {
                    runtime.backgroundImage.color = runtime.originalBackgroundColor;
                }
            }
        }
    }

    private void ApplyElementIcon(Image iconImage, DamageElement element)
    {
        if (iconImage == null)
        {
            return;
        }

        Sprite sprite = GetElementSprite(element);
        if (sprite == null && hideElementIconWhenMissingSprite)
        {
            iconImage.gameObject.SetActive(false);
            return;
        }

        iconImage.gameObject.SetActive(true);
        iconImage.sprite = sprite;
    }

    private Sprite GetElementSprite(DamageElement element)
    {
        switch (element)
        {
            case DamageElement.Physical:
                return physicalElementSprite;
            case DamageElement.Fire:
                return fireElementSprite;
            case DamageElement.Frost:
                return frostElementSprite;
            case DamageElement.Lightning:
                return lightningElementSprite;
            case DamageElement.Wind:
                return windElementSprite;
            case DamageElement.Dot:
                return dotElementSprite;
            default:
                return noneElementSprite;
        }
    }

    private static string GetElementDisplayName(DamageElement element)
    {
        return element == DamageElement.Dot ? "DoT" : element.ToString();
    }

    private static TMP_Text FindTextByName(Transform root, string childName)
    {
        Transform target = FindChildRecursive(root, childName);
        return target != null ? target.GetComponent<TMP_Text>() : null;
    }

    private static Image FindImageByName(Transform root, string childName)
    {
        Transform target = FindChildRecursive(root, childName);
        return target != null ? target.GetComponent<Image>() : null;
    }

    private static Transform FindChildRecursive(Transform root, string targetName)
    {
        if (root == null || string.IsNullOrEmpty(targetName))
        {
            return null;
        }

        if (root.name == targetName)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindChildRecursive(root.GetChild(i), targetName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
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
}
