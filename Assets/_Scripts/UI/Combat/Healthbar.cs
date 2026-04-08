using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarUI : MonoBehaviour
{
    [SerializeField] private StatInfoUI statInfoUI;
    [SerializeField] private Button HealthbarButton;
    [SerializeField] private Image HealthForeground;
    [SerializeField] private Image SPForeground;
    [SerializeField] private TextMeshProUGUI HealthInfo;
    [SerializeField] private Transform BuffParent;
    [SerializeField] private Transform StatusBuffParent;
    [SerializeField] private GameObject BuffPrefab;
    [SerializeField] private GameObject StatusBuffPrefab;
    [SerializeField] private PlayerEntity targetPlayer;

    private float ForegroundInitialWidth;
    private float SPForegroundInitialWidth;

    private void Start()
    {
        if (HealthbarButton != null) HealthbarButton.onClick.AddListener(OnHealthbarClicked);

        if (HealthForeground == null)
        {
            Debug.LogError("HealthForeground image is not assigned in the inspector.");
            return;
        }

        ForegroundInitialWidth = HealthForeground.rectTransform.sizeDelta.x;
        
        if (SPForeground != null)
        {
            SPForegroundInitialWidth = SPForeground.rectTransform.sizeDelta.x;
        }

        if (targetPlayer == null)
        {
            // Only fall back to main player if it's the main player UI (SPForeground active)
            // If it's your cloned ally UI, we shouldn't force it to be the main player
            if (SPForeground != null)
            {
                SetTargetPlayer(PlayerCombat.instance);
            }
        }
        else
        {
            // Initialize with the targetPlayer already assigned in inspect
            SetTargetPlayer(targetPlayer);
        }
    }

    private void OnDestroy()
    {
        if (targetPlayer != null)
        {
            targetPlayer.OnHealthChanged -= UpdateHealthBar;
            if (SPForeground != null)
            {
                targetPlayer.OnSPChanged -= UpdateSPBar;
            }
            if (targetPlayer.buffController != null)
            {
                targetPlayer.buffController.OnBuffsChanged -= UpdateBuffs;
            }
        }
    }

    public void SetTargetPlayer(PlayerEntity newTarget)
    {
        // Unsubscribe from previous target
        if (targetPlayer != null)
        {
            targetPlayer.OnHealthChanged -= UpdateHealthBar;
            if (SPForeground != null) targetPlayer.OnSPChanged -= UpdateSPBar;
            if (targetPlayer.buffController != null) targetPlayer.buffController.OnBuffsChanged -= UpdateBuffs;
        }

        targetPlayer = newTarget;

        // Subscribe to new target and initialize updates
        if (targetPlayer != null)
        {
            targetPlayer.OnHealthChanged += UpdateHealthBar;
            if (SPForeground != null) targetPlayer.OnSPChanged += UpdateSPBar;
            if (targetPlayer.buffController != null) targetPlayer.buffController.OnBuffsChanged += UpdateBuffs;

            UpdateHealthBar(targetPlayer.CurrentHealth, targetPlayer.GetStat(StatType.MaxHealth));
            if (SPForeground != null)
            {
                UpdateSPBar(targetPlayer.CurrentSP, (int)targetPlayer.GetStat(StatType.MaxSkillPoint));
            }
            UpdateBuffs();
            UpdateInfoText();
        }
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float healthPercent = maxHealth > 0 ? (float)currentHealth / maxHealth : 0;
        HealthForeground.rectTransform.sizeDelta = new Vector2(ForegroundInitialWidth * healthPercent, HealthForeground.rectTransform.sizeDelta.y);
        UpdateInfoText();
    }

    private void UpdateSPBar(int currentSP, int maxSP)
    {
        if (SPForeground == null) return;
        float spPercent = maxSP > 0 ? (float)currentSP / maxSP : 0;
        SPForeground.rectTransform.sizeDelta = new Vector2(SPForegroundInitialWidth * spPercent, SPForeground.rectTransform.sizeDelta.y);
        UpdateInfoText();
    }

    private void UpdateInfoText()
    {
        if (targetPlayer == null || HealthInfo == null) return;
        
        if (SPForeground != null)
        {
            HealthInfo.text = $"HP {Math.Max(0, targetPlayer.CurrentHealth):F0} | SP {Math.Max(0, targetPlayer.CurrentSP):F0}";
        }
        else
        {
            HealthInfo.text = $"HP {Math.Max(0, targetPlayer.CurrentHealth):F0}";
        }
    }

    private void UpdateBuffs()
    {
        SetBuffs();
        SetStatusBuff();
    }

    public void SetBuffs()
    {
        foreach (Transform child in BuffParent.transform)
        {
            Destroy(child.gameObject);
        }
        if (targetPlayer == null || targetPlayer.buffController == null) return;
        List<ActiveBuff> Buffs = targetPlayer.buffController.GetBuffsByType(BuffType.Buff);
        BuffParent.gameObject.SetActive(Buffs.Count > 0);
        foreach (var buff in Buffs)
        {
            GameObject buffObj = Instantiate(BuffPrefab, BuffParent.transform);
            buffObj.GetComponent<Image>().sprite = buff.Data.Icon;
            
            var durationTransform = buffObj.transform.Find("Duration");
            if (durationTransform != null)
            {
                var durationText = durationTransform.GetComponentInChildren<TextMeshProUGUI>();
                if (durationText != null)
                    durationText.text = BuffStackColor(buff.CurrentDuration) + $"{buff.CurrentDuration}</color>";
            }

            var stackTransform = buffObj.transform.Find("Stack");
            if (stackTransform != null)
            {
                var stackText = stackTransform.GetComponentInChildren<TextMeshProUGUI>();
                if (stackText != null)
                    stackText.text = BuffStackColor(buff.CurrentStack) + $"{buff.CurrentStack}</color>";
            }
        }
    }
    
    public void SetStatusBuff()
    {
        foreach (Transform child in StatusBuffParent.transform)
        {
            Destroy(child.gameObject);
        }
        if (targetPlayer == null || targetPlayer.buffController == null) return;
        List<ActiveBuff> statusBuffs = new List<ActiveBuff>();
        statusBuffs.AddRange(targetPlayer.buffController.GetBuffsByType(BuffType.CrowdControl));
        statusBuffs.AddRange(targetPlayer.buffController.GetBuffsByType(BuffType.Debuff));
        
        StatusBuffParent.gameObject.SetActive(statusBuffs.Count > 0);
        foreach (var buff in statusBuffs)
        {
            GameObject buffObj = Instantiate(StatusBuffPrefab, StatusBuffParent.transform);
            buffObj.GetComponent<Image>().sprite = buff.Data.Icon;
        }
    }

    private string BuffStackColor(int stack)
    {
        if (stack >= 5)
            return "<color=green>";
        else if (stack >= 3)
            return "<color=yellow>";
        else
            return "<color=red>";
    }
    
    private void OnHealthbarClicked()
    {
        if (statInfoUI != null)
        {
            statInfoUI.SetEntity(targetPlayer);
            statInfoUI.gameObject.SetActive(true);
        }
    }
}
