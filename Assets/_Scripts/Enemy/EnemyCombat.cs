using UnityEngine;

public class EnemyCombat : Entity
{

    [SerializeField] private GameObject HealthBar;

    protected override void Die()
    {
        gameObject.SetActive(false);
    }
    public override void TakeDamage(Damage damage)
    {
        base.TakeDamage(damage);
        var healthBar = HealthBar.GetComponent<UnityEngine.UI.Image>();
        Debug.Log($"{gameObject.name} took {damage.Amount} damage, current health: {CurrentHealth / GetStat(StatType.MaxHealth)}");
        if (healthBar != null)
        {
            healthBar.fillAmount = CurrentHealth / GetStat(StatType.MaxHealth);
        }
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }
    public void Highlight(Color color)
    {
        GameObject EnemyVisual = transform.Find("EnemyVisual").gameObject;
        EnemyVisual.GetComponent<SpriteRenderer>().color = color;
    }
    public void OnMouseOver()
    {
        TargetingPanel.instance.SetEnemyTargetPanel(this);
        TargetingPanel.instance.SetActivePanel(true);
        if (PlayerCombat.instance.GetSelectedSkill == null) return;
        if (PlayerCombat.instance.GetPlayerState != PlayerActionState.Targeting) return;
        if (PlayerCombat.instance.GetSelectedSkill.TargetType == TargetType.Self) return;
        Highlight(Color.red);
    }
    public void OnMouseExit()
    {
        TargetingPanel.instance.gameObject.SetActive(false);
        if (PlayerCombat.instance.GetPlayerState != PlayerActionState.Targeting) return;
        if (PlayerCombat.instance.GetSelectedSkill == null) return;
        if (PlayerCombat.instance.GetSelectedSkill.TargetType == TargetType.Self) return;
        Highlight(Color.yellow);
    }
    public void OnMouseDown()
    {
        if (PlayerCombat.instance.GetPlayerState != PlayerActionState.Targeting) return;
        if (PlayerCombat.instance.GetSelectedSkill == null) return;
        if (PlayerCombat.instance.GetSelectedSkill.TargetType == TargetType.Self) return;
        PlayerCombat.instance.SetEnemyTarget(this);
        TurnManager.Instance.SetState(TurnState.SpeedCompareState);
    }
}