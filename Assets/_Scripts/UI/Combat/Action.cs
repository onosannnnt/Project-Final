using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class ActionBarUI : Singleton<ActionBarUI>
{
    [SerializeField] private Button SkillsButton;
    [FormerlySerializedAs("InventoryButton")]
    [SerializeField] private Button BasicAttackButton;
    [SerializeField] private Skill basicAttackSkill;
    
    [Header("Anchor Slot Position (Preferred)")]
    [Tooltip("If enabled, panel snaps to the anchor assigned for active player index.")]
    [SerializeField] private bool usePlayerAnchors = true;
    [Tooltip("Assign anchors in order: index 0 = player 1, index 1 = player 2, etc.")]
    [SerializeField] private RectTransform[] playerAnchors;

    [Header("UI Auto Position")]
    [Tooltip("If enabled, panel follows active player's world position on the canvas.")]
    [SerializeField] private bool followActivePlayerWorldPosition = true;
    [Tooltip("Offset applied when following world position.")]
    [SerializeField] private Vector2 worldPositionOffset = new Vector2(0f, -120f);

    [Header("Index Based Position (Fallback)")]
    [Tooltip("Base anchored position for player index 0.")]
    [SerializeField] private Vector2 baseUIPosition = new Vector2(0, 0);
    [Tooltip("Offset applied per party index (0,1,2,...) to avoid manual per-player positions.")]
    [SerializeField] private Vector2 uiOffsetPerPlayer = new Vector2(250, 0);

    private bool handlersSetUp = false;

    protected override void Awake()
    {
        base.Awake();
        SetUpEventHandlers();
    }

    private PlayerEntity lastActivePlayer;

    private void Update()
    {
        PlayerEntity activePlayer = TurnManager.Instance != null ? TurnManager.Instance.CurrentActivePlayer : null;
        
        // If the active player has changed, refresh the panel
        if (activePlayer != lastActivePlayer)
        {
            UpdatePanelPosition();
            UpdateBasicAttackInteractable();
            lastActivePlayer = activePlayer;
        }
        
        // Optional: If you want it to strictly follow the world position every frame 
        // (in case players move), you can call UpdatePanelPosition() here.
        if (followActivePlayerWorldPosition && activePlayer != null)
        {
            UpdatePanelPosition();
        }
    }

    private void OnEnable()
    {
        lastActivePlayer = TurnManager.Instance != null ? TurnManager.Instance.CurrentActivePlayer : null;
        UpdatePanelPosition();
        UpdateBasicAttackInteractable();
    }

    private void UpdateBasicAttackInteractable()
    {
        PlayerEntity activePlayer = TurnManager.Instance != null ? TurnManager.Instance.CurrentActivePlayer : null;
        if (activePlayer == null || basicAttackSkill == null || BasicAttackButton == null) return;

        int cost = TurnManager.Instance.GetSkillCost(basicAttackSkill);
        int availableSP = TurnManager.Instance.GetProjectedAvailableSP(activePlayer);
        BasicAttackButton.interactable = (availableSP >= cost);
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

    private void SetUpEventHandlers()
    {
        if (handlersSetUp) return;

        if (SkillsButton != null)
        {
            SkillsButton.onClick.AddListener(() =>
            {
                PlayerEntity activePlayer = TurnManager.Instance != null ? TurnManager.Instance.CurrentActivePlayer : null;
                if (activePlayer != null)
                {
                    // Cancel basic attack targeting if active
                    if (activePlayer.GetPlayerState == PlayerActionState.Targeting)
                    {
                        activePlayer.SetSelectedSkill(null);
                        activePlayer.SetPlayerState(PlayerActionState.Idle);
                        if (TargetingPanel.instance != null)
                        {
                            TargetingPanel.instance.SetActivePanel(false);
                        }
                        // Reset highlights
                        activePlayer.Highlight(Color.white);
                        foreach (var enemy in FindObjectsOfType<EnemyCombat>())
                        {
                            enemy.Highlight(Color.white);
                        }
                    }
                }

                if (SkillPanelUI.Instance != null)
                {
                    SkillPanelUI.Instance.gameObject.SetActive(true);
                    SkillPanelUI.Instance.SetUpSkillPanel();
                    gameObject.SetActive(false);
                }
            });
        }

        if (BasicAttackButton != null)
        {
            BasicAttackButton.onClick.AddListener(() =>
            {
                PlayerEntity activePlayer = TurnManager.Instance != null ? TurnManager.Instance.CurrentActivePlayer : null;
                if (activePlayer == null || basicAttackSkill == null) return;

                activePlayer.SetSelectedSkill(basicAttackSkill);
                activePlayer.HandleSelectSkill();
                if (TargetingPanel.instance != null)
                {
                    TargetingPanel.instance.SetActivePanel(true);
                }
                activePlayer.SetPlayerState(PlayerActionState.Targeting);
                // Don't hide ActionPanel (ActionBarUI) when targeting basic attack
                // gameObject.SetActive(false); 
            });
        }

        handlersSetUp = true;
    }

}
