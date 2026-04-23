using TMPro;
using UnityEngine;

public class TotalCoinUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private UserData userData;

    [Header("UI")]
    [SerializeField] private TMP_Text totalCoinText;
    [SerializeField] private string prefix = string.Empty;

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
        if (totalCoinText == null)
        {
            return;
        }

        if (userData == null)
        {
            totalCoinText.text = "-";
            return;
        }

        string coinValue = userData.TotalCoins.ToString();
        totalCoinText.text = string.IsNullOrEmpty(prefix) ? coinValue : prefix + coinValue;
    }
}
