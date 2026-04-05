using System.Collections.Generic;
using UnityEngine;

public class BuffManager
{
    private Entity owner;
    private List<ActiveBuff> activeBuffs = new List<ActiveBuff>();

    public event System.Action OnBuffsChanged;

    public BuffManager(Entity owner)
    {
        this.owner = owner;
    }
    public List<ActiveBuff> GetAllBuffs()
    {
        return activeBuffs;
    }
    public List<ActiveBuff> GetBuffs()
    {
        List<ActiveBuff> buffs = activeBuffs.FindAll(b => b.Data.buffType == BuffType.Buff);
        return buffs;
    }
    public List<ActiveBuff> GetDebuffs()
    {
        List<ActiveBuff> debuffs = activeBuffs.FindAll(b => b.Data.buffType == BuffType.Debuff);
        return debuffs;
    }

    public void AddBuff(Buff buff)
    {
        ActiveBuff existingBuff = activeBuffs.Find(b => b.Data.BuffName == buff.BuffName);

        if (existingBuff != null)
        {
            existingBuff.CurrentDuration = buff.Duration;
            existingBuff.wasReappliedThisTurn = true;

            if (buff.isStackable)
            {
                existingBuff.CurrentStack += 1;
                if (buff.MaxStack > 0 && existingBuff.CurrentStack > buff.MaxStack)
                {
                    existingBuff.CurrentStack = buff.MaxStack;
                }
            }
            buff.OnApply(owner, existingBuff);
            buff.OnRefresh(owner, existingBuff);
        }
        else
        {
            ActiveBuff newBuff = new ActiveBuff(buff);
            activeBuffs.Add(newBuff);
            buff.OnApply(owner, newBuff);
        }
        OnBuffsChanged?.Invoke();
    }
    public void RemoveBuff(ActiveBuff buff)
    {
        Debug.Log(owner.gameObject.name + " lost buff: " + buff.Data.BuffName);
        activeBuffs.Remove(buff);
        OnBuffsChanged?.Invoke();
    }
    public void ConsumeBuffStack(ActiveBuff buff, int stacksToConsume)
    {
        if (buff == null || !activeBuffs.Contains(buff)) return;

        buff.CurrentStack -= stacksToConsume;

        if (buff.CurrentStack <= 0)
        {
            RemoveBuff(buff);
        }
        else
        {
            OnBuffsChanged?.Invoke();
        }
    }    public virtual void OnApply(Entity owner, ActiveBuff buff)
    {
        buff.Data.OnApply(owner, buff);
    }
    public virtual void OnTurnStart(Entity owner, CombatActionLog log)
    {
        foreach (var buff in activeBuffs)
        {
            buff.Data.OnTurnStart(owner, log, buff);
        }
    }
    public virtual void OnTurnEnd(Entity owner)
    {
        List<ActiveBuff> toRemove = new();
        foreach (var buff in activeBuffs)
        {
            buff.Data.OnTurnEnd(owner, buff);
            if (!buff.Data.isPermanent && buff.CurrentDuration <= 0)
            {
                toRemove.Add(buff);
            }
        }
        Debug.Log(owner.gameObject.name + " has " + toRemove.Count + " buffs to remove.");
        foreach (var buff in toRemove)
        {
            buff.Data.OnRemove(owner, buff);
            activeBuffs.Remove(buff);
        }
        if (toRemove.Count > 0)
        {
            OnBuffsChanged?.Invoke();
        }
    }
    public ActiveBuff GetBuffByName(string buffName)
    {
        return activeBuffs.Find(b => b.Data.BuffName == buffName);
    }
    public List<ActiveBuff> GetBuffsByType(BuffType type)
    {
        return activeBuffs.FindAll(b => b.Data.buffType == type);
    }
}