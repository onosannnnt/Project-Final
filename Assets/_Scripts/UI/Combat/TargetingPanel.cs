using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetingPanel : MonoBehaviour
{
    [SerializeField] private StatInfoUI statInfoUI;
    [SerializeField] private Button DetailPanel;
    [SerializeField] private TextMeshProUGUI TargetingNameText;
    [SerializeField] private TextMeshProUGUI LevelText;
    [SerializeField] private Image Icon;
    [SerializeField] private Transform BuffParent;
    [SerializeField] private GameObject BuffPrefab;
    [SerializeField] private Transform StatusBuffParent;
    [SerializeField] private GameObject StatusBuffPrefab;
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
        bool shouldBeActive = PlayerCombat.instance.GetPlayerState == PlayerActionState.Targeting 
                              && currentTarget != null 
                              && TurnManager.Instance.GetTurnState() == TurnState.PlayerTurnState 
                              && PlayerCombat.instance.GetSelectedSkill.TargetType != TargetType.Self;

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
        SetStatusBuff();
        TargetingNameText.text = currentTarget.Stats.GetName();
        Icon.sprite = currentTarget.Stats.GetIcon();
        LevelText.text = $"Lv.{currentTarget.Stats.Level}";
    }
    public void SetEnemyTargetPanel(Entity enemy)
    {
        currentTarget = enemy;
        if (PlayerCombat.instance.GetPlayerState == PlayerActionState.Targeting)
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
        List<Buff> Buffs = currentTarget.buffController.GetBuffsByType(BuffType.Buff);
        if (Buffs.Count == 0) BuffParent.gameObject.SetActive(false);
        else BuffParent.gameObject.SetActive(true);
        foreach (var buff in Buffs)
        {
            GameObject buffObj = Instantiate(BuffPrefab, BuffParent.transform);
            buffObj.GetComponent<Image>().sprite = buff.Icon;
            buffObj.transform.Find("Duration").GetComponentInChildren<TextMeshProUGUI>().text = BuffStackColor(buff.Duration) + $"{buff.Duration}</color>";
            buffObj.transform.Find("Stack").GetComponentInChildren<TextMeshProUGUI>().text = BuffStackColor(buff.Stack) + $"{buff.Stack}</color>";
        }
    }
    public void SetStatusBuff()
    {
        foreach (Transform child in StatusBuffParent.transform)
        {
            Destroy(child.gameObject);
        }
        if (currentTarget == null) return;
        List<Buff> statusBuffs = currentTarget.buffController.GetBuffsByType(BuffType.CrowdControl);
        if (statusBuffs.Count == 0) StatusBuffParent.gameObject.SetActive(false);
        else StatusBuffParent.gameObject.SetActive(true);
        if (statusBuffs.Count == 0) return;
        foreach (var buff in statusBuffs)
        {
            GameObject buffObj = Instantiate(StatusBuffPrefab, StatusBuffParent.transform);
            buffObj.GetComponent<Image>().sprite = buff.Icon;
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