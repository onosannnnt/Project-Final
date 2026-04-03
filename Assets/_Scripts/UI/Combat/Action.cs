using UnityEngine;
using UnityEngine.UI;

public class ActionBarUI : Singleton<ActionBarUI>
{
    [SerializeField] private Button SkillsButton;
    [SerializeField] private Button InventoryButton;
    public Vector3 offset = new Vector3(0f, 2f, 0f); // Float directly above character in 3D

    private void Start()
    {
        SetUpEventHandlers();
    }

    private void Update()
    {
        if (TurnManager.Instance != null && TurnManager.Instance.CurrentActivePlayer != null && Camera.main != null)
        {
            // Calculate screen position from 3D world position + offset height
            Vector3 worldPos = TurnManager.Instance.CurrentActivePlayer.transform.position + offset;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            // Prevent UI from rendering behind the camera or glitching out
            if (screenPos.z > 0)
            {
                transform.position = screenPos;
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
            Debug.Log("Inventory Button Clicked");
        });
    }

}