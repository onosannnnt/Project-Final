using UnityEngine;

public abstract class PlayerEntity : Entity
{
    protected PlayerActionState playerState;
    protected EnemyCombat enemyTarget;
    protected PlayerEntity allyTarget;

    protected override void Start()
    {
        base.Start();
        playerState = PlayerActionState.Idle;
        SetTargetIndicator(false); // Ensure indicator is hidden at start
    }

    public PlayerActionState GetPlayerState => playerState;

    public virtual void SetPlayerState(PlayerActionState state)
    {
        playerState = state;
    }

    public virtual void SetEnemyTarget(EnemyCombat enemy)
    {
        enemyTarget = enemy;
        if(enemy != null && TargetingPanel.instance != null)
        {
            TargetingPanel.instance.SetEnemyTargetPanel(enemy);
        }
    }

    public Entity GetEnemyTarget()
    {
        return enemyTarget;
    }

    public virtual void SetAllyTarget(PlayerEntity ally)
    {
        allyTarget = ally;
    }

    public PlayerEntity GetAllyTarget()
    {
        return allyTarget;
    }

    public override void SetSP(int amount)
    {
        if (TurnManager.Instance != null && TurnManager.Instance.UseSharedPlayerSkillPointPool)
        {
            TurnManager.Instance.ResourceManager.RestoreSharedPlayerSkillPoints(amount);
        }
        else
        {
            base.SetSP(amount);
        }
    }

    public virtual void SetTargetIndicator(bool active)
    {
        if (targetIndicator != null)
        {
            targetIndicator.SetActive(active);
        }
    }

    protected virtual void Update()
    {
        PlayerEntity activePlayer = TurnManager.Instance.CurrentActivePlayer;
        if (activePlayer == null || activePlayer.GetPlayerState != PlayerActionState.Targeting || activePlayer.GetSelectedSkill == null)
        {
            SetTargetIndicator(false);
            return;
        }

        Skill activeSkill = activePlayer.GetSelectedSkill;

        // Visual logic for the player indicator
        if (activeSkill.TargetType == TargetType.Self)
        {
            SetTargetIndicator(activePlayer == this);
        }
        else if (activeSkill.TargetType == TargetType.Ally)
        {
            if (activeSkill.TargetCount == TargetCount.All)
            {
                SetTargetIndicator(true);
            }
            else if (activeSkill.TargetCount == TargetCount.Single)
            {
                SetTargetIndicator(activePlayer.GetAllyTarget() == this);
            }
        }
        else
        {
            SetTargetIndicator(false);
        }
    }

    public virtual void HandleSelectSkill()
    {
        if (selectedSkill == null) return;
        
        if (selectedSkill.TargetType == TargetType.Self)
        {
            SetTargetIndicator(true);
            
            // Clear other player indicators
            if (PlayerTeamManager.Instance != null)
            {
                foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
                {
                    if (member != this) member.SetTargetIndicator(false);
                }
            }

            foreach (var enemy in FindObjectsOfType<EnemyCombat>())
            {
                enemy.SetTargetIndicator(false);
            }
            SetPlayerState(PlayerActionState.Targeting);
        }
        else if (selectedSkill.TargetType == TargetType.Ally)
        {
            if (allyTarget == null) allyTarget = this;

            if (selectedSkill.TargetCount == TargetCount.All)
            {
                if (PlayerTeamManager.Instance != null)
                {
                    foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
                    {
                        member.SetTargetIndicator(true);
                    }
                }
            }
            else
            {
                // Single target Ally: show indicator for the currently selected ally target
                if (PlayerTeamManager.Instance != null)
                {
                    foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
                    {
                        member.SetTargetIndicator(member == allyTarget);
                    }
                }
            }

            foreach (var enemy in FindObjectsOfType<EnemyCombat>())
            {
                enemy.SetTargetIndicator(false);
            }
            SetPlayerState(PlayerActionState.Targeting);
        }
        else if (selectedSkill.TargetType == TargetType.Enemy)
        {
            SetTargetIndicator(false);

            if (selectedSkill.TargetCount == TargetCount.Single)
            {
                foreach (var enemy in FindObjectsOfType<EnemyCombat>())
                {
                    enemy.SetTargetIndicator(enemy == enemyTarget);
                }
            }
            else if (selectedSkill.TargetCount == TargetCount.All)
            {
                foreach (var enemy in FindObjectsOfType<EnemyCombat>())
                {
                    enemy.SetTargetIndicator(true);
                }
            }
            SetPlayerState(PlayerActionState.Targeting);
        }
    }

    protected virtual void OnMouseEnter()
    {
        // Hover logic removed as requested
    }

    protected virtual void OnMouseExit()
    {
        // Hover logic removed as requested
    }

    protected virtual void OnMouseDown()
    {
        PlayerEntity activePlayer = TurnManager.Instance.CurrentActivePlayer;
        if (activePlayer == null || activePlayer.GetPlayerState != PlayerActionState.Targeting || activePlayer.GetSelectedSkill == null) return;

        Skill activeSkill = activePlayer.GetSelectedSkill;

        if (activeSkill.TargetType == TargetType.Self)
        {
            if (activePlayer == this)
            {
                SetTargetIndicator(false);
                TurnManager.Instance.SubmitPlayerAction(this, this, activeSkill);
            }
        }
        else if (activeSkill.TargetType == TargetType.Ally)
        {
            if (activeSkill.TargetCount == TargetCount.Single)
            {
                if (activePlayer.GetAllyTarget() == this)
                {
                    // Second click: Submit
                    SetTargetIndicator(false);
                    TurnManager.Instance.SubmitPlayerAction(activePlayer, this, activeSkill);
                }
                else
                {
                    // First click: Select
                    activePlayer.SetAllyTarget(this);
                }
            }
            else if (activeSkill.TargetCount == TargetCount.All)
            {
                // For 'All' skills, clicking any valid target triggers it for the whole group
                SetTargetIndicator(false);
                TurnManager.Instance.SubmitPlayerAction(activePlayer, this, activeSkill);
            }
        }
    }
}
