using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatUIManager : Singleton<CombatUIManager>
{
    //player
    [SerializeField] private GameObject Player;
    [SerializeField] private TurnManager TurnManager;
    //playerUi Panel
    [SerializeField] private List<GameObject> PlayerUIPanel;
    //skill panel
    [SerializeField] private GameObject SkillList;
    [SerializeField] private GameObject SkillPrefab;
    //Action Button
    [SerializeField] private Button ActionButtonPanal;
    //turn header
    [SerializeField] private TextMeshProUGUI TurnHeaderText;
    [SerializeField] private TextMeshProUGUI HealthText;
    [SerializeField] private TextMeshProUGUI SPText;

    private static Skill SelectedSkill;

    private void Start()
    {
        ActionButtonPanal.onClick.AddListener(() =>
        {
            Debug.Log("Action Button Clicked");
            SkillList.SetActive(!SkillList.activeSelf);
        });
        SkillMapping();
        UpdatePlayerStatsUI();

    }

    private void SkillMapping()
    {
        foreach (Transform child in SkillList.transform)
        {
            Destroy(child.gameObject);
        }
        int index = 0;
        foreach (var skill in Player.GetComponent<PlayerCombat>().GetSkills())
        {
            Debug.Log(skill);
            GameObject skillButton = Instantiate(SkillPrefab, SkillList.transform);
            skillButton.GetComponent<RectTransform>().localPosition = new Vector3(450 - (index * 200), -460, 0);
            skillButton.GetComponentInChildren<TextMeshProUGUI>().text = skill.skillName;
            skillButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                UpdatePlayerStatsUI();
                PlayerCombat PlayerCombat = Player.GetComponent<PlayerCombat>();
                SelectedSkill = skill;
                Debug.Log("Selected Skill: " + SelectedSkill.skillName);
                TurnManager.setSelectedSkill(SelectedSkill);
                TurnManager.ChangingState(TurnState.SpeedCompareState);
            });
            index += 1;
        }
        // Debug.Log(Player.GetComponent<PlayerCombat>().GetPlayerStats().MaxSkillSlot);
    }
    public void SetTurnHeaderText(string text)
    {
        TurnHeaderText.text = text;
    }

    public void SetSkillListActive(bool isActive)
    {
        SkillList.SetActive(isActive);
    }
    public void UpdatePlayerStatsUI()
    {
        Stat playerStats = Player.GetComponent<PlayerCombat>().GetPlayerStats();
        HealthText.text = "HP: " + Math.Max(playerStats.CurrentHealth, 0).ToString("F2") + " / " + playerStats.MaxHealth.ToString("F2");
        SPText.text = "SP: " + Math.Max(playerStats.CurrentSP, 0) + " / " + playerStats.MaxSP;
    }
    public void UpdatePlayerUIActive(bool isActive)
    {
        foreach (var panel in PlayerUIPanel)
        {
            panel.SetActive(isActive);
        }
    }
}