using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ComprehensiveCorruptedStrikeEffect", menuName = "ScriptableObjects/SkillEffect/ComprehensiveCorruptedStrikeEffect")]
public class ComprehensiveCorruptedStrikeEffect : SkillEffect
{
    [Header("Base Attack")]
    public float BaseDamage = 500f;
    public DamageElement Element = DamageElement.Physical;
    public float Accuracy = 100f;

    [Header("Corrupted Health Scaling")]
    [Tooltip("Bonus damage added for every 10% of Max HP that is Corrupted.")]
    public float DamagePer10PercentCorrupted = 100f;

    [Header("HP Costs & Penalties")]
    [Tooltip("Lose this % of CURRENT HP on use.")]
    public float CurrentHpCostPercent = 0f;
    [Tooltip("Sets CURRENT HP to this % of Max HP on use (Extreme Risk).")]
    public float ForceHpToMaxPercent = -1f; 

    [Header("Threshold Bonuses")]
    [Tooltip("Requirement: Corrupted Health >= X% of Max HP.")]
    public float CorruptedThresholdPercent = 0.3f;
    public float BonusDamageOnThreshold = 0f;
    public float DamageMultiplierOnThreshold = 1f;
    [Tooltip("If threshold met, gain this % of Max HP as Corrupted Health.")]
    public float CorruptedGainPercentOnThreshold = 0f;

    [Header("Self Buffs/Debuffs")]
    public List<Buff> SelfBuffsOnUse;
    public bool StunSelfOnThreshold = false;
    public Buff StunBuffTemplate;

    public override bool IsElementalAttackEffect => true;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (caster == null || target == null) return false;

        float maxHP = caster.GetStat(StatType.MaxHealth);
        float corruptedRatio = caster.CorruptedHealth / maxHP;
        bool thresholdMet = corruptedRatio >= CorruptedThresholdPercent;

        // 1. Accuracy Check
        if (Random.Range(0f, 100f) > Accuracy)
        {
            target.ShowDamage(0, Color.white);
            return false;
        }

        // 2. Calculate Damage
        float finalDamage = BaseDamage;
        
        // Per 10% Scaling
        int chunksOf10Percent = Mathf.FloorToInt(corruptedRatio * 10f);
        finalDamage += (chunksOf10Percent * DamagePer10PercentCorrupted);

        // Threshold Bonus
        if (thresholdMet)
        {
            finalDamage += BonusDamageOnThreshold;
            finalDamage *= DamageMultiplierOnThreshold;
        }

        // Variance
        float variance = Random.Range(0.85f, 1.15f);
        finalDamage *= variance;

        // Apply Damage
        Damage dmg = new Damage(finalDamage, Element, false, 5f, 1.5f);
        DamageCtx ctx = new DamageCtx(caster, target, dmg);
        DamageSystem.Process(ctx, log);

        // 3. Costs & Self-Effects
        ApplySelfEffects(caster, thresholdMet, log);

        return true;
    }

    [System.NonSerialized] private CombatActionLog lastProcessedLog = null;

    private void ApplySelfEffects(Entity caster, bool thresholdMet, CombatActionLog log)
    {
        if (log == null || log == lastProcessedLog) return;
        lastProcessedLog = log;

        float maxHP = caster.GetStat(StatType.MaxHealth);

        // HP Costs
        if (CurrentHpCostPercent > 0)
        {
            float cost = caster.CurrentHealth * CurrentHpCostPercent;
            // PreventDeath = true to ensure player doesn't suicide
            caster.TakeDamage(new Damage(cost, DamageElement.None, false, 0, 1.5f, true));
        }

        if (ForceHpToMaxPercent >= 0)
        {
            float targetHP = maxHP * ForceHpToMaxPercent;
            float currentHP = caster.CurrentHealth;
            if (currentHP > targetHP)
            {
                // PreventDeath = true to ensure player doesn't suicide
                caster.TakeDamage(new Damage(currentHP - targetHP, DamageElement.None, false, 0, 1.5f, true));
            }
        }

        // Corrupted Health Gain
        if (thresholdMet && CorruptedGainPercentOnThreshold > 0)
        {
            caster.GainCorruptedHealth(maxHP * CorruptedGainPercentOnThreshold);
        }

        // Stun Self
        if (thresholdMet && StunSelfOnThreshold && StunBuffTemplate != null)
        {
            caster.buffController.AddBuff(StunBuffTemplate);
            log.AddBuffEffectLog(new BuffEffectLog()
            {
                AppliedTargetID = caster.GetEntityID(),
                AppliedTarget = caster.Stats.EntityName,
                Buff = new BuffEffectData(StunBuffTemplate)
            });
        }

        // Self Buffs
        if (SelfBuffsOnUse != null)
        {
            foreach (var buff in SelfBuffsOnUse)
            {
                caster.buffController.AddBuff(buff);
                log.AddBuffEffectLog(new BuffEffectLog()
                {
                    AppliedTargetID = caster.GetEntityID(),
                    AppliedTarget = caster.Stats.EntityName,
                    Buff = new BuffEffectData(buff)
                });
            }
        }
    }
}
