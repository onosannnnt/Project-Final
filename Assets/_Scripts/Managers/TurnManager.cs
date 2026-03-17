using System;
using System.Collections;
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
        playerCombat = PlayerCombat.instance;
        SetState(TurnState.PlayerTurnState);
    }
    public void SetState(TurnState newState)
    {
        currentState = newState;
        switch (currentState)
        {
            case TurnState.PlayerTurnState:
                HandlePlayerTurn();
                break;
            case TurnState.SpeedCompareState:
                List<GameObject> enemies = GetAllEnemies();
                if (enemies.Count == 0) return;
                foreach (GameObject enemy in enemies)
                {
                    EnemyCombat enemyCombat = enemy.GetComponent<EnemyCombat>();
                    playerCombat.SetPlayerState(PlayerActionState.Action);
                    enemyCombat.Highlight(Color.white);
                }
                HandleSpeedComparison();
                break;
            case TurnState.ActionState:
                StartCoroutine(HandleActionExecution());
                break;
            case TurnState.Win:
                Win();
                break;
            case TurnState.Lose:
                Loader.Load(Loader.Scenes.Overworld);
                break;
        }
    }
    private void HandlePlayerTurn()
    {
        playerCombat.SetEnemyTarget(GetFirstEnemy());
        TargetingPanel.instance.SetEnemyTargetPanel(playerCombat.GetEnemyTarget());
        ActionBarUI.Instance.gameObject.SetActive(true);
    }

    private void HandleSpeedComparison()
    {
        ActionBarUI.Instance.gameObject.SetActive(false);
        SkillPanelUI.Instance.gameObject.SetActive(false);
        ActionQueue playerAction = new ActionQueue
        {
            Caster = playerCombat,
            Target = playerCombat.GetEnemyTarget(),
            Skill = playerCombat.GetSelectedSkill,
            ActionSpeed = playerCombat.GetStat(StatType.ActionSpeed)
        };
        actionQueue.Add(playerAction);
        List<GameObject> enemies = GetAllEnemies();
        foreach (GameObject enemy in enemies)
        {
            Entity enemyEntity = enemy.GetComponent<EnemyCombat>();
            ActionQueue enemyAction = new ActionQueue
            {
                Caster = enemyEntity,
                Target = playerCombat,
                Skill = enemyEntity.skillManager.RandomSkill(),
                ActionSpeed = enemyEntity.Stats.ActionSpeed
            };
            actionQueue.Add(enemyAction);
        }
        actionQueue.Sort((a, b) => b.ActionSpeed.CompareTo(a.ActionSpeed));
        SetState(TurnState.ActionState);
    }
    private IEnumerator HandleActionExecution()
    {
        yield return new WaitForSeconds(1f);
        while (actionQueue.Count > 0)
        {
            ActionQueue currentAction = actionQueue[0];
            Entity entity = currentAction.Caster;
            Skill skill = currentAction.Skill;
            Entity target = currentAction.Target;

            Debug.Log(entity.gameObject.name + " is taking action with " + currentAction.ActionSpeed + " speed.");
            if (entity.CompareTag("Player"))
            {
                playerCombat.buffController.OnTurnStart(playerCombat);
                if (entity.CanAction() == true) playerCombat.Action();
                playerCombat.buffController.OnTurnEnd(playerCombat);
            }
            else if (entity.CompareTag("Enemy"))
            {
                EnemyCombat enemyCombat = entity.GetComponent<EnemyCombat>();
                enemyCombat.buffController.OnTurnStart(enemyCombat);
                if (entity.CanAction() == true) enemyCombat.skillManager.UseSkill(skill, playerCombat);
                enemyCombat.buffController.OnTurnEnd(enemyCombat);
            }
            actionQueue.RemoveAt(0);
            List<GameObject> remainingEnemies = GetAllEnemies();
            remainingEnemies.RemoveAll(enemy => enemy.GetComponent<EnemyCombat>().IsDead());
            if (remainingEnemies.Count == 0)
            {
                SetState(TurnState.Win);
                yield break;
            }
            yield return new WaitForSeconds(1f);
        }
        if (playerCombat.CurrentHealth <= 0)
        {
            SetState(TurnState.Lose);
            yield break;
        }

        SetState(TurnState.PlayerTurnState);
        yield break;
    }

    private void Win()
    {
        Debug.Log("You Win!");
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
    public void RemoveEntityFromActionQueue(Entity entity)
    {
        actionQueue.RemoveAll(a => a.Caster == entity);
    }
    private EnemyCombat GetFirstEnemy()
    {
        var enemies = FindObjectsOfType<EnemyCombat>();
        if (enemies.Length == 0) return null;
        return enemies[0];
    }
    public TurnState GetTurnState()
    {
        return currentState;
    }
}