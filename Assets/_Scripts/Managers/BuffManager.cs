using System.Collections.Generic;
using UnityEngine;

public class BuffManager
{
    private Entity owner;
    private List<Buff> activeBuffs = new List<Buff>();

    public BuffManager(Entity owner)
    {
        this.owner = owner;
    }
    public List<Buff> GetAllBuffs()
    {
        return activeBuffs;
    }
    public List<Buff> GetBuffs()
    {
        List<Buff> buffs = activeBuffs.FindAll(b => b.buffType == BuffType.Buff);
        return buffs;
    }
    public List<Buff> GetDebuffs()
    {
        List<Buff> debuffs = activeBuffs.FindAll(b => b.buffType == BuffType.Debuff);
        return debuffs;
    }

    public void AddBuff(Buff buff)
    {
        Buff existingBuff = activeBuffs.Find(b => b.BuffName == buff.BuffName);

        if (existingBuff != null)
        {
            existingBuff.Duration = buff.Duration;
            existingBuff.wasReappliedThisTurn = true;

            if (existingBuff.isStackable)
            {
                existingBuff.Stack += 1;
            }
            existingBuff.OnApply(owner);
            existingBuff.OnRefresh(owner);
        }
        else
        {
            Buff newBuff = buff.Clone();
            activeBuffs.Add(newBuff);
            newBuff.OnApply(owner);
        }
    }
    public void RemoveBuff(Buff buff)
    {
        Debug.Log(owner.gameObject.name + " lost buff: " + buff.BuffName);
        activeBuffs.Remove(buff);
    }
    public virtual void OnApply(Entity owner, Buff buff)
    {
        buff.OnApply(owner);
    }
    public virtual void OnTurnStart(Entity owner, CombatActionLog log)
    {
        foreach (var buff in activeBuffs)
        {
            buff.OnTurnStart(owner, log);
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
    public List<Buff> GetBuffsByType(BuffType type)
    {
        return activeBuffs.FindAll(b => b.buffType == type);
    }
}