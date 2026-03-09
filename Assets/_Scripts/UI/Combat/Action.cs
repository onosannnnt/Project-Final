using UnityEngine;
using UnityEngine.UI;

public class ActionBarUI : Singleton<ActionBarUI>
{
    [SerializeField] private Button SkillsButton;
    [SerializeField] private Button InventoryButton;

    private void Start()
    {
        SetUpEventHandlers();
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