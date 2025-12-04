using UnityEngine;
using UnityEngine.UI;

public class CombatPlayerUI : Singleton<CombatPlayerUI>
{
    [SerializeField] private PlayerCombat playerCombat;
    [Header("UI Elements")]
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject spBar;

    private void Start()
    {

    }

    public void UpdateHealthBar()
    {
        healthBar.GetComponent<Image>().fillAmount = playerCombat.CurrentHealth / playerCombat.Stats.MaxHealth;
    }
    public void UpdateSPBar()
    {
        spBar.GetComponent<Image>().fillAmount = playerCombat.CurrentSP / playerCombat.Stats.MaxSkillPoint;
    }
}