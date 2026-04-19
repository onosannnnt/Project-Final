using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [SerializeField] private GameObject DamageTextPrefab;
    [SerializeField] private Transform DamageCanvas;
    [SerializeField] private float FloatSpeed = 1f;
    [SerializeField] private float FloatDuration = 1f;
    [Header("Spread Settings")]
    public float SpreadAmount = 0.3f;       // How far left/right the text will drift
    public float RandomSpawnOffset = 0.1f; // Slight scatter so they don't spawn perfectly overlapping
    [Header("Properties")]
    [SerializeField] protected EntitiesBaseStat stats; // Base stats from ScriptableObject
    [SerializeField] private SkillLoadout skills;
    protected int ID;
    protected float currentHealth;
    protected int currentSkillPoint;
    public BuffManager buffController;
    public SkillManager skillManager;
    protected Skill selectedSkill;
    public List<IDamageModifier> OutgoingModifiers = new();
    public List<IDamageModifier> IncomingModifiers = new();

    public event System.Action<float, float> OnHealthChanged;
    public event System.Action<int, int> OnSPChanged;

    protected virtual void Awake()
    {
        if (stats != null) stats = stats.Clone();
        buffController = new BuffManager(this);
        skillManager = new SkillManager(this);
    }
    protected virtual void Start()
    {
        currentHealth = GetStat(StatType.MaxHealth);
        currentSkillPoint = (int)GetStat(StatType.MaxSkillPoint);
        
        if (skills != null)
        {
            skillManager.SetSkills(skills.EquippedSkills);
        }

        OutgoingModifiers.Add(new CriticalModifier());
        IncomingModifiers.Add(new ResistanceModifier());
        IncomingModifiers.Add(new EvasionModifier());
    }

    public virtual float CurrentHealth => math.min(currentHealth, GetStat(StatType.MaxHealth));
    public virtual int CurrentSP => currentSkillPoint;
    public virtual EntitiesBaseStat Stats => stats;
    public virtual SkillLoadout SkillLoadout => skills;
    public virtual Skill GetSelectedSkill => selectedSkill;

    public virtual void SetSkillLoadout(SkillLoadout loadout, bool applyImmediately = true)
    {
        skills = loadout;

        if (!applyImmediately || skillManager == null) return;

        if (skills != null)
        {
            skillManager.SetSkills(skills.EquippedSkills);
        }
        else
        {
            skillManager.SetSkills(new List<Skill>());
        }
    }

    public virtual void SetSelectedSkill(Skill skill)
    {
        selectedSkill = skill;
    }
    public virtual void SetEntityID(int id)
    {
        ID = id;
    }
    public virtual int GetEntityID()
    {
        return ID;
    }

    public virtual void TakeDamage(Damage damage)
    {
        float appliedDamage = damage.Amount;

        bool hasLethalProtection = buffController != null &&
                                   buffController.GetAllBuffs().Exists(b => b.Data != null && b.Data.preventLethalDamage);

        if (hasLethalProtection && currentHealth > 1)
        {
            // Clamp lethal damage so owner stays at 1 HP.
            appliedDamage = Mathf.Min(appliedDamage, currentHealth - 1f);
        }

        currentHealth = math.max(currentHealth - appliedDamage, 0);
        OnHealthChanged?.Invoke(currentHealth, GetStat(StatType.MaxHealth));
// // Debug.Log($"{gameObject.name} took {damage.Amount} damage, current health: {CurrentHealth}/{GetStat(StatType.MaxHealth)}");
        ShowDamage((int)appliedDamage, Utils.GetDamageColor(damage.Element), damage.IsCriticalHit);
        if (currentHealth <= 0)
        {
            // Mark as dead immediately so wave checks work
            MarkAsDead();
            // Defer destruction until after damage text animation
            StartCoroutine(DestroyAfterDelay());
        }
    }

    protected virtual void MarkAsDead()
    {
        // Override in subclasses to add specific death behavior
    }

    private System.Collections.IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(FloatDuration);
        Die();
    }

    public virtual void Heal(float amount)
    {
        bool hasHealingBlock = buffController != null &&
                               buffController.GetAllBuffs().Exists(b => b.Data != null && b.Data.preventHealing);
        if (hasHealingBlock)
        {
            return;
        }

        currentHealth = Mathf.Min(currentHealth + amount, GetStat(StatType.MaxHealth));
        OnHealthChanged?.Invoke(currentHealth, GetStat(StatType.MaxHealth));
        ShowDamage((int)amount, Color.green);
    }
    public virtual void SetSP(int amount)
    {
        currentSkillPoint = Mathf.Clamp(currentSkillPoint + amount, 0, (int)GetStat(StatType.MaxSkillPoint));
        OnSPChanged?.Invoke(currentSkillPoint, (int)GetStat(StatType.MaxSkillPoint));
// // Debug.Log($"{gameObject.name} SP changed by {amount}, current SP: {CurrentSP}/{(int)GetStat(StatType.MaxSkillPoint)}");

    }

    public float GetStat(StatType stat)
    {
        float baseStat = stats.GetBase(stat);
        float flatStat = 0;
        float MultiplierStat = 1f;

        foreach (var buff in buffController.GetAllBuffs())
        {
            foreach (var modifier in buff.Data.modifiers)
            {
                if (modifier.Stat != stat) continue;
                switch (modifier.Type)
                {
                    case ModifierType.Flat:
                        flatStat += modifier.Value;
                        break;
                    case ModifierType.Percent:
                        if (buff.Data.isStackable)
                        {
                            float totalPercent = 0f;
                            switch (buff.Data.StackCalculationType)
                            {
                                case StackMultiplierType.Linear:
                                    totalPercent = modifier.Value * buff.CurrentStack;
                                    break;
                                case StackMultiplierType.DiminishingReturn:
                                    {
                                        int linearStacks = 5; // ปรับได้
                                        for (int i = 0; i < buff.CurrentStack; i++)
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
        foreach (var buff in buffController.GetAllBuffs())
        {
            if (buff.Data.buffType == BuffType.CrowdControl)
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

        // OPTIMIZATION: Cache the text component so we only search for it once
        TextMeshProUGUI textComponent = damageTextObj.GetComponentInChildren<TextMeshProUGUI>();

        textComponent.text = damage.ToString() == "0" ? "Miss" : damage.ToString();
        textComponent.color = color;

        if (isCriticalHit)
        {
            textComponent.text += "!!";
        }

        if (damageTextObj != null && gameObject.activeInHierarchy)
        {
            StartCoroutine(FloatAndFade(damageTextObj));
        }
    }

    private System.Collections.IEnumerator FloatAndFade(GameObject damageTextObj)
    {
        float elapsedTime = 0f;

        // 1. Scatter the starting position slightly
        Vector3 randomSpawnOffset = new Vector3(
            UnityEngine.Random.Range(-RandomSpawnOffset, RandomSpawnOffset),
            UnityEngine.Random.Range(-RandomSpawnOffset, RandomSpawnOffset),
            0f
        );
        Vector3 startPosition = damageTextObj.transform.position + randomSpawnOffset;
        damageTextObj.transform.position = startPosition;

        // 2. Calculate a randomized drift direction (Up + Random Left/Right)
        float randomXDrift = UnityEngine.Random.Range(-SpreadAmount, SpreadAmount);

        // We use normalized so the text travels at the same consistent speed regardless of the angle
        Vector3 moveDirection = new Vector3(randomXDrift, 1f, 0f).normalized;

        while (elapsedTime < FloatDuration)
        {
            // 3. Move the text along the new randomized vector
            damageTextObj.transform.position = startPosition + (moveDirection * FloatSpeed * elapsedTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(damageTextObj);
    }

}
