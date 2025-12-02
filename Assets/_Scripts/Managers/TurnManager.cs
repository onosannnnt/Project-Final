using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : Singleton<TurnManager>
{
    [SerializeField] private CombatUIManager combatUIManager;
    [SerializeField] private Transform WorldParent;
    [SerializeField] private PlayerCombat PlayerCombat;
    [SerializeField] private DropManager DropManager;
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
        ChangingState(TurnState.ActionState);
    }
    private void HandleLoseState()
    {
        combatUIManager.SetTurnHeaderText("You Lose!");
    }
    private void HandleWinState()
    {
        combatUIManager.SetTurnHeaderText("You Win!");
        DropManager.GenerateItemDrop();

    }
    private void HandleActionState()
    {
        combatUIManager.SetTurnHeaderText("Action Phase");
        StartCoroutine(ExecuteActionPhase());
    }
    public void HandleTargetEnemyUI()
    {
        combatUIManager.SetTurnHeaderText("Select an Enemy Target");
    }
    private IEnumerator ExecuteActionPhase()
    {
        foreach (var action in ActionQueues)
        {
            if (IsPlayerWin())
            {
                ChangingState(TurnState.Win);
                yield break;
            }
            // Debug.Log("Executing action for: " + action.Entity.name + " with speed " + action.ActionSpeed + " with skill " + (action.Skill != null ? action.Skill.skillName : "None"));
            if (action.Entity.CompareTag("Player"))
            {
                PlayerCombat.ProcessBuffs();
                PlayerCombat.UseSkill(action.Skill);
                combatUIManager.UpdatePlayerStatsUI();
                Debug.Log("Player buff : " + (PlayerCombat.GetAppliedBuffs().Count > 0 ? PlayerCombat.GetAppliedBuffs()[0].buffType.ToString() : "No Buffs"));

            }
            else if (action.Entity.CompareTag("Enemy"))
            {
                EnemyCombat enemyCombat = action.Entity.GetComponent<EnemyCombat>();
                if (enemyCombat != null)
                {
                    enemyCombat.ProcessBuffs();
                    enemyCombat.UseSkill(PlayerCombat);
                    combatUIManager.UpdatePlayerStatsUI();
                    Debug.Log("Enemy buff : " + (enemyCombat.GetAppliedBuffs().Count > 0 ?
                        enemyCombat.GetAppliedBuffs()[0].buffType.ToString() + " Duration: " + enemyCombat.GetAppliedBuffs()[0].duration + " Stacks: " + enemyCombat.GetAppliedBuffs()[0].Stack
                        :
                        "No Buffs"));
                }
            }
            if (IsPlayerWin())
            {
                ChangingState(TurnState.Win);
                yield break;
            }
            else if (PlayerCombat.GetPlayerStats().CurrentHealth <= 0)
            {
                ChangingState(TurnState.Lose);
                yield break;
            }
            yield return new WaitForSeconds(1f);
        }
        if (IsPlayerWin())
        {
            ChangingState(TurnState.Win);
            yield break;
        }
        if (State != TurnState.Win && State != TurnState.Lose)
        {
            ChangingState(TurnState.PlayerTurnState);
        }
    }
    private bool IsPlayerWin()
    {
        EnemyCombat[] enemies = WorldParent.GetComponentsInChildren<EnemyCombat>();
        return enemies.Length == 0;
    }
    public void SetSelectedSkill(Skill skill)
    {
        SelectedSkill = skill;
    }
    public void RemoveKilledEnemy(EnemyCombat enemy)
    {
        ActionQueues.RemoveAll(a => a.Entity == enemy.gameObject);
    }

}