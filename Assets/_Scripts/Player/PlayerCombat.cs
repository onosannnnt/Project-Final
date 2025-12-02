using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    // Player Stats
    public static PlayerCombat Instance { get; private set; }
    [SerializeField] private EntitiesBaseStat playerBaseStats;
    private Stat playerStats;
    //skills
    [SerializeField] private List<Skill> playerSkills;
    //world
    [SerializeField] private Transform worldParent;
    [SerializeField] private TurnManager TurnManager;

    private PlayerState playerState;
    private Skill currentSkill;
    private EnemyCombat targetedEnemy;
    private List<StatusBuff> activeBuffs = new List<StatusBuff>();
    private List<StatusBuff> skillBuffs = new List<StatusBuff>();


    //monobehavior lifecycle method

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        playerStats.Level = playerBaseStats.Level;
        playerStats.ExperiencePoints = playerBaseStats.ExperiencePoints;
        playerStats.Strength = playerBaseStats.Strength;
        playerStats.Intelligence = playerBaseStats.Intelligence;
        playerStats.Agility = playerBaseStats.Agility;
        playerStats.MaxHealth = playerBaseStats.MaxHealth;
        playerStats.CurrentHealth = playerStats.MaxHealth;
        playerStats.MaxSP = playerBaseStats.MaxSkillPoint;
        playerStats.CurrentSP = playerStats.MaxSP;
        playerStats.PhysicalAttack = playerBaseStats.PhysicalAttack;
        playerStats.MagicAttack = playerBaseStats.MagicAttack;
        playerStats.PhysicalDefense = playerBaseStats.PhysicalDefense;
        playerStats.FireResistance = playerBaseStats.FireResistance;
        playerStats.ColdResistance = playerBaseStats.ColdResistance;
        playerStats.LightningResistance = playerBaseStats.LightningResistance;
        playerStats.ActionSpeed = playerBaseStats.ActionSpeed;
        playerStats.CriticalChance = playerBaseStats.CriticalHitChance;
        playerStats.CriticalDamageMultiplier = playerBaseStats.CriticalHitDamageMultiplier;
        playerStats.Accuracy = playerBaseStats.Accuracy;
        playerStats.EvasionRate = playerBaseStats.EvasionRate;
        playerStats.StatusEffectResistance = playerBaseStats.StatusEffectResistance;
        playerStats.StatusHitChance = playerBaseStats.StatusHitChance;
    }

    //set-get method
    public Stat GetPlayerStats()
    {
        return playerStats;
    }

    public List<Skill> GetSkills()
    {
        return playerSkills;
    }
    public List<StatusBuff> GetAppliedBuffs()
    {
        return activeBuffs;
    }

    //skill usage method
    public void UseSkill(Skill skill)
    {
        if (playerStats.CurrentSP < skill.spCost)
        {
            Debug.Log("Not enough SP to use " + skill.skillName);
            return;
        }
        switch (skill.targetType)
        {
            case TargetType.SingleEnemy:
                UseSkillOnSingleEnemy(skill, targetedEnemy);
                break;
            case TargetType.AllEnemies:
                HandleAllEnemiesTargeting(skill);
                break;
            case TargetType.Self:
                HandleSelfTargeting(skill);
                break;
        }
    }
    public void HandleSingleEnemyTargeting(Skill skill)
    {
        SetState(PlayerState.Targeting);
        TurnManager.HandleTargetEnemyUI();
        currentSkill = skill;
        HighlightEnemies(true);
    }

    private void HandleAllEnemiesTargeting(Skill skill)
    {
        skillBuffs.Clear();
        if (playerStats.CurrentSP < skill.spCost)
        {
            Debug.Log("Not enough SP to use " + skill.skillName);
            return;
        }
        playerStats.CurrentSP -= skill.spCost;
        foreach (var buff in skill.buffs)
        {
            StatusBuff refBuff = buff.Clone();
            skillBuffs.Add(refBuff);
        }
        EnemyCombat[] enemies = worldParent.GetComponentsInChildren<EnemyCombat>();
        List<EnemyCombat> enemyList = new List<EnemyCombat>(enemies);
        bool isCriticalHit = UnityEngine.Random.value < playerStats.CriticalChance;
        foreach (var enemy in enemyList)
        {
            float phycalDamage = skill.physicalDamageMultiplier + playerStats.PhysicalAttack;
            float fireDamage = playerStats.MagicAttack * playerStats.FireDamageMultiplier / 100;
            float coldDamage = playerStats.MagicAttack * playerStats.ColdDamageMultiplier / 100;
            float lightningDamage = playerStats.MagicAttack * playerStats.LightningDamageMultiplier / 100;
            if (isCriticalHit)
            {
                phycalDamage *= playerStats.CriticalDamageMultiplier;
                fireDamage *= playerStats.CriticalDamageMultiplier;
                coldDamage *= playerStats.CriticalDamageMultiplier;
                lightningDamage *= playerStats.CriticalDamageMultiplier;
            }
            foreach (var buff in skillBuffs)
            {
                bool isBuffHit = UnityEngine.Random.Range(0f, 100f) < playerStats.StatusHitChance * 100;
                bool isResisted = UnityEngine.Random.Range(0f, 100f) < playerStats.StatusEffectResistance * 100;
                if (isBuffHit && !isResisted)
                {
                    ApplyBuff(buff);
                    Debug.Log("Applied buff: " + buff.buffType + " to player.");
                }
            }
            skillBuffs.Clear();
            enemy.ApplyDamage(phycalDamage, fireDamage, coldDamage, lightningDamage);
        }
    }
    private void HandleSelfTargeting(Skill skill)
    {
        Heal(skill.healingAmountMultiplier * playerStats.MaxHealth);
        foreach (var buff in skill.buffs)
        {
            StatusBuff refBuff = buff.Clone();
            switch (buff.buffType)
            {
                case BuffType.HealOverTime:
                    Debug.Log("Applied HealOverTime buff to player.");
                    break;
                case BuffType.SpeedUp:
                    StatusBuff existingBuff = activeBuffs.Find(b => b.buffType == BuffType.SpeedUp);
                    if (existingBuff == null)
                    {
                        activeBuffs.Add(refBuff);
                        SpeedUpBuffEffect(refBuff);
                        Debug.Log("Applied SpeedUp buff to player.");
                        refBuff.isApplied = true;
                    }
                    else
                    {
                        existingBuff.ResetDuration(refBuff.duration);
                        Debug.Log("Refreshed SpeedUp buff duration to " + refBuff.duration);
                    }
                    break;
            }
        }

    }
    public void ApplyDamage(float PhysicalDamage, float FireDamage, float ColdDamage, float LightningDamage)
    {
        float physicalDamageAfterDefense = 100 / (100 + playerStats.PhysicalDefense) * PhysicalDamage;
        float fireDamageAfterResistance = Math.Max(0, FireDamage - (FireDamage * playerStats.FireResistance));
        float coldDamageAfterResistance = Math.Max(0, ColdDamage - (ColdDamage * playerStats.ColdResistance));
        float lightningDamageAfterResistance = Math.Max(0, LightningDamage - (LightningDamage * playerStats.LightningResistance));
        float totalDamage = physicalDamageAfterDefense + fireDamageAfterResistance + coldDamageAfterResistance + lightningDamageAfterResistance;
        totalDamage = Mathf.Max(totalDamage, 0); // Ensure damage is not negative
        playerStats.CurrentHealth -= totalDamage;
        Debug.Log("Player took " + totalDamage + " damage. Current Health: " + playerStats.CurrentHealth);
    }
    private void Heal(float healAmount)
    {
        playerStats.CurrentHealth += healAmount;
        if (playerStats.CurrentHealth > playerStats.MaxHealth)
        {
            playerStats.CurrentHealth = playerStats.MaxHealth;
        }
        Debug.Log("Player healed " + healAmount + " health. Current Health: " + playerStats.CurrentHealth);
    }
    public void OnEnemyClicked(EnemyCombat enemy)
    {
        if (playerState != PlayerState.Targeting || currentSkill == null) return;

        if (currentSkill.targetType == TargetType.SingleEnemy)
        {
            targetedEnemy = enemy;
            HighlightEnemies(false);
            currentSkill = null;
            SetState(PlayerState.Idle);
        }
        TurnManager.ChangingState(TurnState.SpeedCompareState);

    }
    private void HighlightEnemies(bool on)
    {
        EnemyCombat[] enemies = worldParent.GetComponentsInChildren<EnemyCombat>();
        List<EnemyCombat> enemyList = new List<EnemyCombat>(enemies);
        foreach (var enemy in enemyList)
        {
            enemy.SetHighlight(on);
        }
    }
    private void SetState(PlayerState newState)
    {
        playerState = newState;
    }

    private void UseSkillOnSingleEnemy(Skill skill, EnemyCombat enemy)
    {
        if (playerStats.CurrentSP < skill.spCost)
        {
            Debug.Log("Not enough SP to use " + skill.skillName);
            return;
        }
        playerStats.CurrentSP -= skill.spCost;
        foreach (var buff in skill.buffs)
        {
            StatusBuff refBuff = buff.Clone(); // Create a local copy to avoid closure issues
            skillBuffs.Add(refBuff);
        }

        float physicalDamage = skill.physicalDamageMultiplier + playerStats.PhysicalAttack;
        float fireDamage = playerStats.MagicAttack * playerStats.FireDamageMultiplier / 100;
        float coldDamage = playerStats.MagicAttack * playerStats.ColdDamageMultiplier / 100;
        float lightningDamage = playerStats.MagicAttack * playerStats.LightningDamageMultiplier / 100;

        bool isCriticalHit = UnityEngine.Random.value < playerStats.CriticalChance;
        if (isCriticalHit)
        {
            physicalDamage *= playerStats.CriticalDamageMultiplier;
            fireDamage *= playerStats.CriticalDamageMultiplier;
            coldDamage *= playerStats.CriticalDamageMultiplier;
            lightningDamage *= playerStats.CriticalDamageMultiplier;
        }
        ;
        foreach (var buff in skillBuffs)
        {
            bool isBuffHit = UnityEngine.Random.Range(0f, 100f) < playerStats.StatusHitChance * 100;
            bool isResisted = UnityEngine.Random.Range(0f, 100f) < enemy.GetEnemyStats().StatusEffectResistance * 100;
            if (isBuffHit && !isResisted)
            {
                enemy.ApplyBuff(buff);
                Debug.Log("Applied buff: " + buff.buffType + " to enemy: " + enemy.name);
            }
            else
            {
                Debug.Log("Buff: " + buff.buffType + " failed to apply to enemy: " + enemy.name);
            }
        }

        enemy.ApplyDamage(physicalDamage, fireDamage, coldDamage, lightningDamage);
    }
    public void ApplyBuff(StatusBuff buff)
    {
        StatusBuff existingBuff = activeBuffs.Find(b => b.buffType == buff.buffType);
        if (existingBuff != null)
        {
            existingBuff.ResetDuration(buff.duration);
            Debug.Log("Refreshed buff: " + activeBuffs.Find(b => b.buffType == buff.buffType).buffType + " duration to " + buff.duration);
        }
        if (buff.isStackable && existingBuff != null)
        {
            buff.Stack += 1;
            Debug.Log("Increased stack of buff: " + buff.buffType + " to " + buff.Stack);
            return;
        }
        activeBuffs.Add(buff);
    }
    public void ProcessBuffs()
    {
        List<StatusBuff> buffsToRemove = new List<StatusBuff>();
        foreach (var buff in activeBuffs)
        {
            Debug.Log("Processing buff: " + buff.buffType + " with duration " + buff.duration + " and stack " + buff.Stack);
            switch (buff.buffType)
            {
                case BuffType.Bleed:
                    BleedEffect(buff);
                    buff.duration -= 1;
                    if (buff.duration <= 0)
                    {
                        buffsToRemove.Add(buff);
                    }
                    break;
                case BuffType.SpeedUp:
                    if (!buff.isApplied)
                    {
                        SpeedUpBuffEffect(buff);
                        buff.isApplied = true;
                    }
                    buff.duration -= 1;
                    if (buff.duration <= 0)
                    {
                        buffsToRemove.Add(buff);
                    }
                    break;
            }
        }
        foreach (var buff in buffsToRemove)
        {
            activeBuffs.Remove(buff);
        }
    }
    private void BleedEffect(StatusBuff buff)
    {
        float physicalDamageAfterDefense = 100f / (100f + playerStats.PhysicalDefense) * buff.Stack * ((buff.PhysicalDamageMultiplierByMaxHealth * playerStats.MaxHealth) + buff.PhysicalDamageFlat);
        float fireDamageAfterResistance = Math.Max(0, buff.fireDamageMultiplierByMaxHealth * playerStats.MaxHealth + buff.FireDamageFlat - ((buff.fireDamageMultiplierByMaxHealth * playerStats.MaxHealth + buff.FireDamageFlat) * playerStats.FireResistance));
        float coldDamageAfterResistance = Math.Max(0, buff.iceDamageMultiplierByMaxHealth * playerStats.MaxHealth + buff.IceDamageFlat - ((buff.iceDamageMultiplierByMaxHealth * playerStats.MaxHealth + buff.IceDamageFlat) * playerStats.ColdResistance));
        float lightningDamageAfterResistance = Math.Max(0, buff.lightningDamageMultiplierByMaxHealth * playerStats.MaxHealth + buff.LightningDamageFlat - ((buff.lightningDamageMultiplierByMaxHealth * playerStats.MaxHealth + buff.LightningDamageFlat) * playerStats.LightningResistance));
        float totalDamage = physicalDamageAfterDefense + fireDamageAfterResistance + coldDamageAfterResistance + lightningDamageAfterResistance;
        totalDamage = Mathf.Max(totalDamage, 0); // Ensure damage is not negative
        playerStats.CurrentHealth -= totalDamage;
        Debug.Log("Player took " + totalDamage + " damage from Bleed. Current Health: " + playerStats.CurrentHealth);
    }
    private void SpeedUpBuffEffect(StatusBuff buff)
    {
        playerStats.ActionSpeed = (playerStats.ActionSpeed + buff.amountFlat) * (1 + buff.amountMultiplier);
        Debug.Log("Player speed increased by SpeedUp buff. New Action Speed: " + playerStats.ActionSpeed);

    }
}