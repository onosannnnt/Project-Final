using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillPanelUI : Singleton<SkillPanelUI>
{
    [SerializeField] private GameObject SkillButtonPrefab;
    [SerializeField] private GameObject SkillPanel;
    [SerializeField] private Button BackButton;
    
    [Header("Anchor Slot Position (Preferred)")]
    [Tooltip("If enabled, panel snaps to the anchor assigned for active player index.")]
    [SerializeField] private bool usePlayerAnchors = true;
    [Tooltip("Assign anchors in order: index 0 = player 1, index 1 = player 2, etc.")]
    [SerializeField] private RectTransform[] playerAnchors;

    [Header("UI Auto Position")]
    [Tooltip("If enabled, panel follows active player's world position on the canvas.")]
    [SerializeField] private bool followActivePlayerWorldPosition = true;
    [Tooltip("Offset applied when following world position.")]
    [SerializeField] private Vector2 worldPositionOffset = new Vector2(0f, -20f);

    [Header("Index Based Position (Fallback)")]
    [Tooltip("Base anchored position for player index 0.")]
    [SerializeField] private Vector2 baseUIPosition = new Vector2(0, 0);
    [Tooltip("Offset applied per party index (0,1,2,...) to avoid manual per-player positions.")]
    [SerializeField] private Vector2 uiOffsetPerPlayer = new Vector2(250, 0);

    private void Start()
    {
        BackButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            ActionBarUI.Instance.gameObject.SetActive(true);
            PlayerEntity activeEntity = TurnManager.Instance != null ? TurnManager.Instance.CurrentActivePlayer : null;
            if (activeEntity != null)
            {
                activeEntity.SetPlayerState(PlayerActionState.Idle);
                activeEntity.SetSelectedSkill(null);
            }
            if (TargetingPanel.instance != null)
            {
                TargetingPanel.instance.SetActivePanel(false);
            }

        });
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        UpdatePanelPosition();
    }

    private void UpdatePanelPosition()
    {
        if (TurnManager.Instance == null || TurnManager.Instance.CurrentActivePlayer == null)
        {
            return;
        }

        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null)
        {
            return;
        }

        int playerIndex = 0;
        if (PlayerTeamManager.Instance != null)
        {
            int resolvedIndex = PlayerTeamManager.Instance.IndexOfMember(TurnManager.Instance.CurrentActivePlayer);
            if (resolvedIndex >= 0)
            {
                playerIndex = resolvedIndex;
            }
        }

        if (usePlayerAnchors && playerAnchors != null && playerIndex >= 0 && playerIndex < playerAnchors.Length)
        {
            RectTransform anchor = playerAnchors[playerIndex];
            if (anchor != null)
            {
                rt.position = anchor.position;
                return;
            }
        }

        if (followActivePlayerWorldPosition)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            RectTransform canvasRect = canvas != null ? canvas.transform as RectTransform : null;
            Transform activeTransform = TurnManager.Instance.CurrentActivePlayer.transform;

            if (canvasRect != null && activeTransform != null)
            {
                Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, activeTransform.position);
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, cam, out Vector2 localPoint))
                {
                    rt.anchoredPosition = localPoint + worldPositionOffset;
                    return;
                }
            }
        }

        rt.anchoredPosition = baseUIPosition + (uiOffsetPerPlayer * playerIndex);
    }

    public void SetUpSkillPanel()
    {
        PlayerEntity activePlayer = TurnManager.Instance != null ? TurnManager.Instance.CurrentActivePlayer : null;
        if (activePlayer == null) return;

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
                PlayerEntity activeEntity = TurnManager.Instance != null ? TurnManager.Instance.CurrentActivePlayer : null;
                if (activeEntity == null) return;
                activeEntity.SetSelectedSkill(skill);
                activeEntity.HandleSelectSkill();
                if (TargetingPanel.instance != null)
                {
                    TargetingPanel.instance.SetActivePanel(true);
                }
                activeEntity.SetPlayerState(PlayerActionState.Targeting);
            });
        }
    }
}
