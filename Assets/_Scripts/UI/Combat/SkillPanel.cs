using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillPanelUI : Singleton<SkillPanelUI>
{
    [SerializeField] private GameObject SkillButtonPrefab;
    [SerializeField] private GameObject SkillPanel;
    [SerializeField] private Button BackButton;
    
    [Header("UI Positions")]
    [Tooltip("Anchored position for the Main Player")]
    public Vector2 mainPlayerUIPosition = new Vector2(0, 0);
    [Tooltip("Anchored position for the Ally")]
    public Vector2 allyUIPosition = new Vector2(250, 0);

    private void Start()
    {
        BackButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            ActionBarUI.Instance.gameObject.SetActive(true);
            var activeEntity = TurnManager.Instance.CurrentActivePlayer as PlayerEntity ?? PlayerCombat.instance;
            activeEntity.SetPlayerState(PlayerActionState.Idle);
            activeEntity.SetSelectedSkill(null);
            TargetingPanel.instance.SetActivePanel(false);

        });
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        UpdatePanelPosition();
    }

    private void UpdatePanelPosition()
    {
        if (TurnManager.Instance != null && TurnManager.Instance.CurrentActivePlayer != null)
        {
            RectTransform rt = GetComponent<RectTransform>();
            if (rt != null)
            {
                bool isMainPlayer = TurnManager.Instance.CurrentActivePlayer.GetComponent<PlayerCombat>() != null;
                rt.anchoredPosition = isMainPlayer ? mainPlayerUIPosition : allyUIPosition;
            }
        }
    }

    public void SetUpSkillPanel()
    {
        Entity activePlayer = TurnManager.Instance.CurrentActivePlayer;
        if (activePlayer == null) activePlayer = PlayerCombat.instance;

        foreach (Transform child in SkillPanel.transform)
        {
            Destroy(child.gameObject);
        }
        int count = activePlayer.skillManager.GetSkills().Count;
        if (count == 0) return;
        foreach (var skill in activePlayer.skillManager.GetSkills())
        {
            GameObject skillButton = Instantiate(SkillButtonPrefab, SkillPanel.transform);
            skillButton.GetComponentInChildren<TextMeshProUGUI>().text = skill.skillName;

            CombatSkillHoverHandler hoverHandler = skillButton.GetComponent<CombatSkillHoverHandler>();
            if (hoverHandler != null)
            {
                hoverHandler.SetSkillData(skill); 
            }           
            
            if (activePlayer.CurrentSP < skill.SkillPoint)
            {
                skillButton.GetComponent<Button>().interactable = false;
            }
            skillButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                var activeEntity = TurnManager.Instance.CurrentActivePlayer as PlayerEntity ?? PlayerCombat.instance;
                activeEntity.SetSelectedSkill(skill);
                activeEntity.HandleSelectSkill();
                TargetingPanel.instance.SetActivePanel(true);
                activeEntity.SetPlayerState(PlayerActionState.Targeting);
            });
        }
    }
}
