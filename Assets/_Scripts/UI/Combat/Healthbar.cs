using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarUI : Singleton<HealthbarUI>
{
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
    }
    private void Update()
    {
        if (PlayerCombat.instance == null) return;

        float healthPercent = (float)player.CurrentHealth / PlayerCombat.instance.Stats.MaxHealth;
        HealthForeground.rectTransform.sizeDelta = new Vector2(ForegroundInitialWidth * healthPercent, HealthForeground.rectTransform.sizeDelta.y);

        float spPercent = (float)player.CurrentSP / PlayerCombat.instance.Stats.MaxSkillPoint;
        SPForeground.rectTransform.sizeDelta = new Vector2(ForegroundInitialWidth * spPercent, SPForeground.rectTransform.sizeDelta.y);

        HealthInfo.text = $"HP {Math.Max(0, player.CurrentHealth)} | SP {Math.Max(0, player.CurrentSP)}";

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
        List<Buff> Buffs = player.buffController.GetBuffsByType(BuffType.CrowdControl);
        if (Buffs.Count == 0) BuffParent.gameObject.SetActive(false);
        else BuffParent.gameObject.SetActive(true);
        foreach (var buff in Buffs)
        {
            GameObject buffObj = Instantiate(BuffPrefab, BuffParent.transform);
            buffObj.GetComponent<Image>().sprite = buff.Icon;
            buffObj.transform.Find("Duration").GetComponentInChildren<TextMeshProUGUI>().text = BuffStackColor(buff.Duration) + $"{buff.Duration}</color>";
            buffObj.transform.Find("Stack").GetComponentInChildren<TextMeshProUGUI>().text = BuffStackColor(buff.Stack) + $"{buff.Stack}</color>";
        }
    }
    public void SetStatusBuff()
    {
        foreach (Transform child in StatusBuffParent.transform)
        {
            Destroy(child.gameObject);
        }
        if (player == null) return;
        List<Buff> statusBuffs = player.buffController.GetBuffsByType(BuffType.CrowdControl);
        if (statusBuffs.Count == 0) StatusBuffParent.gameObject.SetActive(false);
        else StatusBuffParent.gameObject.SetActive(true);
        if (statusBuffs.Count == 0) return;
        foreach (var buff in statusBuffs)
        {
            GameObject buffObj = Instantiate(StatusBuffPrefab, StatusBuffParent.transform);
            buffObj.GetComponent<Image>().sprite = buff.Icon;
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
}