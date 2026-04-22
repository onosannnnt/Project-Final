using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QuestSelectionUI : MonoBehaviour
{
    [System.Serializable]
    public class QuestSlot
    {
        [Range(UserData.TutorialQuestIndex, UserData.LastQuestIndex)]
        public int questIndex;

        public Button selectButton;
        public Image slotBackground;
        public TMP_Text questNameText;
        public TMP_Text rewardText;
        public TMP_Text statusText;
    }

    [Header("Data")]
    [SerializeField] private UserData userData;
    [SerializeField] private List<QuestSlot> questSlots = new List<QuestSlot>();

    [Header("Buttons")]
    [SerializeField] private Button acceptButton;
    [SerializeField] private Image acceptButtonBackground;
    [SerializeField] private TMP_Text acceptButtonText;
    [SerializeField] private Button closeButton;

    [Header("Accept Button Colors")]
    [SerializeField] private Color acceptEnabledColor = new Color(1f, 0.82f, 0.18f);
    [SerializeField] private Color acceptDisabledColor = new Color(0.76f, 0.76f, 0.76f);
    [SerializeField] private Color acceptEnabledTextColor = Color.black;
    [SerializeField] private Color acceptDisabledTextColor = new Color(0.55f, 0.55f, 0.55f);

    [Header("Quest Slot Colors")]
    [SerializeField] private Color slotDefaultColor = Color.white;
    [SerializeField] private Color slotSelectedColor = new Color(1f, 0.96f, 0.78f);
    [SerializeField] private Color slotDisabledColor = new Color(0.9f, 0.9f, 0.9f);

    [Header("Behavior")]
    [SerializeField] private bool closePanelOnAccept = true;
    [SerializeField] private bool loadCombatOnAccept;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private UnityEvent onQuestAccepted;
    [SerializeField] private UnityEvent onPanelClosed;

    private int selectedQuestIndex = -1;

    private void Start()
    {
        BindButtons();
        RefreshQuestDetails();
        ClearSelection();
    }

    private void OnEnable()
    {
        RefreshQuestDetails();
        ClearSelection();
    }

    private void BindButtons()
    {
        for (int i = 0; i < questSlots.Count; i++)
        {
            int slotIndex = i;
            if (questSlots[slotIndex].selectButton != null)
            {
                questSlots[slotIndex].selectButton.onClick.AddListener(() => OnQuestSlotClicked(slotIndex));
            }
        }

        if (acceptButton != null)
        {
            acceptButton.onClick.AddListener(OnAcceptButtonClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
    }

    public void RefreshQuestDetails()
    {
        for (int i = 0; i < questSlots.Count; i++)
        {
            QuestSlot slot = questSlots[i];

            if (userData == null)
            {
                SetSafeText(slot.questNameText, "-");
                SetSafeText(slot.rewardText, "-");
                SetSafeText(slot.statusText, "Unavailable");
                SetSlotInteractable(slot, false);
                SetSlotColor(slot, slotDisabledColor);
                continue;
            }

            int questIndex = slot.questIndex;
            bool canSelect = userData.CanStartQuest(questIndex);
            bool completed = userData.IsQuestCompleted(questIndex);

            SetSafeText(slot.questNameText, userData.GetQuestDisplayName(questIndex));
            SetSafeText(slot.rewardText, userData.GetQuestCoinReward(questIndex).ToString());
            SetSafeText(slot.statusText, canSelect ? "Available" : (completed ? "Completed" : "Locked"));

            SetSlotInteractable(slot, canSelect);
            SetSlotColor(slot, canSelect ? slotDefaultColor : slotDisabledColor);
        }
    }

    public void OpenPanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        RefreshQuestDetails();
        ClearSelection();
    }

    public void OnCloseButtonClicked()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        onPanelClosed?.Invoke();
    }

    private void OnQuestSlotClicked(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= questSlots.Count || userData == null)
        {
            return;
        }

        int questIndex = questSlots[slotIndex].questIndex;
        if (!userData.CanStartQuest(questIndex))
        {
            return;
        }

        selectedQuestIndex = questIndex;
        SetAcceptButtonState(true);
        RefreshSlotSelectionVisuals();
    }

    private void OnAcceptButtonClicked()
    {
        if (userData == null || selectedQuestIndex < 0)
        {
            SetAcceptButtonState(false);
            return;
        }

        if (!userData.TrySetSelectedQuest(selectedQuestIndex))
        {
            Debug.LogWarning("Cannot accept selected quest. " + userData.GetQuestBlockedReason(selectedQuestIndex));
            RefreshQuestDetails();
            ClearSelection();
            return;
        }

        onQuestAccepted?.Invoke();

        if (loadCombatOnAccept)
        {
            Loader.Load(Loader.Scenes.Combat);
            return;
        }

        if (closePanelOnAccept)
        {
            OnCloseButtonClicked();
        }
    }

    private void ClearSelection()
    {
        selectedQuestIndex = -1;
        SetAcceptButtonState(false);
        RefreshSlotSelectionVisuals();
    }

    private void RefreshSlotSelectionVisuals()
    {
        for (int i = 0; i < questSlots.Count; i++)
        {
            QuestSlot slot = questSlots[i];
            if (userData == null)
            {
                SetSlotColor(slot, slotDisabledColor);
                continue;
            }

            bool canSelect = userData.CanStartQuest(slot.questIndex);
            bool isSelected = selectedQuestIndex == slot.questIndex;

            if (!canSelect)
            {
                SetSlotColor(slot, slotDisabledColor);
            }
            else if (isSelected)
            {
                SetSlotColor(slot, slotSelectedColor);
            }
            else
            {
                SetSlotColor(slot, slotDefaultColor);
            }
        }
    }

    private void SetAcceptButtonState(bool enabled)
    {
        if (acceptButton != null)
        {
            acceptButton.interactable = enabled;
        }

        if (acceptButtonBackground != null)
        {
            acceptButtonBackground.color = enabled ? acceptEnabledColor : acceptDisabledColor;
        }

        if (acceptButtonText != null)
        {
            acceptButtonText.color = enabled ? acceptEnabledTextColor : acceptDisabledTextColor;
        }
    }

    private static void SetSafeText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private static void SetSlotInteractable(QuestSlot slot, bool enabled)
    {
        if (slot.selectButton != null)
        {
            slot.selectButton.interactable = enabled;
        }
    }

    private static void SetSlotColor(QuestSlot slot, Color color)
    {
        if (slot.slotBackground != null)
        {
            slot.slotBackground.color = color;
        }
    }
}
