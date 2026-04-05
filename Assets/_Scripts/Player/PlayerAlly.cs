using UnityEngine;

public class PlayerAlly : PlayerEntity
{
    protected override void Start()
    {
        base.Start(); // Let Entity do its base init first.
        // Sync stats here or leave to TeamManager
        SyncStatsWithMainPlayer();
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
            PlayerCombat.instance.Stats.SetBase(StatType.ActionSpeed, totalSpeed * PlayerTeamManager.Instance.mainPlayerSpeedRatio);

            // Ally gets remainder (e.g. 30%)
            Stats.SetBase(StatType.ActionSpeed, totalSpeed * (1f - PlayerTeamManager.Instance.mainPlayerSpeedRatio));

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
}
