using System;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [SerializeField] private EntitiesBaseStat stats; // Base stats from ScriptableObject
    protected float currentHealth;
    protected int currentSkillPoint;
    protected List<StatusBuff> activeBuffs = new List<StatusBuff>();
    protected Skill selectedSkill;

    protected virtual void Start()
    {
        currentHealth = stats.MaxHealth;
        currentSkillPoint = stats.MaxSkillPoint;
    }

    public virtual float CurrentHealth => currentHealth;
    public virtual int CurrentSP => currentSkillPoint;
    public virtual EntitiesBaseStat Stats => stats;
    public virtual Skill SelectedSkill => selectedSkill;

    public virtual void TakeDamage(Damage damage)
    {
        float physicalDamageAfterMitigation = Mathf.Max(damage.PhysicalDamage * (1 - stats.PhysicalDefense / 100f), 0);
        float fireDamageAfterMitigation = Mathf.Max(damage.FireDamage * (1 - stats.FireResistance / 100f), 0);
        float coldDamageAfterMitigation = Mathf.Max(damage.ColdDamage * (1 - stats.ColdDamageMultiplier / 100f), 0);
        float lightningDamageAfterMitigation = Mathf.Max(damage.LightningDamage * (1 - stats.LightningResistance / 100f), 0);
        float totalDamage = physicalDamageAfterMitigation + fireDamageAfterMitigation + coldDamageAfterMitigation + lightningDamageAfterMitigation;
        currentHealth -= totalDamage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public virtual void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, stats.MaxHealth);
    }
    public virtual void SPRecover(int amount)
    {
        currentSkillPoint = Mathf.Min(currentSkillPoint + amount, stats.MaxSkillPoint);
    }
    public virtual void ApplyBuff(StatusBuff buff)
    {
        if (buff == null) return;
        StatusBuff existingBuff = activeBuffs.Find(b => b.BuffType == buff.BuffType);
        if (existingBuff != null)
        {
            if (buff.IsStackable)
            {
                existingBuff.Stack += buff.Stack;
            }
            existingBuff.Duration = buff.Duration;
        }
        else
        {
            activeBuffs.Add(buff.Clone());
        }
    }
    protected abstract void Die();
}