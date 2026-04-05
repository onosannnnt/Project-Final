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

    public virtual void Highlight(Color color)
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
                        enemy.Highlight(Color.yellow);
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
