using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetingPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI targetingNameText;
    [SerializeField] private GameObject healthBarForeground;
    [SerializeField] private TextMeshProUGUI TextHealthBar;
    [SerializeField] private GameObject BuffParent;
    [SerializeField] private GameObject BuffPrefab;
    private Entity currentTarget;
    private float maxhealthBarForegroundWidth;
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
        maxhealthBarForegroundWidth = healthBarForeground.GetComponent<RectTransform>().sizeDelta.x;
        gameObject.SetActive(false);
    }
    private void Update()
    {
        if (PlayerCombat.instance.GetPlayerState == PlayerActionState.Targeting && currentTarget != null && TurnManager.Instance.GetTurnState() == TurnState.PlayerTurnState && PlayerCombat.instance.GetSelectedSkill.TargetType != TargetType.Self)
        {
            SetActivePanel(true);
        }
        else
        {
            SetActivePanel(false);
        }
    }
    private void UpdateHealthBar()
    {
        if (currentTarget == null) return;
        targetingNameText.text = currentTarget.gameObject.name;
        float healthPercent = currentTarget.CurrentHealth / currentTarget.GetStat(StatType.MaxHealth);
        Debug.Log($"Updating health bar for {currentTarget.gameObject.name}: {healthPercent * 100}%");
        healthBarForeground.GetComponent<RectTransform>().sizeDelta = new Vector2(maxhealthBarForegroundWidth * healthPercent, healthBarForeground.GetComponent<RectTransform>().sizeDelta.y);
        TextHealthBar.text = HealthColor(healthPercent) + $"{currentTarget.CurrentHealth} / {currentTarget.GetStat(StatType.MaxHealth)} </color>";
    }
    public void SetActivePanel(bool active)
    {
        gameObject.SetActive(active);
        UpdateHealthBar();
        SetBuffs();
    }
    public void SetEnemyTargetPanel(Entity enemy)
    {
        currentTarget = enemy;
        if (PlayerCombat.instance.GetPlayerState == PlayerActionState.Targeting) SetActivePanel(true);
    }
    public void SetBuffs()
    {
        float startX = -165f;
        float spacing = 40f;
        foreach (Transform child in BuffParent.transform)
        {
            Destroy(child.gameObject);
        }
        if (currentTarget == null) return;
        foreach (var buff in currentTarget.buffController.GetBuff())
        {
            GameObject buffObj = Instantiate(BuffPrefab, BuffParent.transform);
            buffObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX, 0);
            buffObj.GetComponent<Image>().sprite = buff.Icon;
            buffObj.transform.Find("Duration").GetComponentInChildren<TextMeshProUGUI>().text = BuffStackColor(buff.Duration) + $"{buff.Duration}</color>";
            buffObj.transform.Find("Stack").GetComponentInChildren<TextMeshProUGUI>().text = BuffStackColor(buff.Stack) + $"{buff.Stack}</color>";
            startX += spacing;
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
    private string HealthColor(float healthPercent)
    {
        if (healthPercent >= 0.5f)
            return "<color=white>";
        else
            return "<color=black>";
    }
}