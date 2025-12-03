using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    // Enemy Stats
    [SerializeField] private EntitiesBaseStat enemyBaseStats;
    [SerializeField] private List<Skill> enemySkills;
    [SerializeField] private PlayerCombat playerCombat;
    //UI
    [SerializeField] private GameObject HealthBar;

    private Stat enemyStats;
    private List<StatusBuff> activeBuffs = new List<StatusBuff>();
    private List<StatusBuff> skillBuffs = new List<StatusBuff>();
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
        enemyStats.StatusHitChance = enemyBaseStats.StatusHitChance;
        SetHealthBar();

    }

    public void ApplyDamage(float physicalDamage, float fireDamage, float coldDamage, float lightningDamage)
    {
        float physicalDamageAfterDefense = 100 / (100 + enemyStats.PhysicalDefense) * physicalDamage;
        float fireDamageAfterMultiplier = fireDamage * (1 + enemyStats.FireDamageMultiplier / 100);
        float coldDamageAfterMultiplier = coldDamage * (1 + enemyStats.ColdDamageMultiplier / 100);
        float lightningDamageAfterMultiplier = lightningDamage * (1 + enemyStats.LightningDamageMultiplier / 100);
        float totalDamage = physicalDamageAfterDefense + fireDamageAfterMultiplier + coldDamageAfterMultiplier + lightningDamageAfterMultiplier;
        enemyStats.CurrentHealth -= totalDamage;
        SetHealthBar();
        Debug.Log("Enemy took " + totalDamage + " damage. Current Health: " + enemyStats.CurrentHealth);
        if (enemyStats.CurrentHealth <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        Destroy(gameObject);
    }
    public void UseSkill(PlayerCombat player)
    {
        Skill skill = enemySkills[0]; // For simplicity, enemy uses the first skill
        foreach (var buff in skill.buffs)
        {
            StatusBuff refBuff = buff.Clone();
            skillBuffs.Add(refBuff);
        }
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
                foreach (var buff in skillBuffs)
                {
                    bool isBuffHit = UnityEngine.Random.Range(0f, 100f) < enemyStats.StatusHitChance * 100;
                    bool isResisted = UnityEngine.Random.Range(0f, 100f) < player.GetPlayerStats().StatusEffectResistance * 100;
                    if (isBuffHit && !isResisted)
                    {
                        player.ApplyBuff(buff);
                        Debug.Log("Enemy applied buff: " + buff.buffType + " to Player");
                    }
                    else
                    {
                        Debug.Log("Enemy's buff: " + buff.buffType + " failed to apply to Player");
                    }
                }
                skillBuffs.Clear();
                break;
        }
    }
    public Stat GetEnemyStats()
    {
        return enemyStats;
    }

    public Skill RandomSkill()
    {
        enemyStats.ActionSpeed = UnityEngine.Random.Range(1, 60);
        int randomIndex = UnityEngine.Random.Range(0, enemySkills.Count);
        return enemySkills[randomIndex];
    }

    public void SetHighlight(bool on)
    {
        GameObject EnemyVisual = transform.Find("EnemyVisual").gameObject;
        SpriteRenderer spriteRenderer = EnemyVisual.GetComponent<SpriteRenderer>();
        if (on)
        {
            spriteRenderer.color = Color.red;
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }

    private void OnMouseDown()
    {
        PlayerCombat.Instance.OnEnemyClicked(this);
    }
    public void ApplyBuff(StatusBuff buff)
    {
        StatusBuff existingBuff = activeBuffs.Find(b => b.buffType == buff.buffType);
        if (existingBuff != null)
        {
            existingBuff.ResetDuration(buff.duration);
            Debug.Log("Refreshed buff: " + activeBuffs.Find(b => b.buffType == buff.buffType).buffType + " duration to " + buff.duration);
        }
        Debug.Log("is stackable: " + buff.isStackable);
        if (buff.isStackable && existingBuff != null)
        {
            buff.Stack += 1;
            Debug.Log("Increased stack of buff: " + buff.buffType + " to " + buff.Stack);
            return;
        }
        activeBuffs.Add(buff);
        Debug.Log("Applied buff with duration: " + buff.duration);
    }
    public void ProcessBuffs()
    {
        List<StatusBuff> buffsToRemove = new List<StatusBuff>();
        foreach (var buff in activeBuffs)
        {
            switch (buff.buffType)
            {
                case BuffType.Bleed:
                    BleedEffect(buff);
                    buff.duration -= 1f;
                    if (buff.duration <= 0)
                    {
                        buffsToRemove.Add(buff);
                    }
                    break;
                    // Add other buff types as needed
            }
        }
        foreach (var buff in buffsToRemove)
        {
            activeBuffs.Remove(buff);
        }
    }
    private void BleedEffect(StatusBuff buff)
    {
        float physicalDamageAfterDefense = 100 / (100 + enemyStats.PhysicalDefense) * buff.Stack * ((buff.PhysicalDamageMultiplierByMaxHealth * enemyStats.MaxHealth) + buff.PhysicalDamageFlat);
        float fireDamageAfterResistance = Math.Max(0, buff.fireDamageMultiplierByMaxHealth * enemyStats.MaxHealth + buff.FireDamageFlat - ((buff.fireDamageMultiplierByMaxHealth * enemyStats.MaxHealth + buff.FireDamageFlat) * enemyStats.FireResistance));
        float coldDamageAfterResistance = Math.Max(0, buff.iceDamageMultiplierByMaxHealth * enemyStats.MaxHealth + buff.IceDamageFlat - ((buff.iceDamageMultiplierByMaxHealth * enemyStats.MaxHealth + buff.IceDamageFlat) * enemyStats.ColdResistance));
        float lightningDamageAfterResistance = Math.Max(0, buff.lightningDamageMultiplierByMaxHealth * enemyStats.MaxHealth + buff.LightningDamageFlat - ((buff.lightningDamageMultiplierByMaxHealth * enemyStats.MaxHealth + buff.LightningDamageFlat) * enemyStats.LightningResistance));
        float totalDamage = physicalDamageAfterDefense + fireDamageAfterResistance + coldDamageAfterResistance + lightningDamageAfterResistance;
        totalDamage = Mathf.Max(totalDamage, 0); // Ensure damage is not negative
        enemyStats.CurrentHealth -= totalDamage;
        SetHealthBar();
        Debug.Log("Enemy took " + totalDamage + " damage from Bleed. Current Health: " + enemyStats.CurrentHealth);
        if (enemyStats.CurrentHealth <= 0)
        {
            Die();
        }
    }
    public List<StatusBuff> GetAppliedBuffs()
    {
        return activeBuffs;
    }
    private void SetHealthBar()
    {
        Debug.Log("Setting Health Bar: " + enemyStats.CurrentHealth / enemyStats.MaxHealth);
        var healthBar = HealthBar.GetComponent<UnityEngine.UI.Image>();
        if (healthBar != null)
        {
            healthBar.fillAmount = enemyStats.CurrentHealth / enemyStats.MaxHealth;
        }
    }
}