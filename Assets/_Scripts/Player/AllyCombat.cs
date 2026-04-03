using UnityEngine;

public class AllyCombat : Entity
{
    private PlayerActionState playerState;
    private EnemyCombat enemyTarget;

    protected override void Start()
    {
        base.Start();
        playerState = PlayerActionState.Idle;
    }

    public void SyncStatsWithPlayer()
    {
        // Inherit stats and loadout from the main player
        if (PlayerCombat.instance != null)
        {
            // Same stats (but own separate Health due to base.Start initializing currentHealth)
            currentHealth = PlayerCombat.instance.GetStat(StatType.MaxHealth);
            
            // Skill loadout is copied
            skillManager.SetSkills(PlayerCombat.instance.skillManager.GetSkills());
            
            Stats.EntityName = PlayerCombat.instance.Stats.EntityName + " (Ally)";
        }
    }

    // Shared SP logic - redirects to main PlayerCombat
    public override int CurrentSP => PlayerCombat.instance.CurrentSP;
    public override void SetSP(int amount)
    {
        PlayerCombat.instance.SetSP(amount);
    }

    protected override void Die()
    {
        Debug.Log("Ally Died");
        gameObject.SetActive(false);
        TurnManager.Instance.RemoveEntityFromActionQueue(this);
        Destroy(gameObject);
    }
}
