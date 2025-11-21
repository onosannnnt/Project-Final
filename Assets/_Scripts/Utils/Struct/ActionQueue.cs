using UnityEngine;

public struct ActionQueue
{
    public GameObject Entity;
    public Skill Skill;
    public float ActionSpeed;
    public ActionQueue(GameObject owner, Skill skill, float actionSpeed)
    {
        Entity = owner;
        Skill = skill;
        ActionSpeed = actionSpeed;
    }
}