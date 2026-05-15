using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetingPanel : MonoBehaviour
{
    [SerializeField] private StatInfoUI statInfoUI;
    [SerializeField] private Button DetailPanel;
    [SerializeField] private TextMeshProUGUI TargetingNameText;
    [SerializeField] private Image Icon;
    [SerializeField] private Transform BuffParent;
    [SerializeField] private GameObject BuffPrefab;
    [SerializeField] private GameObject DebuffPrefab;
    private Entity currentTarget;
    public static TargetingPanel instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        gameObject.SetActive(false);
        DetailPanel.onClick.AddListener(OnPanelClicked);
    }
    private void Update()
    {
        // เช็คเงื่อนไขว่า "ควรจะแสดงแผง UI หรือไม่"
        bool shouldBeActive = (TurnManager.Instance.CurrentActivePlayer as PlayerEntity ?? PlayerCombat.instance).GetPlayerState == PlayerActionState.Targeting
                              && currentTarget != null
                              && TurnManager.Instance.GetTurnState() == TurnState.PlayerTurnState
                              && (TurnManager.Instance.CurrentActivePlayer as PlayerEntity ?? PlayerCombat.instance).GetSelectedSkill != null
                              && (TurnManager.Instance.CurrentActivePlayer as PlayerEntity ?? PlayerCombat.instance).GetSelectedSkill.TargetType != TargetType.Self;

        // ถ้ายอมให้แสดงผล แต่ตัว GameObject ยังปิดอยู่ ค่อยสั่งเปิด (จะทำแค่ครั้งเดียว ไม่รัว)
        if (shouldBeActive && !gameObject.activeSelf)
        {
            SetActivePanel(true);
        }
        // ถ้าไม่ให้แสดงผล แต่ GameObject ยังเปิดอยู่ ค่อยสั่งปิด
        else if (!shouldBeActive && gameObject.activeSelf)
        {
            SetActivePanel(false);
        }
    }
    public void SetActivePanel(bool active)
    {
        gameObject.SetActive(active);
        if (currentTarget == null) return;
        SetBuffs();
        TargetingNameText.text = currentTarget.Stats.GetName();
        Icon.sprite = currentTarget.Stats.GetIcon();
    }
    public void SetEnemyTargetPanel(Entity enemy)
    {
        currentTarget = enemy;
        if ((TurnManager.Instance.CurrentActivePlayer as PlayerEntity ?? PlayerCombat.instance).GetPlayerState == PlayerActionState.Targeting)
        {
            SetActivePanel(true);
        }
    }
    public void SetBuffs()
    {
        foreach (Transform child in BuffParent.transform)
        {
            Destroy(child.gameObject);
        }
        if (currentTarget == null) return;

        List<ActiveBuff> buffs = currentTarget.buffController.GetBuffsByType(BuffType.Buff);
        List<ActiveBuff> debuffs = new List<ActiveBuff>();
        debuffs.AddRange(currentTarget.buffController.GetBuffsByType(BuffType.CrowdControl));
        debuffs.AddRange(currentTarget.buffController.GetBuffsByType(BuffType.Debuff));

        int total = buffs.Count + debuffs.Count;
        BuffParent.gameObject.SetActive(total > 0);

        foreach (var buff in buffs)
        {
            GameObject buffObj = Instantiate(BuffPrefab, BuffParent.transform);
            buffObj.GetComponent<Image>().sprite = buff.Data.Icon;
            buffObj.transform.Find("Duration").GetComponentInChildren<TextMeshProUGUI>().text = BuffStackColor(buff.CurrentDuration) + $"{buff.CurrentDuration}</color>";
            buffObj.transform.Find("Stack").GetComponentInChildren<TextMeshProUGUI>().text = BuffStackColor(buff.CurrentStack) + $"{buff.CurrentStack}</color>";
        }

        if (debuffs.Count > 0 && DebuffPrefab != null)
        {
            foreach (var db in debuffs)
            {
                GameObject buffObj = Instantiate(DebuffPrefab, BuffParent.transform);
                buffObj.GetComponent<Image>().sprite = db.Data.Icon;
            }
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
    private void OnPanelClicked()
    {
        statInfoUI.SetEntity(currentTarget);
        statInfoUI.gameObject.SetActive(true);
    }
}
