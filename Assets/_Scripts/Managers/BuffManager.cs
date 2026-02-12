using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class BuffManager
{
    private Entity owner;
    private List<Buff> activeBuffs = new List<Buff>();

    public BuffManager(Entity owner)
    {
        this.owner = owner;
    }
    public List<Buff> GetBuff()
    {
        return activeBuffs;
    }

    public void addBuff(Buff buff)
    {
        Buff existingBuff = activeBuffs.Find(b => b.BuffName == buff.BuffName);
        if (existingBuff != null)
        {
            existingBuff.Duration = Mathf.Max(existingBuff.Duration, buff.Duration);
            if (existingBuff.isStackable) existingBuff.Stack += 1;
        }
        else activeBuffs.Add(buff.Clone());
        OnApply(owner, buff);
    }
    public void removeBuff(Buff buff)
    {
        Debug.Log(owner.gameObject.name + " lost buff: " + buff.BuffName);
        activeBuffs.Remove(buff);
    }
    public virtual void OnApply(Entity owner, Buff buff)
    {
        buff.OnApply(owner);
    }
    public virtual void OnTurnStart(Entity owner)
    {
        foreach (var buff in activeBuffs)
        {
            buff.OnTurnStart(owner);
        }
    }
    public virtual void OnTurnEnd(Entity owner)
    {
        List<Buff> toRemove = new();
        foreach (var buff in activeBuffs)
        {
            buff.OnTurnEnd(owner);
            if (buff.Duration <= 0)
            {
                toRemove.Add(buff);
            }
        }
        Debug.Log(owner.gameObject.name + " has " + toRemove.Count + " buffs to remove.");
        foreach (var buff in toRemove)
        {
            buff.OnRemove(owner);
            activeBuffs.Remove(buff);
        }
    }
    public Buff GetBuffByName(string buffName)
    {
        return activeBuffs.Find(b => b.BuffName == buffName);
    }
}