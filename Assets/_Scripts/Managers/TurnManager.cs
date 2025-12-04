using System.Collections.Generic;
using UnityEngine;

public class TurnManager : Singleton<TurnManager>
{
    [SerializeField] private Transform worldParent;
    private TurnState currentState;
    private List<ActionQueue> actionQueue = new List<ActionQueue>();
    private PlayerCombat playerCombat;
    private void Start()
    {
        playerCombat = FindObjectOfType<PlayerCombat>();
        SetState(TurnState.PlayerTurnState);
    }
    private void SetState(TurnState newState)
    {
        currentState = newState;
        switch (currentState)
        {
            case TurnState.PlayerTurnState:
                HandlePlayerTurn();
                break;
            case TurnState.SpeedCompareState:
                HandleSpeedComparison();
                break;
            case TurnState.ActionState:
                HandleActionExecution();
                break;
            case TurnState.Win:
                // Handle win condition
                break;
            case TurnState.Lose:
                Loader.Load(Loader.Scenes.Overworld);
                break;
        }
    }
    private void HandlePlayerTurn()
    {
        Debug.Log("Player's Turn");
        Damage damage = new Damage
        {
            PhysicalDamage = 10f,
            FireDamage = 0f,
            ColdDamage = 0f,
            LightningDamage = 0f
        };
        playerCombat.TakeDamage(damage);
        Debug.Log("Player Health: " + playerCombat.CurrentHealth + " with : " + (playerCombat.CurrentHealth / playerCombat.Stats.MaxHealth * 100) + "%");
        CombatPlayerUI.Instance.GetComponent<CombatPlayerUI>().UpdateHealthBar();
    }

    private void HandleSpeedComparison()
    {
        ActionQueue playerAction = new ActionQueue();
        playerAction.Entity = playerCombat.gameObject;
        playerAction.Skill = playerCombat.SelectedSkill;
        playerAction.ActionSpeed = playerCombat.Stats.ActionSpeed;
        actionQueue.Add(playerAction);
        List<GameObject> enemies = GetAllEnemies();
        foreach (GameObject enemy in enemies)
        {
            Entity enemyEntity = enemy.GetComponent<Entity>(); //TODO: Change to EnemyCombat when created
            ActionQueue enemyAction = new ActionQueue();
            enemyAction.Entity = enemy;
            enemyAction.Skill = enemyEntity.SelectedSkill;
            enemyAction.ActionSpeed = enemyEntity.Stats.ActionSpeed;
            actionQueue.Add(enemyAction);
        }
        actionQueue.Sort((a, b) => b.ActionSpeed.CompareTo(a.ActionSpeed));
        SetState(TurnState.ActionState);
    }
    private IEnumerable<WaitForSeconds> HandleActionExecution()
    {
        while (actionQueue.Count > 0)
        {
            ActionQueue currentAction = actionQueue[0];
            GameObject entity = currentAction.Entity;
            Skill skill = currentAction.Skill;
            // Execute the skill (implementation depends on your Skill class)
            Debug.Log($"{entity.name} uses {skill.SkillName}");
            actionQueue.RemoveAt(0);
            yield return new WaitForSeconds(1f);
        }
        SetState(TurnState.PlayerTurnState);
        yield break;
    }
    private void Win()
    {

    }
    private List<GameObject> GetAllEnemies()
    {
        List<GameObject> enemies = new List<GameObject>();
        foreach (Transform child in worldParent)
        {
            if (child.CompareTag("Enemy"))
            {
                enemies.Add(child.gameObject);
            }
        }
        return enemies;
    }
}