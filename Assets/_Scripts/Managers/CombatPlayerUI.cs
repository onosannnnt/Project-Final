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
    [SerializeField] private Button inventoryButton;
    [SerializeField] private GameObject SkillPanel;
    [SerializeField] private GameObject SkillPrefab;
    private void Start()
    {
        SetUpEventHandlers();
    }
    private void Update()
    {
        UpdateHealthBar();
        UpdateSPBar();
    }
    private void SetUpEventHandlers()
    {
        skillsButton.onClick.AddListener(() =>
        {
            SkillPanel.SetActive(true);
            actionPanel.SetActive(false);
            SetUpSkillPanel();
        });
        inventoryButton.onClick.AddListener(() =>
        {
            Debug.Log("Inventory Button Clicked");
        });
    }

    public void UpdateHealthBar()
    {
        healthBar.GetComponent<Image>().fillAmount = playerCombat.CurrentHealth / playerCombat.Stats.MaxHealth;
    }
    public void UpdateSPBar()
    {
        spBar.GetComponent<Image>().fillAmount = (float)playerCombat.CurrentSP / playerCombat.Stats.MaxSkillPoint;
    }
    private void SetUpSkillPanel()
    {
        PlayerCombat playerCombat = PlayerCombat.instance;
        foreach (Transform child in SkillPanel.transform)
        {
            if (child.gameObject.name != "Back")
                Destroy(child.gameObject);
        }
        int count = playerCombat.skillManager.GetSkills().Count;
        if (count == 0) return;
        float spacing = 150f;
        int mid = count / 2;
        int index = 0;
        foreach (var skill in playerCombat.skillManager.GetSkills())
        {
            float x = (index - mid) * spacing;
            if (count % 2 == 0)
            {
                x += spacing * 0.5f;
            }
            GameObject skillButton = Instantiate(SkillPrefab, SkillPanel.transform);
            skillButton.GetComponent<RectTransform>().localPosition = new Vector3(x, 0, 0);
            skillButton.GetComponentInChildren<TextMeshProUGUI>().text = skill.name;
            if (playerCombat.CurrentSP < skill.SkillPoint)
            {
                skillButton.GetComponent<Button>().interactable = false;
            }
            skillButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                playerCombat.SetSelectedSkill(skill);
                playerCombat.HandleSelectSkill();
                playerCombat.SetPlayerState(PlayerActionState.Targeting);
            });
            index += 1;
        }
    }
    public void OnBackButtonClicked()
    {
        SkillPanel.SetActive(false);
        actionPanel.SetActive(true);
        PlayerCombat.instance.SetPlayerState(PlayerActionState.Idle);
    }
    public void SetSkillPanelActive(bool isActive)
    {
        SkillPanel.SetActive(isActive);
    }
    public void SetActionPanelActive(bool isActive)
    {
        actionPanel.SetActive(isActive);
    }
}