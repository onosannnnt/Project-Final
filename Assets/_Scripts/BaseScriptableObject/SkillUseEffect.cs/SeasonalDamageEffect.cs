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
    public WeatherType RequiredWeather;
    [Tooltip("Matching the Current Season grants +20% damage bonus automatically.")]
    public bool EnableMatchingBonus = true;

    [Header("Weather Bonuses (Current Season)")]
    public float WeatherBonusDamage = 0f;
    public float WeatherBonusPercent = 0f;

    [Header("Forecast (Next Season)")]
    public bool UseForecast = false;
    public WeatherType ForecastWeather;
    public float ForecastBonusDamage = 0f;
    public bool RepeatOnForecast = false;

    [Header("Momentum")]
    public bool UseMomentum = false;
    public string MomentumBuffName = "Momentum";
    public float MomentumBonusDamage = 0f;
    public float MomentumBonusPercent = 0f;

    [Header("Special Effects (Triggered if Current Season matches)")]
    public float HealCasterAmount = 0f;
    public bool StealBuffs = false;
    public float MaxHPReductionPercent = 0f;
    public float MomentumMaxHPReductionPercent = 0f;
    public Buff MaxHPDeBuffTemplate;
    public float SleepChance = 0f;
    public Buff SleepBuffTemplate;
    public float RepeatAttackDamage = 0f;

    public override bool IsElementalAttackEffect => true;

    public override void OnSkillStarted(Entity caster, List<Entity> targets, CombatActionLog log)
    {
        // We could pre-calculate damage here, but since it might vary per target (if we add target-specific logic),
        // we'll do the logic in Execute for now.
    }

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        if (WeatherManager.Instance == null || target == null) return false;

        WeatherType current = WeatherManager.Instance.CurrentWeather;
        WeatherType next = WeatherManager.Instance.NextWeather;

        // 1. Accuracy Check
        if (Random.Range(0f, 100f) > Accuracy)
        {
            target.ShowDamage(0, Color.white);
            return false;
        }

        // 2. Calculate Damage
        float totalDamage = BaseDamage;
        bool hasMomentum = caster.buffController.GetBuffByName(MomentumBuffName) != null && style == SkillStyle.CE;

        // Matching Season Bonus (+20%)
        if (EnableMatchingBonus && current == RequiredWeather)
        {
            totalDamage *= 1.2f;
        }

        // Specific Weather Bonus
        if (current == RequiredWeather)
        {
            totalDamage += WeatherBonusDamage;
            if (WeatherBonusPercent > 0) totalDamage *= (1f + (WeatherBonusPercent / 100f));
        }

        // Forecast Bonus
        if (UseForecast && next == ForecastWeather)
        {
            totalDamage += ForecastBonusDamage;
        }

        // Momentum Damage Bonus
        if (UseMomentum && hasMomentum)
        {
            // Note: The global +40% is already handled by MomentumBuff's IDamageModifier.
            // This section handles skill-specific bonuses (like flat damage additions).
            totalDamage += MomentumBonusDamage;
            if (MomentumBonusPercent > 0) totalDamage *= (1f + (MomentumBonusPercent / 100f));
        }

        // Variance
        float variance = Random.Range(0.85f, 1.15f);
        float finalDamage = totalDamage * variance;

        // Apply Damage
        DealDamage(caster, target, finalDamage, false, log, style);

        // 3. Side Effects (Only if Current Season matches)
        if (current == RequiredWeather)
        {
            // Repeat Attack (Sun)
            if (RepeatAttackDamage > 0)
            {
                DealDamage(caster, target, RepeatAttackDamage * variance, false, log, style);
            }

            // Heal Caster (Rain)
            if (HealCasterAmount > 0)
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
            if (SleepChance > 0 && SleepBuffTemplate != null)
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
            if (StealBuffs)
            {
                List<ActiveBuff> targetBuffs = new List<ActiveBuff>(target.buffController.GetBuffs());
                foreach (var b in targetBuffs)
                {
                    // Protection: Only steal buffs that are explicitly marked as stealable
                    if (b.Data.buffType == BuffType.Buff && b.Data.isStealable)
                    {
                        caster.buffController.AddBuff(b.Data);
                        target.buffController.RemoveBuff(b);
                    }
                }
            }

            // Max HP Reduction (Windy)
            if (MaxHPReductionPercent > 0 && MaxHPDeBuffTemplate != null)
            {
                float reduction = hasMomentum ? MomentumMaxHPReductionPercent : MaxHPReductionPercent;
                // Since our system usually uses fixed modifiers in Buff assets, 
                // for "Dynamic" reduction we'd need a way to pass this.
                // For now, we'll assume the provided MaxHPDeBuffTemplate handles it, 
                // or we apply it multiple times.
                target.buffController.AddBuff(MaxHPDeBuffTemplate);
                log.AddBuffEffectLog(new BuffEffectLog()
                {
                    AppliedTargetID = target.GetEntityID(),
                    AppliedTarget = target.Stats.EntityName,
                    Buff = new BuffEffectData(MaxHPDeBuffTemplate)
                });
            }
        }

        // Repeat on Forecast
        if (UseForecast && next == ForecastWeather && RepeatOnForecast)
        {
            DealDamage(caster, target, finalDamage, false, log, style);
        }

        return true;
    }

    private void DealDamage(Entity caster, Entity target, float amount, bool isCrit, CombatActionLog log, SkillStyle style)
    {
        Damage damage = new Damage(amount, Element, isCrit, 5f, 1.5f);
        DamageCtx ctx = new DamageCtx(caster, target, damage, style, log);
        DamageSystem.Process(ctx, log);
    }
}
