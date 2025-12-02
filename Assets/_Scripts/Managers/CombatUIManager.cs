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
    [SerializeField] private GameObject ActionPanel;
    //Action Button
    [SerializeField] private Button ActionButtonPanal;
    [SerializeField] private Button InventoryButtonPanal;
    [SerializeField] private Button SkillGobackButton;

    //turn header
    [SerializeField] private TextMeshProUGUI TurnHeaderText;
    [SerializeField] private Image HealthImage;
    [SerializeField] private TextMeshProUGUI SPText;

    private static Skill SelectedSkill;

    private void Start()
    {
        ActionButtonPanal.onClick.AddListener(() =>
        {
            Debug.Log("Action Button Clicked");
            SkillList.SetActive(!SkillList.activeSelf);
            ActionPanel.SetActive(false);
        });
        InventoryButtonPanal.onClick.AddListener(() =>
        {
            Debug.Log("Inventory Button Clicked");
            InventoryButtonPanal.gameObject.SetActive(false);
            // Toggle Inventory Panel here
        });
        SkillGobackButton.onClick.AddListener(() =>
        {
            SkillList.SetActive(false);
            ActionPanel.gameObject.SetActive(true);
        });
        SkillList.SetActive(false);
        SkillMapping();
        UpdatePlayerStatsUI();

    }

    private void SkillMapping()
    {
        foreach (Transform child in SkillList.transform)
        {
            if (child.gameObject.name != "Back")
                Destroy(child.gameObject);
        }
        int count = Player.GetComponent<PlayerCombat>().GetSkills().Count;
        float spacing = 20f;
        int mid = count / 2;
        int index = 0;
        foreach (var skill in Player.GetComponent<PlayerCombat>().GetSkills())
        {
            float x = (index - mid) * spacing;
            if (count % 2 == 0)
            {
                x += spacing * 0.5f;
            }
            GameObject skillButton = Instantiate(SkillPrefab, SkillList.transform);
            skillButton.GetComponent<RectTransform>().localPosition = new Vector3(x, 0, 0);
            skillButton.GetComponentInChildren<TextMeshProUGUI>().text = skill.skillName;
            if (Player.GetComponent<PlayerCombat>().GetPlayerStats().CurrentSP < skill.spCost)
            {
                skillButton.GetComponent<Button>().interactable = false;
            }
            skillButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            UpdatePlayerStatsUI();
            PlayerCombat PlayerCombat = Player.GetComponent<PlayerCombat>();
            SelectedSkill = skill;
            Debug.Log("Selected Skill: " + SelectedSkill.skillName);
            TurnManager.SetSelectedSkill(SelectedSkill);
            if (SelectedSkill.targetType == TargetType.SingleEnemy)
            {
                PlayerCombat.HandleSingleEnemyTargeting(SelectedSkill);
            }
            else
            {
                TurnManager.ChangingState(TurnState.SpeedCompareState);
            }
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
        // HealthText.text = "HP: " + Math.Max(playerStats.CurrentHealth, 0).ToString("F2") + " / " + playerStats.MaxHealth.ToString("F2");
        HealthImage.fillAmount = playerStats.CurrentHealth / playerStats.MaxHealth;
        // SPText.text = "SP: " + Math.Max(playerStats.CurrentSP, 0) + " / " + playerStats.MaxSP;
    }
    public void UpdatePlayerUIActive(bool isActive)
    {
        foreach (var panel in PlayerUIPanel)
        {
            panel.SetActive(isActive);
        }
        SkillMapping();
    }
}