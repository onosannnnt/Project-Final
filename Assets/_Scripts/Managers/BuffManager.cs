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
    public List<ActiveBuff> GetNegativeEffects()
    {
        return activeBuffs.FindAll(b => b.Data.buffType == BuffType.Debuff || b.Data.buffType == BuffType.CrowdControl);
    }

    public void AddBuff(Buff buff)
    {
        if (buff.buffType == BuffType.Debuff || buff.buffType == BuffType.CrowdControl)
        {
            if (activeBuffs.Exists(b => b.Data != null && b.Data.grantsDebuffImmunity))
            {
                // // Debug.Log($"{owner.gameObject.name} is immune to debuffs!");
                return;
            }
        }

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
            // Fix: Only call OnRefresh for existing buffs, OnApply is for the first time
            buff.OnRefresh(owner, existingBuff);
        }
        else
        {
            ActiveBuff newBuff = new ActiveBuff(buff);
            activeBuffs.Add(newBuff);

            // SPECIAL LOGIC: Newly gained Momentum is not usable until the next turn
            if (buff.BuffName == "Momentum")
            {
                newBuff.isUsable = false;
            }

            buff.OnApply(owner, newBuff);
        }
        
        Debug.Log($"[BuffManager] {owner.gameObject.name} added buff: {buff.BuffName} (Type: {buff.buffType}, Icon: {(buff.Icon != null ? buff.Icon.name : "NULL")}). Total buffs: {activeBuffs.Count}");
        OnBuffsChanged?.Invoke();
    }
    public void RemoveBuff(ActiveBuff buff)
    {
        if (buff == null || !activeBuffs.Contains(buff)) return;

        Debug.Log($"[BuffManager] {owner.gameObject.name} REMOVING buff: {buff.Data.BuffName}. Reason: Manual/Scripted call.");
        buff.Data.OnRemove(owner, buff);
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
    }
    public virtual void OnApply(Entity owner, ActiveBuff buff)
    {
        buff.Data.OnApply(owner, buff);
    }
    public virtual void OnTurnStart(Entity owner, CombatActionLog log)
    {
        List<ActiveBuff> snapshot = new List<ActiveBuff>(activeBuffs);
        foreach (var buff in snapshot)
        {
            buff.isUsable = true; // Reset usability at turn start
            buff.Data.OnTurnStart(owner, log, buff);
        }
    }
    public virtual void OnTurnEnd(Entity owner, CombatActionLog log)
    {
        List<ActiveBuff> toRemove = new();
        List<ActiveBuff> snapshot = new List<ActiveBuff>(activeBuffs);
        foreach (var buff in snapshot)
        {
            buff.Data.OnTurnEndAction(owner, log, buff);
            buff.Data.OnTurnEnd(owner, buff);
            if (!buff.Data.isPermanent && buff.CurrentDuration <= 0)
            {
                toRemove.Add(buff);
            }
        }
        // // Debug.Log(owner.gameObject.name + " has " + toRemove.Count + " buffs to remove.");
        foreach (var buff in toRemove)
        {
            Debug.Log($"[BuffManager] {owner.gameObject.name} REMOVING buff: {buff.Data.BuffName}. Reason: Duration expired ({buff.CurrentDuration}) and not permanent.");
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
