using UnityEngine;
using UnityEngine.UI;

public class ActionBarUI : Singleton<ActionBarUI>
{
    [SerializeField] private Button SkillsButton;
    [SerializeField] private Button InventoryButton;
    
    [Header("UI Positions")]
    [Tooltip("Anchored position for the Main Player")]
    public Vector2 mainPlayerUIPosition = new Vector2(0, 0);
    [Tooltip("Anchored position for the Ally")]
    public Vector2 allyUIPosition = new Vector2(250, 0);

    private void Start()
    {
        SetUpEventHandlers();
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
                // If it's the main player vs an ally
                bool isMainPlayer = TurnManager.Instance.CurrentActivePlayer.GetComponent<PlayerCombat>() != null;
                rt.anchoredPosition = isMainPlayer ? mainPlayerUIPosition : allyUIPosition;
            }
        }
    }

    private void SetUpEventHandlers()
    {
        SkillsButton.onClick.AddListener(() =>
        {
            SkillPanelUI.Instance.gameObject.SetActive(true);
            SkillPanelUI.Instance.SetUpSkillPanel();
            gameObject.SetActive(false);
        });
        InventoryButton.onClick.AddListener(() =>
        {
// // Debug.Log("Inventory Button Clicked");
        });
    }

}
