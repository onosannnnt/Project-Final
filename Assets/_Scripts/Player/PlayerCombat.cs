using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    // Player Stats
    [SerializeField] private EntitiesBaseStat playerBaseStats;
    private Stat playerStats;
    //skills
    [SerializeField] private List<Skill> playerSkills;
    //world
    [SerializeField] private Transform worldParent;
    [SerializeField] private TurnManager TurnManager;

    //monobehavior lifecycle method
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
                HandleSingleEnemyTargeting(skill);
                break;
            case TargetType.AllEnemies:
                HandleAllEnemiesTargeting(skill);
                break;
            case TargetType.Self:
                HandleSelfTargeting(skill);
                break;
        }
    }
    private void HandleSingleEnemyTargeting(Skill skill)
    {

    }

    private void HandleAllEnemiesTargeting(Skill skill)
    {
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
            enemy.ApplyDamage(phycalDamage, fireDamage, coldDamage, lightningDamage);
        }
    }
    private void HandleSelfTargeting(Skill skill)
    {
        Heal(skill.healingAmountMultiplier * playerStats.MaxHealth);
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
}