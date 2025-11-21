using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : Singleton<TurnManager>
{
    [SerializeField] private CombatUIManager combatUIManager;
    [SerializeField] private Transform WorldParent;
    [SerializeField] private PlayerCombat PlayerCombat;
    public TurnState State { get; private set; }

    private List<ActionQueue> ActionQueues = new List<ActionQueue>();
    private Skill SelectedSkill;


    private void Start()
    {
        ChangingState(TurnState.PlayerTurnState);
    }
    private void Update()
    {
        // For testing purposes: Cycle through states with the space key
        if (Input.GetKeyDown(KeyCode.Space))
        {
            switch (State)
            {
                case TurnState.PlayerTurnState:
                    ChangingState(TurnState.SpeedCompareState);
                    break;
                case TurnState.SpeedCompareState:
                    ChangingState(TurnState.ActionState);
                    break;
                case TurnState.ActionState:
                    ChangingState(TurnState.PlayerTurnState);
                    break;
            }
        }
    }

    public void ChangingState(TurnState newState)
    {
        if (State == newState) return;
        State = newState;

        switch (State)
        {
            case TurnState.PlayerTurnState:
                HandlePlayerTurnState();
                break;
            case TurnState.SpeedCompareState:
                HandleSpeedCompareState();
                break;
            case TurnState.ActionState:
                HandleActionState();
                break;
            case TurnState.Win:
                HandleWinState();
                break;
            case TurnState.Lose:
                HandleLoseState();
                break;
        }
    }
    private void HandlePlayerTurnState()
    {
        combatUIManager.UpdatePlayerUIActive(true);
        combatUIManager.SetTurnHeaderText("Player's Turn");
    }
    private void HandleSpeedCompareState()
    {
        combatUIManager.UpdatePlayerUIActive(false);
        ActionQueues.Clear();
        combatUIManager.SetSkillListActive(false);
        //player
        ActionQueue PlayerActionQueue = new ActionQueue();
        PlayerActionQueue.Entity = PlayerCombat.gameObject;
        PlayerActionQueue.ActionSpeed = PlayerCombat.GetPlayerStats().ActionSpeed;
        PlayerActionQueue.Skill = SelectedSkill;
        ActionQueues.Add(PlayerActionQueue);
        //enemies
        EnemyCombat[] enemies = WorldParent.GetComponentsInChildren<EnemyCombat>();
        List<EnemyCombat> enemyList = new List<EnemyCombat>(enemies);
        foreach (var enemy in enemyList)
        {
            ActionQueue actionQueue = new ActionQueue();
            actionQueue.Entity = enemy.gameObject;
            actionQueue.ActionSpeed = enemy.GetEnemyStats().ActionSpeed;
            actionQueue.Skill = enemy.RandomSkill(); // For simplicity, enemy uses a random skill
            ActionQueues.Add(actionQueue);
        }
        ActionQueues.Sort((a, b) => b.ActionSpeed.CompareTo(a.ActionSpeed));
        combatUIManager.SetTurnHeaderText("Comparing Speeds");
        foreach (var action in ActionQueues)
        {
            Debug.Log("Entity: " + action.Entity.name + ", Action Speed: " + action.ActionSpeed);
        }
        ChangingState(TurnState.ActionState);
    }
    private void HandleLoseState()
    {
        combatUIManager.SetTurnHeaderText("You Lose!");
    }
    private void HandleWinState()
    {
        combatUIManager.SetTurnHeaderText("You Win!");
    }
    private void HandleActionState()
    {
        combatUIManager.SetTurnHeaderText("Action Phase");
        StartCoroutine(ExecuteActionPhase());
    }
    private IEnumerator ExecuteActionPhase()
    {
        foreach (var action in ActionQueues)
        {
            // Debug.Log("Executing action for: " + action.Entity.name + " with speed " + action.ActionSpeed + " with skill " + (action.Skill != null ? action.Skill.skillName : "None"));
            if (action.Entity.CompareTag("Player"))
            {
                PlayerCombat.UseSkill(action.Skill);
                combatUIManager.UpdatePlayerStatsUI();
            }
            else if (action.Entity.CompareTag("Enemy"))
            {
                EnemyCombat enemyCombat = action.Entity.GetComponent<EnemyCombat>();
                if (enemyCombat != null)
                {
                    enemyCombat.UseSkill(PlayerCombat);
                    combatUIManager.UpdatePlayerStatsUI();
                }
            }
            if (IsPlayerWin())
            {
                ChangingState(TurnState.Win);
                yield break;
            }
            yield return new WaitForSeconds(1f);
        }
        if (PlayerCombat.GetPlayerStats().CurrentHealth <= 0)
        {
            ChangingState(TurnState.Lose);
        }
        else
        {
            ChangingState(TurnState.PlayerTurnState);
        }
    }
    private bool IsPlayerWin()
    {
        EnemyCombat[] enemies = WorldParent.GetComponentsInChildren<EnemyCombat>();
        Debug.Log("Remaining Enemies: " + enemies.Length);
        return enemies.Length == 0;
    }
    public void setSelectedSkill(Skill skill)
    {
        SelectedSkill = skill;
    }

}