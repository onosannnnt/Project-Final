using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillPanelUI : Singleton<SkillPanelUI>
{
    [SerializeField] private GameObject SkillButtonPrefab;
    [SerializeField] private GameObject SkillPanel;
    [SerializeField] private Button BackButton;

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
    public void SetUpSkillPanel()
    {
        PlayerCombat playerCombat = PlayerCombat.instance;
        foreach (Transform child in SkillPanel.transform)
        {
            Destroy(child.gameObject);
        }
        int count = playerCombat.skillManager.GetSkills().Count;
        if (count == 0) return;
        foreach (var skill in playerCombat.skillManager.GetSkills())
        {
            GameObject skillButton = Instantiate(SkillButtonPrefab, SkillPanel.transform);
            skillButton.GetComponentInChildren<TextMeshProUGUI>().text = skill.skillName;

            CombatSkillHoverHandler hoverHandler = skillButton.GetComponent<CombatSkillHoverHandler>();
            if (hoverHandler != null)
            {
                hoverHandler.SetSkillData(skill); // ส่งข้อมูลสกิลให้สคริปต์ Hover รู้จัก!
            }           
            
            if (playerCombat.CurrentSP < skill.SkillPoint)
            {
                skillButton.GetComponent<Button>().interactable = false;
            }
            skillButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                playerCombat.SetSelectedSkill(skill);
                playerCombat.HandleSelectSkill();
                TargetingPanel.instance.SetActivePanel(true);
                playerCombat.SetPlayerState(PlayerActionState.Targeting);
            });
        }
    }
}