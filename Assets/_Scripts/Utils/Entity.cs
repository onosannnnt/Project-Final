using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class Entity : MonoBehaviour
{

    [SerializeField] private EntitiesBaseStat stats; // Base stats from ScriptableObject
    [SerializeField] private List<Skill> skills;
    protected float currentHealth;
    protected int currentSkillPoint;
    public BuffManager buffController;
    public SkillManager skillManager;
    protected Skill selectedSkill;

    protected virtual void Awake()
    {
        buffController = new BuffManager(this);
        skillManager = new SkillManager(this);
    }
    protected virtual void Start()
    {
        currentHealth = GetStat(StatType.MaxHealth);
        currentSkillPoint = (int)GetStat(StatType.MaxSkillPoint);
        skillManager.SetSkills(skills);
    }

    public virtual float CurrentHealth => math.min(currentHealth, GetStat(StatType.MaxHealth));
    public virtual int CurrentSP => currentSkillPoint;
    public virtual EntitiesBaseStat Stats => stats;
    public virtual Skill GetSelectedSkill => selectedSkill;
    public virtual void SetSelectedSkill(Skill skill)
    {
        selectedSkill = skill;
    }

    public virtual void TakeDamage(Damage damage)
    {
        float total = 0f;

        float mitigated = ApplyMitigation(damage);
        total += mitigated;

        currentHealth -= total;
        Debug.Log($"{gameObject.name} took {total} damage.");

        if (currentHealth <= 0)
            Die();
    }
    private float ApplyMitigation(Damage damage)
    {
        float resist = damage.Type switch
        {
            DamageType.Physical => GetStat(StatType.PhysicalDefense),
            DamageType.Fire => GetStat(StatType.FireResistance),
            DamageType.Cold => GetStat(StatType.ColdResistance),
            DamageType.Lightning => GetStat(StatType.LightningResistance),
            _ => 0
        };

        float reduced = damage.Amount * (1f - resist / 100f);
        return Mathf.Max(reduced, 0);
    }
    public virtual void Heal(Entity source, float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, stats.MaxHealth);
    }

    public virtual void SPRecover(int amount)
    {
        currentSkillPoint = Mathf.Min(currentSkillPoint + amount, stats.MaxSkillPoint);
    }

    public float GetStat(StatType stat)
    {
        float baseStat = stats.GetBase(stat);
        float flatStat = 0;
        float MultiplierStat = 1f;

        foreach (var buff in buffController.GetBuff())
        {
            foreach (var modifier in buff.modifiers)
            {
                if (modifier.Stat != stat) continue;
                switch (modifier.Type)
                {
                    case ModifierType.Flat:
                        flatStat += modifier.Value;
                        break;
                    case ModifierType.Percent:
                        if (buff.isStackable)
                        {
                            float totalPercent = 0f;
                            switch (buff.StackCalculationType)
                            {
                                case StackMultiplierType.Linear:
                                    totalPercent = modifier.Value * buff.Stack;
                                    break;
                                case StackMultiplierType.DiminishingReturn:
                                    {
                                        int linearStacks = 5; // ปรับได้
                                        for (int i = 0; i < buff.Stack; i++)
                                        {
                                            if (i < linearStacks)
                                            {
                                                totalPercent += modifier.Value;
                                            }
                                            else
                                            {
                                                int drIndex = i - linearStacks + 2;
                                                totalPercent += modifier.Value / drIndex;
                                            }
                                        }
                                        break;
                                    }
                            }
                            MultiplierStat *= 1 + totalPercent;
                        }
                        else
                        {
                            MultiplierStat *= 1 + modifier.Value;
                        }
                        break;
                }
            }
        }
        return (baseStat + flatStat) * MultiplierStat;
    }
    protected abstract void Die();
    protected virtual bool CanAction()
    {
        foreach (var buff in buffController.GetBuff())
        {
            if (buff.buffType == BuffType.CrowdControl)
            {
                return false;
            }
        }
        return true;
    }

}