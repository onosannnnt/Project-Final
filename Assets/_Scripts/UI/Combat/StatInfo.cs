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
    private float maxWitdthHealthbar = -1f;
    private float maxWitdthSkillPointBar = -1f;

    private void Awake()
    {
        if (ExitButton != null) ExitButton.onClick.AddListener(OnExitClicked);
    }

    private void Start()
    {
        // Initial capture attempt
        CaptureInitialDimensions();
        
        // Default to hidden if not already managed by something else
        // gameObject.SetActive(false); 
        // Note: The user's original script had gameObject.SetActive(false) in Start.
        // If this is causing issues with Start not running when expected, we should be careful.
    }

    private void OnEnable()
    {
        if (hideCustomObject && customObjectToHide != null)
        {
            customObjectToHide.SetActive(false);
        }

        CaptureInitialDimensions();
        
        if (Entity != null)
        {
            SubscribeEvents();
            RefreshAll();
        }
    }

    private void OnDisable()
    {
        if (hideCustomObject && customObjectToHide != null)
        {
            customObjectToHide.SetActive(true);
        }

        UnsubscribeEvents();
    }

    private void Update()
    {
        if (Entity == null) return;
        
        // Auto-close if it's not player turn anymore (as per original logic)
        if (TurnManager.Instance != null && TurnManager.Instance.GetTurnState() != TurnState.PlayerTurnState)
        {
            gameObject.SetActive(false);
            return;
        }

        // Real-time updates for bars if needed (though events should handle it)
        // We keep these here just in case events are missed or for shared SP pool updates
        SetupHealthBar();
        SetupSkillPointBar();
    }

    public void SetEntity(Entity entity)
    {
        UnsubscribeEvents();
        Entity = entity;
        
        if (gameObject.activeInHierarchy)
        {
            SubscribeEvents();
            RefreshAll();
        }
    }

    private void SubscribeEvents()
    {
        if (Entity == null) return;
        Entity.OnHealthChanged += HandleHealthChanged;
        Entity.OnSPChanged += HandleSPChanged;
        if (Entity.buffController != null)
            Entity.buffController.OnBuffsChanged += HandleBuffsChanged;
    }

    private void UnsubscribeEvents()
    {
        if (Entity == null) return;
        Entity.OnHealthChanged -= HandleHealthChanged;
        Entity.OnSPChanged -= HandleSPChanged;
        if (Entity.buffController != null)
            Entity.buffController.OnBuffsChanged -= HandleBuffsChanged;
    }

    private void HandleHealthChanged(float current, float max) => SetupHealthBar();
    private void HandleSPChanged(int current, int max) => SetupSkillPointBar();
    private void HandleBuffsChanged() { SetupBuffPanel(); SetStatusBuff(); }

    private void RefreshAll()
    {
        SetupHealthBar();
        SetupSkillPointBar();
        SetupBuffPanel();
        SetStatusBuff();
    }

    private void CaptureInitialDimensions()
    {
        // Capture dimensions if not already captured. 
        // We use the current sizeDelta.x if it's positive, assuming it represents the full width.
        if (maxWitdthHealthbar <= 0 && HealthbarFG != null && HealthbarFG.rectTransform.rect.width > 0)
        {
            maxWitdthHealthbar = HealthbarFG.rectTransform.rect.width;
        }
        
        if (maxWitdthSkillPointBar <= 0 && SkillPointBarFG != null && SkillPointBarFG.rectTransform.rect.width > 0)
        {
            maxWitdthSkillPointBar = SkillPointBarFG.rectTransform.rect.width;
        }
    }

    private void SetupBuffPanel()
    {
        if (BuffCardPrefab == null || BuffTransform == null || DebuffTransform == null || Entity == null) return;
        
        var buffs = Entity.buffController.GetBuffs();
        var debuffs = new List<ActiveBuff>();
        debuffs.AddRange(Entity.buffController.GetDebuffs());
        debuffs.AddRange(Entity.buffController.GetBuffsByType(BuffType.CrowdControl));

        if (BuffHeaderText != null) BuffHeaderText.text = $"Buffs: {buffs.Count}";
        if (DebuffHeaderText != null) DebuffHeaderText.text = $"Debuffs: {debuffs.Count}";

        ClearTransform(BuffTransform);
        ClearTransform(DebuffTransform);

        foreach (var buff in buffs)
        {
            InstantiateBuffUI(buff, BuffTransform);
        }

        foreach (var debuff in debuffs)
        {
            InstantiateBuffUI(debuff, DebuffTransform);
        }
        
        // Force layout rebuild to avoid "wrong size" issues with layout groups
        LayoutRebuilder.ForceRebuildLayoutImmediate(BuffTransform as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(DebuffTransform as RectTransform);
    }

    private void InstantiateBuffUI(ActiveBuff buff, Transform parent)
    {
        GameObject buffGO = Instantiate(BuffCardPrefab, parent);
        BuffItemUI ui = buffGO.GetComponent<BuffItemUI>();
        if (ui != null)
        {
            ui.Setup(buff.Data.BuffName, buff.Data.Description, buff.Data.Icon, buff.CurrentStack, buff.CurrentDuration, buff.Data.isPermanent);
        }
    }

    private void ClearTransform(Transform t)
    {
        foreach (Transform child in t)
        {
            // Set inactive immediately to remove from layout calculations before Destroy kicks in
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }

    private void OnExitClicked()
    {
        gameObject.SetActive(false);
    }

    private void SetupHealthBar()
    {
        if (HealthbarFG == null || Entity == null) return;
        
        CaptureInitialDimensions();
        if (maxWitdthHealthbar <= 0) return;

        float maxHealth = Entity.GetStat(StatType.MaxHealth);
        float healthRatio = maxHealth > 0 ? (float)Entity.CurrentHealth / maxHealth : 0;
        
        HealthbarFG.rectTransform.sizeDelta = new Vector2(maxWitdthHealthbar * healthRatio, HealthbarFG.rectTransform.sizeDelta.y);
        if (HealthText != null) HealthText.text = $"{(int)Entity.CurrentHealth} / {(int)maxHealth}";
    }

    private void SetupSkillPointBar()
    {
        if (SkillPointBarFG == null || Entity == null) return;
        
        CaptureInitialDimensions();

        // Hide SP bar for entities that aren't players (like enemies)
        if (Entity is not PlayerEntity)
        {
            if (SkillPointBarFG.transform.parent != null)
                SkillPointBarFG.transform.parent.gameObject.SetActive(false);
            return;
        }

        if (SkillPointBarFG.transform.parent != null)
            SkillPointBarFG.transform.parent.gameObject.SetActive(true);

        if (maxWitdthSkillPointBar <= 0) return;

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

        if (SkillPointText != null) SkillPointText.text = $"{currentSP} / {maxSP}";
    }

    private void SetStatusBuff()
    {
        if (StatusBuffParent == null || Entity == null) return;
        
        ClearTransform(StatusBuffParent);

        List<ActiveBuff> statusBuffs = new List<ActiveBuff>();
        statusBuffs.AddRange(Entity.buffController.GetBuffsByType(BuffType.CrowdControl));
        statusBuffs.AddRange(Entity.buffController.GetBuffsByType(BuffType.Debuff));

        StatusBuffParent.gameObject.SetActive(statusBuffs.Count > 0);
        if (statusBuffs.Count == 0) return;

        foreach (var buff in statusBuffs)
        {
            GameObject buffObj = Instantiate(StatusBuffPrefab, StatusBuffParent);
            if (buffObj.GetComponent<Image>() != null)
                buffObj.GetComponent<Image>().sprite = buff.Data.Icon;
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(StatusBuffParent as RectTransform);
    }
}
