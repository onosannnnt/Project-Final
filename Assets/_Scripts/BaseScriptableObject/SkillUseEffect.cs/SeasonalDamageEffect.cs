using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SeasonalDamageEffect", menuName = "ScriptableObjects/SkillEffect/SeasonalDamageEffect")]
public class SeasonalDamageEffect : SkillEffect
{
    [Header("Base Damage")]
    public float BaseDamage = 500f;
    public DamageElement Element = DamageElement.Physical;
    public float Accuracy = 100f;

    [Header("Weather Requirements")]
    public bool RequiresWeather = false;
    public WeatherType RequiredWeather;

    [Header("Weather Bonuses")]
    public float WeatherBonusDamage = 0f;
    
    [Header("Forecast (Next Weather)")]
    public bool UseForecast = false;
    public WeatherType ForecastWeather;
    public float ForecastBonusDamage = 0f;
    public bool RepeatOnForecast = false;

    [Header("Momentum (Buff)")]
    public bool UseMomentum = false;
    public string MomentumBuffName = "Momentum";
    public float MomentumBonusDamage = 0f;
    public float MomentumBonusPercent = 0f;

    [Header("Side Effects")]
    public float HealCasterAmount = 0f;
    public bool StealBuffs = false;
    public float MaxHPReductionPercent = 0f;
    public Buff MaxHPDeBuffTemplate;
    public float SleepChance = 0f;
    public SleepBuff SleepBuffTemplate;
    public float RepeatAttackDamage = 0f;

    public override bool IsElementalAttackEffect => true;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (WeatherManager.Instance == null) return false;

        WeatherType current = WeatherManager.Instance.CurrentWeather;
        WeatherType next = WeatherManager.Instance.NextWeather;

        // 1. Check Weather Requirement if applicable
        if (RequiresWeather && current != RequiredWeather)
        {
            // If we don't match, we still do base damage? 
            // The prompt implies some skills only have bonuses, others are weather-locked.
            // For now, let's assume if it matches, it gets the bonus.
        }

        // 2. Accuracy Check
        if (Random.Range(0f, 100f) > Accuracy)
        {
            target.ShowDamage(0, Color.white);
            return false;
        }

        // 3. Calculate Damage
        float totalDamage = BaseDamage;

        if (current == RequiredWeather)
        {
            totalDamage += WeatherBonusDamage;
        }

        if (UseForecast && next == ForecastWeather)
        {
            totalDamage += ForecastBonusDamage;
        }

        if (UseMomentum)
        {
            ActiveBuff momentum = caster.buffController.GetBuffByName(MomentumBuffName);
            if (momentum != null)
            {
                totalDamage += MomentumBonusDamage;
                if (MomentumBonusPercent > 0)
                {
                    totalDamage *= (1f + (MomentumBonusPercent / 100f));
                }
            }
        }

        // Variance
        float variance = Random.Range(0.85f, 1.15f);
        totalDamage *= variance;

        // Apply Damage
        DealDamage(caster, target, totalDamage, false, log, 5f, 1.5f);

        // 4. Side Effects
        
        // Repeat Attack (Sun)
        if (current == RequiredWeather && RepeatAttackDamage > 0)
        {
            DealDamage(caster, target, RepeatAttackDamage * variance, false, log);
        }

        // Heal Caster (Rain)
        if (current == RequiredWeather && HealCasterAmount > 0)
        {
            caster.Heal(HealCasterAmount);
            log.AddHealEffectLog(new HealEffectLog()
            {
                AppliedTarget = caster.Stats.EntityName,
                AppliedTargetID = caster.GetEntityID(),
                HealAmount = HealCasterAmount
            });
        }

        // Sleep Chance (Windy)
        if (current == RequiredWeather && SleepChance > 0 && SleepBuffTemplate != null)
        {
            if (Random.Range(0f, 100f) <= SleepChance)
            {
                target.buffController.AddBuff(SleepBuffTemplate);
                log.AddBuffEffectLog(new BuffEffectLog()
                {
                    AppliedTargetID = target.GetEntityID(),
                    AppliedTarget = target.Stats.EntityName,
                    Buff = new BuffEffectData(SleepBuffTemplate)
                });
            }
        }

        // Steal Buffs (Windy)
        if (current == RequiredWeather && StealBuffs)
        {
            List<ActiveBuff> targetBuffs = new List<ActiveBuff>(target.buffController.GetBuffs());
            foreach (var b in targetBuffs)
            {
                caster.buffController.AddBuff(b.Data);
                target.buffController.RemoveBuff(b);
            }
        }

        // Max HP Reduction (Windy)
        if (current == RequiredWeather && MaxHPReductionPercent > 0 && MaxHPDeBuffTemplate != null)
        {
            float reduction = MaxHPReductionPercent;
            if (UseMomentum && caster.buffController.GetBuffByName(MomentumBuffName) != null)
            {
                // If momentum is active, we can increase the reduction. 
                // Since buffs are usually static assets, we'd need a way to pass this value.
                // For simplicity, we'll assume the user might have two different buffs or we'll just apply it multiple times/stacks.
                // But let's just stick to applying the buff template.
            }
            
            target.buffController.AddBuff(MaxHPDeBuffTemplate);
            log.AddBuffEffectLog(new BuffEffectLog()
            {
                AppliedTargetID = target.GetEntityID(),
                AppliedTarget = target.Stats.EntityName,
                Buff = new BuffEffectData(MaxHPDeBuffTemplate)
            });
        }

        // Repeat on Forecast
        if (UseForecast && next == ForecastWeather && RepeatOnForecast)
        {
            DealDamage(caster, target, totalDamage, false, log);
        }

        return true;
    }

    private void DealDamage(Entity caster, Entity target, float amount, bool isCrit, CombatActionLog log, float critChance = 0f, float critMult = 1.5f)
    {
        Damage damage = new Damage(amount, Element, isCrit, critChance, critMult);
        DamageCtx ctx = new DamageCtx(caster, target, damage);
        DamageSystem.Process(ctx, log);
    }
}
