
using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionQueueUI : MonoBehaviour
{
    [SerializeField] private Transform actionQueueContainer;
    [SerializeField] private GameObject actionQueuePrefab;
    private ActionQueue actionQueue;
    private void Start()
    {
        SetupActionQueue();
    }
    private void SetupActionQueue()
    {
        foreach (Transform child in actionQueueContainer)
        {
            Destroy(child.gameObject);
        }
        if (actionQueue == null) return;
        GameObject actionQueueGO = Instantiate(actionQueuePrefab, actionQueueContainer);
        ActionQueuePrefab ui = actionQueueGO.GetComponent<ActionQueuePrefab>();
        ui.Setup(actionQueue.Skill.skillIcon, actionQueue.Skill.skillName);
    }
    public void SetActionQueue(ActionQueue actions)
    {
        actionQueue = actions;
        SetupActionQueue();
    }
}