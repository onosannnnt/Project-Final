using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillPanelUI : Singleton<SkillPanelUI>
{
    [SerializeField] private GameObject SkillButtonPrefab;
    [SerializeField] private GameObject SkillPanel;
    [SerializeField] private Button BackButton;
    [Header("Tracking Settings")]
    public Vector3 worldOffset = new Vector3(0f, 2f, 0f); // How high above the character in 3D
    public Vector2 screenOffset = new Vector2(0f, 0f);      // Extra UI pixels to shift

    private void Start()
    {
        BackButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            ActionBarUI.Instance.gameObject.SetActive(true);
            PlayerCombat.instance.SetPlayerState(PlayerActionState.Idle);
            PlayerCombat.instance.SetSelectedSkill(null);
            TargetingPanel.instance.SetActivePanel(false);

        });
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (TurnManager.Instance != null && TurnManager.Instance.CurrentActivePlayer != null && Camera.main != null)
        {
            // Calculate screen position from 3D world position + offset height
            Vector3 worldPos = TurnManager.Instance.CurrentActivePlayer.transform.position + worldOffset;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            
            // Prevent UI from rendering behind the camera
            if (screenPos.z > 0)
            {
                screenPos.x += screenOffset.x;
                screenPos.y += screenOffset.y;
                
                // Direct assignment instead of Lerp to stop weird sliding issues
                transform.position = screenPos;
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
                PlayerCombat.instance.SetSelectedSkill(skill);
                PlayerCombat.instance.HandleSelectSkill();
                TargetingPanel.instance.SetActivePanel(true);
                PlayerCombat.instance.SetPlayerState(PlayerActionState.Targeting);
            });
        }
    }
}