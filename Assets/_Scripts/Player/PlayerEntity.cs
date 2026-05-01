using UnityEngine;

public abstract class PlayerEntity : Entity
{
    protected PlayerActionState playerState;
    protected EnemyCombat enemyTarget;

    protected override void Start()
    {
        base.Start();
        playerState = PlayerActionState.Idle;
        Highlight(Color.white); // Ensure indicator is hidden at start
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

    public virtual void Highlight(Color color)
    {
        // Stop tinting the character sprite
        Transform visual = transform.Find("PlayerVisual");
        SpriteRenderer sr = null;
        if (visual != null) sr = visual.GetComponent<SpriteRenderer>();
        else sr = GetComponentInChildren<SpriteRenderer>();
        
        if (sr != null) sr.color = Color.white;

        if (targetIndicator != null)
        {
            bool isHighlighted = color != Color.white;
            targetIndicator.SetActive(isHighlighted);
        }
    }

    public virtual void HandleSelectSkill()
    {
        if (selectedSkill == null) return;
        
        if (selectedSkill.TargetType == TargetType.Self)
        {
            Highlight(Color.yellow);
            
            // Clear other player highlights
            if (PlayerTeamManager.Instance != null)
            {
                foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
                {
                    if (member != this) member.Highlight(Color.white);
                }
            }

            foreach (var enemy in FindObjectsOfType<EnemyCombat>())
            {
                enemy.Highlight(Color.white);
            }
            SetPlayerState(PlayerActionState.Targeting);
        }
        else if (selectedSkill.TargetType == TargetType.Ally)
        {
            // For Ally skills, only show indicators if TargetCount is All (as per "all skill" request)
            // If it's Single, we might want to hide them until hovered, but for now let's follow the "Self or All" rule.
            
            if (selectedSkill.TargetCount == TargetCount.All)
            {
                if (PlayerTeamManager.Instance != null)
                {
                    foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
                    {
                        member.Highlight(Color.yellow);
                    }
                }
            }
            else
            {
                // Single target Ally: keep indicators hidden initially
                if (PlayerTeamManager.Instance != null)
                {
                    foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
                    {
                        member.Highlight(Color.white);
                    }
                }
            }

            foreach (var enemy in FindObjectsOfType<EnemyCombat>())
            {
                enemy.Highlight(Color.white);
            }
            SetPlayerState(PlayerActionState.Targeting);
        }
        else if (selectedSkill.TargetType == TargetType.Enemy)
        {
            Highlight(Color.white);

            if (selectedSkill.TargetCount == TargetCount.Single)
            {
                foreach (var enemy in FindObjectsOfType<EnemyCombat>())
                {
                    if (enemy == enemyTarget)
                        enemy.Highlight(Color.red);
                    else
                        enemy.Highlight(Color.white); // Don't show indicators for non-targets
                }
            }
            else if (selectedSkill.TargetCount == TargetCount.All)
            {
                foreach (var enemy in FindObjectsOfType<EnemyCombat>())
                {
                    enemy.Highlight(Color.red);
                }
            }
            SetPlayerState(PlayerActionState.Targeting);
        }
    }

    protected virtual void OnMouseEnter()
    {
        if (playerState != PlayerActionState.Targeting || selectedSkill == null) return;
        
        if (selectedSkill.TargetType == TargetType.Self)
        {
            if (TurnManager.Instance.CurrentActivePlayer == this) Highlight(Color.green);
        }
        else if (selectedSkill.TargetType == TargetType.Ally)
        {
            if (selectedSkill.TargetCount == TargetCount.Single)
            {
                Highlight(Color.green);
            }
            else if (selectedSkill.TargetCount == TargetCount.All)
            {
                if (PlayerTeamManager.Instance != null)
                {
                    foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
                    {
                        member.Highlight(Color.green);
                    }
                }
            }
        }
    }

    protected virtual void OnMouseExit()
    {
        if (playerState != PlayerActionState.Targeting || selectedSkill == null) return;

        if (selectedSkill.TargetType == TargetType.Self)
        {
            if (TurnManager.Instance.CurrentActivePlayer == this) Highlight(Color.yellow);
        }
        else if (selectedSkill.TargetType == TargetType.Ally)
        {
            if (selectedSkill.TargetCount == TargetCount.Single)
            {
                Highlight(Color.yellow);
            }
            else if (selectedSkill.TargetCount == TargetCount.All)
            {
                if (PlayerTeamManager.Instance != null)
                {
                    foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
                    {
                        member.Highlight(Color.yellow);
                    }
                }
            }
        }
    }

    protected virtual void OnMouseDown()
    {
        if (playerState != PlayerActionState.Targeting || selectedSkill == null) return;

        if (selectedSkill.TargetType == TargetType.Self)
        {
            Entity currentActive = TurnManager.Instance.CurrentActivePlayer ?? this;
            if (currentActive == this)
            {
                Highlight(Color.white);
                TurnManager.Instance.SubmitPlayerAction(this, this, selectedSkill);
            }
        }
        else if (selectedSkill.TargetType == TargetType.Ally)
        {
            PlayerEntity activePlayer = TurnManager.Instance.CurrentActivePlayer;
            if (activePlayer == null) return;

            if (selectedSkill.TargetCount == TargetCount.Single)
            {
                // Clear all player highlights
                if (PlayerTeamManager.Instance != null)
                {
                    foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
                    {
                        member.Highlight(Color.white);
                    }
                }
                TurnManager.Instance.SubmitPlayerAction(activePlayer, this, selectedSkill);
            }
            else if (selectedSkill.TargetCount == TargetCount.All)
            {
                // For 'All' skills, clicking any valid target triggers it for the whole group
                if (PlayerTeamManager.Instance != null)
                {
                    foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
                    {
                        member.Highlight(Color.white);
                    }
                }
                TurnManager.Instance.SubmitPlayerAction(activePlayer, this, selectedSkill);
            }
        }
    }
}
