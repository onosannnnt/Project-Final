using System;
using System.Collections;
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
    [Header("Auto Binding")]
    [SerializeField] private bool autoBindFromTeam = true;
    [SerializeField] private int teamMemberIndex = 0;
    [SerializeField] private float autoBindTimeoutSeconds = 3f;

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
            if (autoBindFromTeam)
            {
                StartCoroutine(BindTargetFromTeamWhenReady());
            }
            else if (SPForeground != null && teamMemberIndex == 0 && PlayerCombat.instance != null)
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

    private IEnumerator BindTargetFromTeamWhenReady()
    {
        float elapsed = 0f;

        while (elapsed < autoBindTimeoutSeconds)
        {
            if (PlayerTeamManager.Instance != null && teamMemberIndex >= 0)
            {
                PlayerEntity teamMember = PlayerTeamManager.Instance.GetMemberAt(teamMemberIndex);
                if (teamMember != null)
                {
                    SetTargetPlayer(teamMember);
                    yield break;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Legacy fallback only for main player bar
        if (targetPlayer == null && teamMemberIndex == 0 && PlayerCombat.instance != null)
        {
            SetTargetPlayer(PlayerCombat.instance);
            yield break;
        }

        if (targetPlayer == null)
        {
            Debug.LogWarning($"[HealthbarUI] Failed to bind team member index {teamMemberIndex}. Check PlayerTeamManager slots and bar index setup.");
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
        // List<ActiveBuff> statusBuffs = new List<ActiveBuff>();
        // List<ActiveBuff> Buffs = targetPlayer.buffController.GetBuffsByType(BuffType.Buff);
        List<ActiveBuff> Buffs = new List<ActiveBuff>();
        Buffs.AddRange(targetPlayer.buffController.GetBuffsByType(BuffType.Buff));
        Buffs.AddRange(targetPlayer.buffController.GetBuffsByType(BuffType.Debuff));
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
