using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarUI : Singleton<HealthbarUI>
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

    private float ForegroundInitialWidth;
    private PlayerCombat player;
    private void Start()
    {
        HealthbarButton.onClick.AddListener(OnHealthbarClicked);

        if (HealthForeground == null)
        {
            Debug.LogError("HealthForeground image is not assigned in the inspector.");
            return;
        }
        if (SPForeground == null)
        {
            Debug.LogError("SPForeground image is not assigned in the inspector.");
            return;
        }
        ForegroundInitialWidth = HealthForeground.rectTransform.sizeDelta.x;
        player = PlayerCombat.instance;
        if (player != null)
        {
            player.OnHealthChanged += UpdateHealthBar;
            player.OnSPChanged += UpdateSPBar;
            player.buffController.OnBuffsChanged += UpdateBuffs;
            
            // Initialization updates
            UpdateHealthBar(player.CurrentHealth, player.GetStat(StatType.MaxHealth));
            UpdateSPBar(player.CurrentSP, (int)player.GetStat(StatType.MaxSkillPoint));
            UpdateBuffs();
        }
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnHealthChanged -= UpdateHealthBar;
            player.OnSPChanged -= UpdateSPBar;
            if (player.buffController != null)
            {
                player.buffController.OnBuffsChanged -= UpdateBuffs;
            }
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
        float spPercent = maxSP > 0 ? (float)currentSP / maxSP : 0;
        SPForeground.rectTransform.sizeDelta = new Vector2(ForegroundInitialWidth * spPercent, SPForeground.rectTransform.sizeDelta.y);
        UpdateInfoText();
    }

    private void UpdateInfoText()
    {
        if (player == null) return;
        HealthInfo.text = $"HP {Math.Max(0, player.CurrentHealth):F2} | SP {Math.Max(0, player.CurrentSP):F2}";
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
        if (PlayerCombat.instance == null) return;
        List<ActiveBuff> Buffs = player.buffController.GetBuffsByType(BuffType.Buff);
        if (Buffs.Count == 0) BuffParent.gameObject.SetActive(false);
        else BuffParent.gameObject.SetActive(true);
        foreach (var buff in Buffs)
        {
            GameObject buffObj = Instantiate(BuffPrefab, BuffParent.transform);
            buffObj.GetComponent<Image>().sprite = buff.Data.Icon;
            buffObj.transform.Find("Duration").GetComponentInChildren<TextMeshProUGUI>().text = BuffStackColor(buff.CurrentDuration) + $"{buff.CurrentDuration}</color>";
            buffObj.transform.Find("Stack").GetComponentInChildren<TextMeshProUGUI>().text = BuffStackColor(buff.CurrentStack) + $"{buff.CurrentStack}</color>";
        }
    }
    public void SetStatusBuff()
    {
        foreach (Transform child in StatusBuffParent.transform)
        {
            Destroy(child.gameObject);
        }
        if (player == null) return;
        List<ActiveBuff> statusBuffs = new List<ActiveBuff>();
        statusBuffs.AddRange(player.buffController.GetBuffsByType(BuffType.CrowdControl));
        statusBuffs.AddRange(player.buffController.GetBuffsByType(BuffType.Debuff));
        
        if (statusBuffs.Count == 0) StatusBuffParent.gameObject.SetActive(false);
        else StatusBuffParent.gameObject.SetActive(true);
        if (statusBuffs.Count == 0) return;
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
        statInfoUI.SetEntity(player);
        statInfoUI.gameObject.SetActive(true);
    }
}
