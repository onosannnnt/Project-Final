using UnityEngine;

public abstract class PlayerEntity : Entity
{
    protected PlayerActionState playerState;
    protected EnemyCombat enemyTarget;

    protected override void Start()
    {
        base.Start();
        playerState = PlayerActionState.Idle;
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
            TurnManager.Instance.RestoreSharedPlayerSkillPoints(amount);
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
            
            if (isHighlighted)
            {
                // Apply the highlight color to the indicator
                SpriteRenderer indicatorSR = targetIndicator.GetComponent<SpriteRenderer>();
                if (indicatorSR != null) indicatorSR.color = color;
            }
        }
    }

    public virtual void HandleSelectSkill()
    {
        if (selectedSkill == null) return;
        
        if (selectedSkill.TargetType == TargetType.Self)
        {
            Highlight(Color.yellow);
            
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
        if (selectedSkill.TargetType != TargetType.Self) return;
        if (TurnManager.Instance.CurrentActivePlayer == this || TurnManager.Instance.CurrentActivePlayer == null) Highlight(Color.green);
    }

    protected virtual void OnMouseExit()
    {
        if (playerState != PlayerActionState.Targeting || selectedSkill == null) return;
        if (selectedSkill.TargetType != TargetType.Self) return;
        if (TurnManager.Instance.CurrentActivePlayer == this || TurnManager.Instance.CurrentActivePlayer == null) Highlight(Color.yellow);
    }

    protected virtual void OnMouseDown()
    {
        if (playerState != PlayerActionState.Targeting || selectedSkill == null) return;
        if (selectedSkill.TargetType != TargetType.Self) return;
        
        Entity currentActive = TurnManager.Instance.CurrentActivePlayer ?? this;
        if (currentActive == this)
        {
            Highlight(Color.white);
            TurnManager.Instance.SubmitPlayerAction(this, this, selectedSkill);
        }
    }
}
