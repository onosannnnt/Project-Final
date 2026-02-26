using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarUI : Singleton<HealthbarUI>
{
    [SerializeField] private Image HealthForeground;
    [SerializeField] private Image SPForeground;
    [SerializeField] private TextMeshProUGUI HealthInfo;

    private float ForegroundInitialWidth;
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
    }
    private void Update()
    {
        if (PlayerCombat.instance == null) return;

        float healthPercent = (float)PlayerCombat.instance.CurrentHealth / PlayerCombat.instance.Stats.MaxHealth;
        HealthForeground.rectTransform.sizeDelta = new Vector2(ForegroundInitialWidth * healthPercent, HealthForeground.rectTransform.sizeDelta.y);

        float spPercent = (float)PlayerCombat.instance.CurrentSP / PlayerCombat.instance.Stats.MaxSkillPoint;
        SPForeground.rectTransform.sizeDelta = new Vector2(ForegroundInitialWidth * spPercent, SPForeground.rectTransform.sizeDelta.y);

        HealthInfo.text = $"HP {Math.Max(0, PlayerCombat.instance.CurrentHealth)} | SP {Math.Max(0, PlayerCombat.instance.CurrentSP)}";
    }
}