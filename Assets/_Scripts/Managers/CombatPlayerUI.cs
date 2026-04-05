using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatPlayerUI : Singleton<CombatPlayerUI>
{
    [SerializeField] private PlayerCombat playerCombat;
    [Header("UI Elements")]
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject spBar;
    [SerializeField] private GameObject actionPanel;
    [SerializeField] private Button skillsButton;
    [SerializeField] private GameObject SkillList;
    [SerializeField] private GameObject SkillPrefab;

    [Header("UI Positions")]
    [Tooltip("Anchored position of the Action/Skill panels for the Main Player")]
    [SerializeField] private Vector2 mainPlayerUIPosition = new Vector2(0, 0);
    [Tooltip("Anchored position of the Action/Skill panels for the Ally")]
    [SerializeField] private Vector2 allyUIPosition = new Vector2(250, 0);

    private void Start()
    {
        SetUpEventHandlers();
    }
    // private void Update()
    // {
    //     UpdateHealthBar();
    //     UpdateSPBar();
    // }

    private void SetUpEventHandlers()
    {
        skillsButton.onClick.AddListener(() =>
        {
            SkillList.SetActive(true);
            actionPanel.SetActive(false);
            SetUpSkillPanel();
        });
    }

    public void UpdateHealthBar()
    {
        healthBar.GetComponent<Image>().fillAmount = playerCombat.CurrentHealth / playerCombat.GetStat(StatType.MaxHealth);
    }
    public void UpdateSPBar()
    {
        spBar.GetComponent<Image>().fillAmount = (float)playerCombat.CurrentSP / playerCombat.GetStat(StatType.MaxSkillPoint);
    }
    private void SetUpSkillPanel()
    {
        Entity currentPlayer = TurnManager.Instance?.CurrentActivePlayer ?? playerCombat;
        
        foreach (Transform child in SkillList.transform)
        {
            if (child.gameObject.name != "Back")
                Destroy(child.gameObject);
        }
        int count = currentPlayer.skillManager.GetSkills().Count;
        if (count == 0) return;
        float spacing = 150f;
        int mid = count / 2;
        int index = 0;
        foreach (var skill in currentPlayer.skillManager.GetSkills())
        {
            float x = (index - mid) * spacing;
            if (count % 2 == 0)
            {
                x += spacing * 0.5f;
            }
            GameObject skillButton = Instantiate(SkillPrefab, SkillList.transform);
            skillButton.GetComponent<RectTransform>().localPosition = new Vector3(x, 0, 0);
            skillButton.GetComponentInChildren<TextMeshProUGUI>().text = skill.name;
            
            if (playerCombat.CurrentSP < skill.SkillPoint) // TODO: Assuming SP is shared, otherwise use currentPlayer's SP
            {
                skillButton.GetComponent<Button>().interactable = false;
            }
            skillButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                var activeEntity = TurnManager.Instance.CurrentActivePlayer as PlayerEntity ?? playerCombat;
                activeEntity.SetSelectedSkill(skill);
                activeEntity.HandleSelectSkill();
                TargetingPanel.instance.SetActivePanel(true);
                activeEntity.SetPlayerState(PlayerActionState.Targeting);
            });
            index += 1;
        }
    }
    public void OnBackButtonClicked()
    {
        SkillList.SetActive(false);
        actionPanel.SetActive(true);
        var activeEntity = TurnManager.Instance.CurrentActivePlayer as PlayerEntity ?? PlayerCombat.instance;
        activeEntity.SetPlayerState(PlayerActionState.Idle);
    }

    public void SetSkillPanelActive(bool isActive)
    {
        SkillList.SetActive(isActive);
        if (isActive)
            UpdatePanelPosition(SkillList);
    }

    public void SetActionPanelActive(bool isActive)
    {
        actionPanel.SetActive(isActive);
        if (isActive)
            UpdatePanelPosition(actionPanel);
    }

    private void UpdatePanelPosition(GameObject panel)
    {
        Entity currentPlayer = TurnManager.Instance?.CurrentActivePlayer ?? playerCombat;
        RectTransform rt = panel.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = (currentPlayer == playerCombat) ? mainPlayerUIPosition : allyUIPosition;
        }
    }
}
