using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [SerializeField] private GameObject DamageTextPrefab;
    [SerializeField] private Transform DamageCanvas;
    [SerializeField] private float FloatSpeed = 1f;
    [SerializeField] private float FloatDuration = 1f;
    [SerializeField] private EntitiesBaseStat stats; // Base stats from ScriptableObject
    [SerializeField] private SkillLoadout skills;
    protected float currentHealth;
    protected int currentSkillPoint;
    public BuffManager buffController;
    public SkillManager skillManager;
    protected Skill selectedSkill;
    public List<IDamageModifier> OutgoingModifiers = new();
    public List<IDamageModifier> IncomingModifiers = new();

    protected virtual void Awake()
    {
        buffController = new BuffManager(this);
        skillManager = new SkillManager(this);
    }
    protected virtual void Start()
    {
        currentHealth = GetStat(StatType.MaxHealth);
        currentSkillPoint = (int)GetStat(StatType.MaxSkillPoint);
        skillManager.SetSkills(skills.EquippedSkills);

        OutgoingModifiers.Add(new CriticalModifier());
        IncomingModifiers.Add(new ResistanceModifier());
        IncomingModifiers.Add(new EvasionModifier());
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

        currentHealth = math.max(currentHealth - damage.Amount, 0);
        Debug.Log($"{gameObject.name} took {damage.Amount} damage, current health: {CurrentHealth}/{GetStat(StatType.MaxHealth)}");
        ShowDamage((int)damage.Amount, Utils.GetDamageColor(damage.Type), damage.IsCriticalHit);
        if (currentHealth <= 0)
            Die();
    }

    public virtual void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, GetStat(StatType.MaxHealth));
        ShowDamage((int)amount, Color.green);
    }
    public virtual void SetSP(int amount)
    {
        currentSkillPoint = Mathf.Clamp(currentSkillPoint + amount, 0, (int)GetStat(StatType.MaxSkillPoint));
        Debug.Log($"{gameObject.name} SP changed by {amount}, current SP: {CurrentSP}/{(int)GetStat(StatType.MaxSkillPoint)}");

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
    public virtual bool CanAction()
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
    public void AddModifier(IDamageModifier modifier, EntityModifierType modifierType)
    {
        if (modifierType == EntityModifierType.Outgoing)
            OutgoingModifiers.Add(modifier);
        else if (modifierType == EntityModifierType.Incoming)
            IncomingModifiers.Add(modifier);
    }
    public void RemoveModifier(IDamageModifier modifier, EntityModifierType modifierType)
    {
        if (modifierType == EntityModifierType.Outgoing)
            OutgoingModifiers.Remove(modifier);
        else if (modifierType == EntityModifierType.Incoming)
            IncomingModifiers.Remove(modifier);
    }
    public void ShowDamage(int damage, Color color, bool isCriticalHit = false)
    {
        GameObject damageTextObj = Instantiate(DamageTextPrefab, DamageCanvas);
        damageTextObj.GetComponentInChildren<TextMeshProUGUI>().text = damage.ToString() == "0" ? "Miss" : damage.ToString();
        if (isCriticalHit)
        {
            damageTextObj.GetComponentInChildren<TextMeshProUGUI>().text += "!!";
        }
        damageTextObj.GetComponentInChildren<TextMeshProUGUI>().color = color;
        if (damageTextObj != null)
        {
            StartCoroutine(FloatAndFade(damageTextObj));
        }
    }
    private System.Collections.IEnumerator FloatAndFade(GameObject damageTextObj)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = damageTextObj.transform.position;
        while (elapsedTime < FloatDuration)
        {
            damageTextObj.transform.position = startPosition + Vector3.up * FloatSpeed * elapsedTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(damageTextObj);
    }
}