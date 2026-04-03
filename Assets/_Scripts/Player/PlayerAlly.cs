using UnityEngine;

public class PlayerAlly : Entity
{
    private PlayerActionState playerState;
    private EnemyCombat enemyTarget;

    protected override void Start()
    {
        playerState = PlayerActionState.Idle;
        // Sync stats here or leave to TeamManager
        SyncStatsWithMainPlayer();
        base.Start(); // Call base.Start() AFTER we set up the stats!
    }

    public void SyncStatsWithMainPlayer()
    {
        if (PlayerCombat.instance != null && PlayerTeamManager.Instance != null)
        {
            // Fully clone the main player's stats object so we have our own copy
            if (stats == null) 
            {
                stats = Instantiate(PlayerCombat.instance.Stats);
            }

            // Distribute ActionSpeed based on the slider ratio from TeamManager
            float totalSpeed = PlayerTeamManager.Instance.GetOriginalTotalSpeed();
            
            // Player gets Ratio (e.g. 70%)
            PlayerCombat.instance.Stats.ActionSpeed = totalSpeed * PlayerTeamManager.Instance.mainPlayerSpeedRatio;
            
            // Ally gets remainder (e.g. 30%)
            Stats.ActionSpeed = totalSpeed * (1f - PlayerTeamManager.Instance.mainPlayerSpeedRatio);

            // Set HP to Full using base stats + modifier formula.
            currentHealth = GetStat(StatType.MaxHealth);

            // Inherit skills 
            skillManager.SetSkills(PlayerCombat.instance.skillManager.GetSkills());
            Stats.EntityName = PlayerCombat.instance.Stats.EntityName + " (Ally)";
        }
    }

    // Shared SP Logic Redirect - We use the main Player's SP
    public override int CurrentSP => PlayerCombat.instance.CurrentSP;
    
    public override void SetSP(int amount)
    {
        PlayerCombat.instance.SetSP(amount);
    }

    public override void TakeDamage(Damage damage)
    {
        base.TakeDamage(damage);
        // Add Ally Healthbar UI update here if needed later
    }

    protected override void Die()
    {
        gameObject.SetActive(false);
        TurnManager.Instance.RemoveEntityFromActionQueue(this);
        Destroy(gameObject);
    }

    public void Highlight(Color color)
    {
        Transform visual = transform.Find("PlayerVisual");
        if (visual != null)
        {
            visual.GetComponent<SpriteRenderer>().color = color;
        }
        else
        {
            GetComponentInChildren<SpriteRenderer>().color = color;
        }
    }

    private void OnMouseEnter()
    {
        if (PlayerCombat.instance.GetPlayerState != PlayerActionState.Targeting || PlayerCombat.instance.GetSelectedSkill == null) return;
        if (PlayerCombat.instance.GetSelectedSkill.TargetType != TargetType.Self) return;
        
        if (TurnManager.Instance.CurrentActivePlayer == this)
            Highlight(Color.green);
    }

    private void OnMouseExit()
    {
        if (PlayerCombat.instance.GetPlayerState != PlayerActionState.Targeting || PlayerCombat.instance.GetSelectedSkill == null) return;
        if (PlayerCombat.instance.GetSelectedSkill.TargetType != TargetType.Self) return;
        
        if (TurnManager.Instance.CurrentActivePlayer == this)
            Highlight(Color.yellow);
    }

    private void OnMouseDown()
    {
        if (PlayerCombat.instance.GetPlayerState != PlayerActionState.Targeting || PlayerCombat.instance.GetSelectedSkill == null) return;
        if (PlayerCombat.instance.GetSelectedSkill.TargetType != TargetType.Self) return;
        
        Entity currentActive = TurnManager.Instance.CurrentActivePlayer;
        if (currentActive == this)
        {
            TurnManager.Instance.SubmitPlayerAction(this, this, PlayerCombat.instance.GetSelectedSkill);
        }
    }
}
