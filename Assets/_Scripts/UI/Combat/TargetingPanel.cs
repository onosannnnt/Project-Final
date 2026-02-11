using TMPro;
using UnityEngine;

public class TargetingPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI targetingNameText;
    [SerializeField] private GameObject healthBarForeground;
    [SerializeField] private TextMeshProUGUI TextHealthBar;
    [SerializeField] private GameObject BuffParent;
    [SerializeField] private GameObject BuffPrefab;
    private EnemyCombat currentTarget;
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
    public void Start()
    {
        gameObject.SetActive(false);
        maxhealthBarForegroundWidth = healthBarForeground.GetComponent<RectTransform>().sizeDelta.x;

    }
    private void UpdateHealthBar()
    {
        if (currentTarget == null) return;
        targetingNameText.text = currentTarget.gameObject.name;
        float healthPercent = currentTarget.CurrentHealth / currentTarget.GetStat(StatType.MaxHealth);
        healthBarForeground.GetComponent<RectTransform>().sizeDelta = new Vector2(maxhealthBarForegroundWidth * healthPercent, healthBarForeground.GetComponent<RectTransform>().sizeDelta.y);
        TextHealthBar.text = HealthColor(healthPercent) + $"{currentTarget.CurrentHealth} / {currentTarget.GetStat(StatType.MaxHealth)} </color>";
    }
    public void SetActivePanel(bool active)
    {
        gameObject.SetActive(active);
        UpdateHealthBar();
        SetBuffs();
    }
    public void SetEnemyTargetPanel(EnemyCombat enemy)
    {
        currentTarget = enemy;
    }
    public void SetBuffs()
    {
        float startX = -165f;
        float spacing = 40f;
        foreach (Transform child in BuffParent.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (var buff in currentTarget.buffController.GetBuff())
        {
            GameObject buffObj = Instantiate(BuffPrefab, BuffParent.transform);
            buffObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX, 0);
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