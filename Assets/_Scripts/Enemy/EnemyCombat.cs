using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    // Enemy Stats
    [SerializeField] private EntitiesBaseStat enemyBaseStats;
    [SerializeField] private List<Skill> enemySkills;
    private Stat enemyStats;
    //monobehavior lifecycle method
    private void Start()
    {
        // Initialize enemy stats from base stats
        enemyStats.MaxHealth = enemyBaseStats.MaxHealth;
        enemyStats.CurrentHealth = enemyBaseStats.MaxHealth;
        enemyStats.MaxSP = enemyBaseStats.MaxSkillPoint;
        enemyStats.CurrentSP = enemyBaseStats.MaxSkillPoint;
        enemyStats.PhysicalAttack = enemyBaseStats.PhysicalAttack;
        enemyStats.MagicAttack = enemyBaseStats.MagicAttack;
        enemyStats.PhysicalDefense = enemyBaseStats.PhysicalDefense;
        enemyStats.FireDamageMultiplier = enemyBaseStats.FireDamageMultiplier;
        enemyStats.ColdDamageMultiplier = enemyBaseStats.ColdDamageMultiplier;
        enemyStats.LightningDamageMultiplier = enemyBaseStats.LightningDamageMultiplier;
        enemyStats.FireResistance = enemyBaseStats.FireResistance;
        enemyStats.ColdResistance = enemyBaseStats.ColdResistance;
        enemyStats.LightningResistance = enemyBaseStats.LightningResistance;
        enemyStats.ActionSpeed = enemyBaseStats.ActionSpeed;
        enemyStats.CriticalChance = enemyBaseStats.CriticalHitChance;
        enemyStats.CriticalDamageMultiplier = enemyBaseStats.CriticalHitDamageMultiplier;
        enemyStats.Accuracy = enemyBaseStats.Accuracy;
        enemyStats.EvasionRate = enemyBaseStats.EvasionRate;
        enemyStats.StatusEffectResistance = enemyBaseStats.StatusEffectResistance;
    }

    public void ApplyDamage(float physicalDamage, float fireDamage, float coldDamage, float lightningDamage)
    {
        float physicalDamageAfterDefense = 100 / (100 + enemyStats.PhysicalDefense) * physicalDamage;
        float fireDamageAfterMultiplier = fireDamage * (1 + enemyStats.FireDamageMultiplier / 100);
        float coldDamageAfterMultiplier = coldDamage * (1 + enemyStats.ColdDamageMultiplier / 100);
        float lightningDamageAfterMultiplier = lightningDamage * (1 + enemyStats.LightningDamageMultiplier / 100);
        float totalDamage = physicalDamageAfterDefense + fireDamageAfterMultiplier + coldDamageAfterMultiplier + lightningDamageAfterMultiplier;
        enemyStats.CurrentHealth -= totalDamage;
        Debug.Log("Enemy took " + totalDamage + " damage. Current Health: " + enemyStats.CurrentHealth);
        if (enemyStats.CurrentHealth <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        // Handle enemy death (e.g., play animation, drop loot, etc.)
        Destroy(gameObject);
    }
    public void UseSkill(PlayerCombat player)
    {
        Skill skill = enemySkills[0]; // For simplicity, enemy uses the first skill
        if (enemyStats.CurrentSP < skill.spCost)
        {
            Debug.Log("Enemy does not have enough SP to use " + skill.skillName);
            return;
        }
        enemyStats.CurrentSP -= skill.spCost;
        switch (skill.targetType)
        {
            case TargetType.SingleEnemy:
                float physicalDamage = skill.physicalDamageMultiplier + enemyStats.PhysicalAttack;
                float fireDamage = skill.magicDamageMultiplier + enemyStats.FireDamageMultiplier;
                float coldDamage = skill.magicDamageMultiplier + enemyStats.ColdDamageMultiplier;
                float lightningDamage = skill.magicDamageMultiplier + enemyStats.LightningDamageMultiplier;
                player.ApplyDamage(physicalDamage, fireDamage, coldDamage, lightningDamage);
                break;
        }
    }
    public Stat GetEnemyStats()
    {
        return enemyStats;
    }

    public Skill RandomSkill()
    {
        int randomIndex = Random.Range(0, enemySkills.Count);
        return enemySkills[randomIndex];
    }
}