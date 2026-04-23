using TMPro;
using UnityEngine;

public class QuestProgressUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private UserData userData;

    [Header("Visibility")]
    [SerializeField] private GameObject panelToHide;

    [Header("UI")]
    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private TMP_Text coinRewardText;

    [Header("Refresh")]
    [SerializeField] private bool autoRefresh = true;
    [SerializeField] private float refreshInterval = 0.2f;

    private float refreshTimer;
    private CanvasGroup panelCanvasGroup;

    private void Awake()
    {
        GameObject targetPanel = panelToHide != null ? panelToHide : gameObject;
        panelCanvasGroup = targetPanel.GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
        {
            panelCanvasGroup = targetPanel.AddComponent<CanvasGroup>();
        }
    }

    private void OnEnable()
    {
        refreshTimer = 0f;
        RefreshUI();
    }

    private void Update()
    {
        if (!autoRefresh)
        {
            return;
        }

        refreshTimer += Time.deltaTime;
        if (refreshTimer >= Mathf.Max(0.05f, refreshInterval))
        {
            refreshTimer = 0f;
            RefreshUI();
        }
    }

    public void RefreshUI()
    {
        if (userData == null)
        {
            SetPanelVisible(false);
            SetSafeText(questNameText, "Quest: -");
            SetSafeText(coinRewardText, "-");
            return;
        }

        if (userData.IsGameFinished || !userData.HasSelectedQuest())
        {
            SetPanelVisible(false);
            SetSafeText(questNameText, "Quest: None");
            SetSafeText(coinRewardText, "0");
            return;
        }

        SetPanelVisible(true);

        int currentQuestIndex = userData.GetCurrentInProgressQuestIndex();
        string questName = userData.GetQuestDisplayName(currentQuestIndex);
        int activeQuestReward = userData.GetQuestCoinReward(currentQuestIndex);

        SetSafeText(questNameText, questName);
        SetSafeText(coinRewardText, activeQuestReward.ToString());
    }

    private void SetPanelVisible(bool visible)
    {
        if (panelCanvasGroup == null)
        {
            return;
        }

        panelCanvasGroup.alpha = visible ? 1f : 0f;
        panelCanvasGroup.interactable = visible;
        panelCanvasGroup.blocksRaycasts = visible;
    }

    private static void SetSafeText(TMP_Text label, string value)
    {
        if (label != null)
        {
            label.text = value;
        }
    }
}
