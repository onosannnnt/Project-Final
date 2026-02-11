using Unity.Mathematics;
using UnityEngine;

public class PlayerCombat : Entity
{
    public static PlayerCombat instance;
    private PlayerActionState playerState;
    private Entity enemyTarget;
    protected override void Awake()
    {
        base.Awake();
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    protected override void Start()
    {
        base.Start();
        playerState = PlayerActionState.Idle;
    }
    protected override void Die()
    {
        Debug.Log("Player Died");
    }
    public void SelectSkill(Skill skill)
    {
        selectedSkill = skill;
    }
    public PlayerActionState GetPlayerState => playerState;
    public void SetPlayerState(PlayerActionState state)
    {
        playerState = state;
    }
    public void SetEnemyTarget(Entity enemy)
    {
        enemyTarget = enemy;
    }
    public Entity GetEnemyTarget()
    {
        return enemyTarget;
    }
    public void Highlight(Color color)
    {
        GameObject PlayerVisual = transform.Find("PlayerVisual").gameObject;
        PlayerVisual.GetComponent<SpriteRenderer>().color = color;
    }
    public void HandleSelectSkill()
    {
        if (selectedSkill == null) return;
        if (selectedSkill.TargetType != TargetType.Self)
        {
            foreach (var enemy in FindObjectsOfType<EnemyCombat>())
            {
                enemy.Highlight(Color.yellow);
            }
        }
        switch (selectedSkill.TargetType)
        {
            case TargetType.Self:
                Highlight(Color.yellow);
                foreach (var enemy in FindObjectsOfType<EnemyCombat>())
                {
                    enemy.Highlight(Color.white);
                }
                SetPlayerState(PlayerActionState.Targeting);
                break;

            case TargetType.SingleEnemy:
                Highlight(Color.white);
                foreach (var enemy in FindObjectsOfType<EnemyCombat>())
                {
                    enemy.Highlight(Color.yellow);
                }
                SetPlayerState(PlayerActionState.Targeting);
                break;

            case TargetType.AllEnemies:
                Highlight(Color.white);
                foreach (var enemy in FindObjectsOfType<EnemyCombat>())
                {
                    enemy.Highlight(Color.yellow);
                }
                SetPlayerState(PlayerActionState.Targeting);
                break;
        }
    }
    private void OnMouseEnter()
    {
        if (playerState != PlayerActionState.Targeting || selectedSkill == null) return;
        if (selectedSkill.TargetType != TargetType.Self) return;
        Highlight(Color.green);
    }
    private void OnMouseExit()
    {
        if (playerState != PlayerActionState.Targeting || selectedSkill == null) return;
        if (selectedSkill.TargetType != TargetType.Self) return;
        Highlight(Color.yellow);
    }
    private void OnMouseDown()
    {
        if (playerState != PlayerActionState.Targeting || selectedSkill == null) return;
        if (selectedSkill.TargetType != TargetType.Self) return;
        Highlight(Color.white);
        TurnManager.Instance.SetState(TurnState.SpeedCompareState);
    }
    public void Action()
    {
        if (currentSkillPoint < selectedSkill.SkillPoint)
        {
            Debug.Log("Not enough SP to use " + selectedSkill.name);
            return;
        }
        currentSkillPoint -= selectedSkill.SkillPoint;
        switch (selectedSkill.TargetType)
        {
            case TargetType.Self:
                Debug.Log("Player used " + selectedSkill.name + " on self");
                skillManager.UseSkill(selectedSkill, this);
                break;

            case TargetType.SingleEnemy:
                Debug.Log("Player used " + selectedSkill.name + " on " + enemyTarget.gameObject.name);
                skillManager.UseSkill(selectedSkill, enemyTarget);
                break;

            case TargetType.AllEnemies:
                Debug.Log("Player used " + selectedSkill.name + " on all enemies");
                foreach (var enemy in FindObjectsOfType<EnemyCombat>())
                {
                    skillManager.UseSkill(selectedSkill, enemy);
                }
                break;
        }
        currentSkillPoint += math.min(selectedSkill.SkillPointRestore, (int)GetStat(StatType.MaxSkillPoint));
        SetSelectedSkill(null);
        SetEnemyTarget(null);
        SetPlayerState(PlayerActionState.Idle);
        foreach (var enemy in FindObjectsOfType<EnemyCombat>())
        {
            enemy.Highlight(Color.white);
        }
    }
}