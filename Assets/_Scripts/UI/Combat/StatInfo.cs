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
    [SerializeField] private Transform BuffParent;
    [SerializeField] private GameObject BuffIconPrefab;
    [SerializeField] private Transform StatusBuffParent;
    [SerializeField] private GameObject StatusBuffPrefab;

    private Entity Entity;
    private List<ActiveBuff> Buffs;
    private List<ActiveBuff> Debuffs;
    private float maxWitdthHealthbar;
    private void Start()
    {
        gameObject.SetActive(false);
        ExitButton.onClick.AddListener(OnExitClicked);
        if (HealthbarFG != null) maxWitdthHealthbar = HealthbarFG.rectTransform.sizeDelta.x;
    }
    private void Update()
    {
        if (Entity == null) return;
        if (TurnManager.Instance.GetTurnState() != TurnState.PlayerTurnState) gameObject.SetActive(false);
        SetupBuffPanel();
        SetupHealthBar();
        SetBuffs();
        SetStatusBuff();
    }
    private void OnEnable()
    {
        if (TargetingPanel.instance == null) return;
        TargetingPanel.instance.gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        if (TargetingPanel.instance == null) return;
        TargetingPanel.instance.gameObject.SetActive(true);
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
                ui.Setup($"{buff.Data.BuffName}({buff.CurrentDuration})", buff.Data.Description, buff.Data.Icon, buff.CurrentStack);
            }
        }

        if (Debuffs.Count > 0)
        {
            foreach (var debuff in Debuffs)
            {
                GameObject debuffGO = Instantiate(BuffCardPrefab, DebuffTransform);
                BuffItemUI ui = debuffGO.GetComponent<BuffItemUI>();
                ui.Setup($"{debuff.Data.BuffName}({debuff.CurrentDuration})", debuff.Data.Description, debuff.Data.Icon, debuff.CurrentStack);
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

        HealthText.text = $"{Entity.CurrentHealth} / {Entity.GetStat(StatType.MaxHealth)}";
    }
    private void SetBuffs()
    {
        if (BuffParent == null) return;
        foreach (Transform child in BuffParent)
        {
            Destroy(child.gameObject);
        }
        if (Entity == null) return;
        List<ActiveBuff> Buffs = Entity.buffController.GetBuffsByType(BuffType.Buff);
        if (Buffs.Count == 0) BuffParent.gameObject.SetActive(false);
        else BuffParent.gameObject.SetActive(true);
        foreach (var buff in Buffs)
        {
            GameObject buffObj = Instantiate(BuffIconPrefab, BuffParent.transform);
            buffObj.GetComponent<Image>().sprite = buff.Data.Icon;
            buffObj.transform.Find("Duration").GetComponentInChildren<TextMeshProUGUI>().text = BuffStackColor(buff.CurrentDuration) + $"{buff.CurrentDuration}</color>";
            buffObj.transform.Find("Stack").GetComponentInChildren<TextMeshProUGUI>().text = BuffStackColor(buff.CurrentStack) + $"{buff.CurrentStack}</color>";
        }
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
