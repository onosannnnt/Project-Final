using UnityEngine;

public class EnemyCombat : Entity
{
    [SerializeField] private GameObject healthBarForeground;
    private float maxhealthBarForegroundWidth;
    private bool isDead;

    protected override void Start()
    {
        base.Start();
        maxhealthBarForegroundWidth = healthBarForeground.GetComponent<RectTransform>().sizeDelta.x;
        if (PlayerCombat.instance.GetEnemyTarget() == this) TargetingPanel.instance.SetEnemyTargetPanel(this);
    }
    private void Update()
    {
        if (PlayerCombat.instance.GetPlayerState != PlayerActionState.Targeting || PlayerCombat.instance.GetSelectedSkill == null)
        {
            Highlight(Color.white);
            return;
        }

        if (PlayerCombat.instance.GetSelectedSkill.TargetType == TargetType.Self)
        {
            Highlight(Color.white);
            return;
        }

        // If the selected skill targets all enemies, highlight all in red
        if (PlayerCombat.instance.GetSelectedSkill.TargetType == TargetType.AllEnemies)
        {
            Highlight(Color.red);
        }
        // If the selected skill targets a single enemy
        else if (PlayerCombat.instance.GetSelectedSkill.TargetType == TargetType.SingleEnemy)
        {
            if (PlayerCombat.instance.GetEnemyTarget() == this)
                Highlight(Color.red);
            else
                Highlight(Color.yellow);
        }
    }

    protected override void Die()
    {
        gameObject.SetActive(false);
        isDead = true;
        TurnManager.Instance.RemoveActionQueue(this);
        Destroy(gameObject);
    }
    public override void TakeDamage(Damage damage)
    {
        base.TakeDamage(damage);
        var healthBar = healthBarForeground.GetComponent<UnityEngine.UI.Image>();
        Debug.Log($"{gameObject.name} took {damage.Amount} damage, current health: {CurrentHealth / GetStat(StatType.MaxHealth)}");
        if (healthBar != null)
        {
            float healthPercent = CurrentHealth / GetStat(StatType.MaxHealth);
            healthBarForeground.GetComponent<RectTransform>().sizeDelta = new Vector2(maxhealthBarForegroundWidth * healthPercent, healthBarForeground.GetComponent<RectTransform>().sizeDelta.y);
        }
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void OnMouseDown()
    {
        if (PlayerCombat.instance.GetPlayerState != PlayerActionState.Targeting) return;
        if (PlayerCombat.instance.GetSelectedSkill == null) return;
        if (PlayerCombat.instance.GetSelectedSkill.TargetType == TargetType.Self) return;
        if (PlayerCombat.instance.GetEnemyTarget() == this)
            TurnManager.Instance.SetState(TurnState.SpeedCompareState);
        PlayerCombat.instance.SetEnemyTarget(this);
        TargetingPanel.instance.SetEnemyTargetPanel(this);

    }
    public bool IsDead()
    {
        return isDead;
    }
    public void Highlight(Color color)
    {
        GameObject EnemyVisual = transform.Find("EnemyVisual").gameObject;
        EnemyVisual.GetComponent<SpriteRenderer>().color = color;
    }
}