using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatInfoUI : MonoBehaviour
{
    [SerializeField] private Button ExitButton;
    [SerializeField] private TMP_Text BuffHeaderText;
    [SerializeField] private TMP_Text DebuffHeaderText;
    [SerializeField] private Transform BuffTransform;
    [SerializeField] private Transform DebuffTransform;
    [SerializeField] private GameObject BuffCardPrefab;
    [SerializeField] private Image HealthbarFG;
    [SerializeField] private TMP_Text HealthText;
    [SerializeField] private Image SkillPointBarFG;
    [SerializeField] private TMP_Text SkillPointText;
    [SerializeField] private Transform StatusBuffParent;
    [SerializeField] private GameObject StatusBuffPrefab;

    [Header("Visibility Settings")]
    [SerializeField] private GameObject customObjectToHide;
    [SerializeField] private bool hideCustomObject = false;

    private Entity Entity;
    private List<ActiveBuff> Buffs;
    private List<ActiveBuff> Debuffs;
    private float maxWitdthHealthbar;
    private float maxWitdthSkillPointBar;
    private void Start()
    {
        gameObject.SetActive(false);
        ExitButton.onClick.AddListener(OnExitClicked);
        if (HealthbarFG != null) maxWitdthHealthbar = HealthbarFG.rectTransform.sizeDelta.x;
        if (SkillPointBarFG != null) maxWitdthSkillPointBar = SkillPointBarFG.rectTransform.sizeDelta.x;
    }
    private void Update()
    {
        if (Entity == null) return;
        if (TurnManager.Instance.GetTurnState() != TurnState.PlayerTurnState) gameObject.SetActive(false);
        SetupBuffPanel();
        SetupHealthBar();
        SetupSkillPointBar();
        SetStatusBuff();
    }
    private void OnEnable()
    {
        if (hideCustomObject && customObjectToHide != null)
        {
            customObjectToHide.SetActive(false);
        }
    }
    private void OnDisable()
    {
        if (hideCustomObject && customObjectToHide != null)
        {
            customObjectToHide.SetActive(true);
        }
    }
    public void SetEntity(Entity entity)
    {
        Entity = entity;
    }

    private void SetupBuffPanel()
    {
        if (BuffCardPrefab == null || BuffTransform == null || DebuffTransform == null) return;
        Buffs = Entity.buffController.GetBuffs();

        Debuffs = new List<ActiveBuff>();
        Debuffs.AddRange(Entity.buffController.GetDebuffs());
        Debuffs.AddRange(Entity.buffController.GetBuffsByType(BuffType.CrowdControl));

        BuffHeaderText.text = $"Buffs: {Buffs.Count}";
        DebuffHeaderText.text = $"Debuffs: {Debuffs.Count}";

        foreach (Transform child in BuffTransform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in DebuffTransform)
        {
            Destroy(child.gameObject);
        }

        if (Buffs.Count > 0)
        {
            foreach (var buff in Buffs)
            {
                GameObject buffGO = Instantiate(BuffCardPrefab, BuffTransform);
                BuffItemUI ui = buffGO.GetComponent<BuffItemUI>();
                // ui.Setup($"{buff.Data.BuffName}({buff.CurrentDuration})", buff.Data.Description, buff.Data.Icon, buff.CurrentStack);
                ui.Setup($"{buff.Data.BuffName}", buff.Data.Description, buff.Data.Icon, buff.CurrentStack, buff.CurrentDuration, buff.Data.isPermanent);
            }
        }

        if (Debuffs.Count > 0)
        {
            foreach (var debuff in Debuffs)
            {
                GameObject debuffGO = Instantiate(BuffCardPrefab, DebuffTransform);
                BuffItemUI ui = debuffGO.GetComponent<BuffItemUI>();
                // ui.Setup($"{debuff.Data.BuffName}({debuff.CurrentDuration})", debuff.Data.Description, debuff.Data.Icon, debuff.CurrentStack);
                ui.Setup($"{debuff.Data.BuffName}", debuff.Data.Description, debuff.Data.Icon, debuff.CurrentStack, debuff.CurrentDuration, debuff.Data.isPermanent);
            }
        }
    }
    private void OnExitClicked()
    {
        gameObject.SetActive(false);
    }
    private void SetupHealthBar()
    {
        if (HealthbarFG == null) return;

        float healthRatio = (float)Entity.CurrentHealth / Entity.GetStat(StatType.MaxHealth);
        HealthbarFG.rectTransform.sizeDelta = new Vector2(maxWitdthHealthbar * healthRatio, HealthbarFG.rectTransform.sizeDelta.y);

        HealthText.text = $"{(int)Entity.CurrentHealth} / {(int)Entity.GetStat(StatType.MaxHealth)}";
    }
    private void SetupSkillPointBar()
    {
        if (SkillPointBarFG == null) return;

        // Hide SP bar for entities that aren't players (like enemies)
        if (Entity is not PlayerEntity)
        {
            SkillPointBarFG.transform.parent.gameObject.SetActive(false);
            return;
        }

        SkillPointBarFG.transform.parent.gameObject.SetActive(true);

        int currentSP = Entity.CurrentSP;
        int maxSP = (int)Entity.GetStat(StatType.MaxSkillPoint);

        // If using shared SP pool, override values
        if (TurnManager.Instance != null && TurnManager.Instance.UseSharedPlayerSkillPointPool)
        {
            currentSP = TurnManager.Instance.ResourceManager.GetSharedPlayerCurrentSkillPoints();
            maxSP = TurnManager.Instance.ResourceManager.GetSharedPlayerMaxSkillPoints();
        }

        float spRatio = maxSP > 0 ? (float)currentSP / maxSP : 0;
        SkillPointBarFG.rectTransform.sizeDelta = new Vector2(maxWitdthSkillPointBar * spRatio, SkillPointBarFG.rectTransform.sizeDelta.y);

        SkillPointText.text = $"{currentSP} / {maxSP}";
    }
    private void SetStatusBuff()
    {
        if (StatusBuffParent == null) return;
        foreach (Transform child in StatusBuffParent)
        {
            Destroy(child.gameObject);
        }
        if (Entity == null) return;

        List<ActiveBuff> statusBuffs = new List<ActiveBuff>();
        statusBuffs.AddRange(Entity.buffController.GetBuffsByType(BuffType.CrowdControl));
        statusBuffs.AddRange(Entity.buffController.GetBuffsByType(BuffType.Debuff));

        if (statusBuffs.Count == 0) StatusBuffParent.gameObject.SetActive(false);
        else StatusBuffParent.gameObject.SetActive(true);
        if (statusBuffs.Count == 0) return;
        foreach (var buff in statusBuffs)
        {
            GameObject buffObj = Instantiate(StatusBuffPrefab, StatusBuffParent.transform);
            buffObj.GetComponent<Image>().sprite = buff.Data.Icon;
        }
    }
    private string BuffStackColor(int stack)
    {
        if (stack >= 5)
            return "<color=green>";
        else if (stack >= 3)
            return "<color=yellow>";
        else
            return "<color=red>";
    }
}
