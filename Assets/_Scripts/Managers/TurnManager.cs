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
    private List<ActionQueue> VirtualActionQueues = new List<ActionQueue>();
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
        VirtualActionQueues = new List<ActionQueue>(ActionQueues);
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
        combatUIManager.UpdateActionPanel();
        yield return new WaitForSeconds(1f);

        // ให้ VirtualActionQueues เป็นสำเนา
        VirtualActionQueues = new List<ActionQueue>(ActionQueues);

        while (VirtualActionQueues.Count > 0)
        {
            var action = VirtualActionQueues[0];
            VirtualActionQueues.RemoveAt(0); // เอาออกจากคิวทันที

            combatUIManager.UpdateActionPanel();

            if (IsPlayerWin())
            {
                ChangingState(TurnState.Win);
                yield break;
            }

            if (action.Entity == null)
                continue;

            if (action.Entity.CompareTag("Player"))
            {
                PlayerCombat.ProcessBuffs();
                PlayerCombat.UseSkill(action.Skill);
                combatUIManager.UpdatePlayerStatsUI();
            }
            else if (action.Entity.TryGetComponent<EnemyCombat>(out var enemyCombat))
            {
                enemyCombat.ProcessBuffs();
                enemyCombat.UseSkill(PlayerCombat);
                combatUIManager.UpdatePlayerStatsUI();
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

            combatUIManager.UpdateActionPanel();
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
    public List<ActionQueue> GetVirtualActionQueues()
    {
        return VirtualActionQueues;
    }
}