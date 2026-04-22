using TMPro;
using UnityEngine;

public class QuestProgressUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private UserData userData;

    [Header("UI")]
    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private TMP_Text coinRewardText;

    [Header("Refresh")]
    [SerializeField] private bool autoRefresh = true;
    [SerializeField] private float refreshInterval = 0.2f;

    private float refreshTimer;

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
            SetSafeText(questNameText, "Quest: -");
            SetSafeText(coinRewardText, "-");
            return;
        }

        if (userData.IsGameFinished)
        {
            SetSafeText(questNameText, "Quest: None");
            SetSafeText(coinRewardText, userData.TotalCoins.ToString());
            return;
        }

        int currentQuestIndex = userData.GetCurrentInProgressQuestIndex();
        string questName = userData.GetQuestDisplayName(currentQuestIndex);

        SetSafeText(questNameText, questName);
        SetSafeText(coinRewardText, userData.TotalCoins.ToString());
    }

    private static void SetSafeText(TMP_Text label, string value)
    {
        if (label != null)
        {
            label.text = value;
        }
    }
}
