using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SeasonalDamageEffect", menuName = "ScriptableObjects/SkillEffect/SeasonalDamageEffect")]
public class SeasonalDamageEffect : SkillEffect
{
    [Header("Base Damage")]
    public float BaseDamage = 500f;
    public DamageElement Element = DamageElement.Physical;
    [Range(0f, 1f)] public float Accuracy = 1f;

    [Header("Weather Requirements")]
    public WeatherType RequiredWeather;
    [Tooltip("Matching the Current Season grants +20% damage bonus automatically.")]
    public bool EnableMatchingBonus = true;

    [Header("Weather Bonuses (Current Season)")]
    public float WeatherBonusDamage = 0f;
    [Range(0f, 1f)] public float WeatherBonusPercent = 0f;

    [Header("Forecast (Next Season)")]
    public bool UseForecast = false;
    public WeatherType ForecastWeather;
    public float ForecastBonusDamage = 0f;
    public bool RepeatOnForecast = false;

    [Header("Momentum")]
    public bool UseMomentum = false;
    public string MomentumBuffName = "Momentum";
    public float MomentumBonusDamage = 0f;
    [Range(0f, 1f)] public float MomentumBonusPercent = 0f;

    [Header("Special Effects (Triggered if Current Season matches)")]
    public float HealCasterAmount = 0f;
    public bool StealBuffs = false;
    public Buff SideEffectDebuffTemplate;
    [Range(0f, 1f)] public float MaxHPReductionPercent = 0f;
    [Range(0f, 1f)] public float MomentumMaxHPReductionPercent = 0f;
    public Buff MaxHPDeBuffTemplate;
    [Range(0f, 1f)] public float SleepChance = 0f;
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
        if (Random.value > Accuracy)
        {
            target.ShowDamage(0, Color.white);
            return false;
        }

        // 2. Calculate Damage
        float totalDamage = BaseDamage;
        ActiveBuff momentumBuff = caster.buffController.GetBuffByName(MomentumBuffName);
        bool hasMomentum = momentumBuff != null && momentumBuff.isUsable && style == SkillStyle.CE;

        // Matching Season Bonus (+20%)
        if (EnableMatchingBonus && current == RequiredWeather)
        {
            totalDamage *= 1.2f;
        }

        // Specific Weather Bonus
        if (current == RequiredWeather)
        {
            totalDamage += WeatherBonusDamage;
            if (WeatherBonusPercent > 0) totalDamage *= (1f + WeatherBonusPercent);
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
            if (MomentumBonusPercent > 0) totalDamage *= (1f + MomentumBonusPercent);
        }

        // Variance
        float variance = Random.Range(0.85f, 1.15f);
        float finalDamage = totalDamage * variance;

        // Apply Damage
        DealDamage(caster, target, finalDamage, false, log, style);

        // 3. Side Effects (Only if Current Season matches)
        if (current == RequiredWeather)
        {
            // Apply Debuff
            if (SideEffectDebuffTemplate != null)
            {
                target.buffController.AddBuff(SideEffectDebuffTemplate);
                log.AddBuffEffectLog(new BuffEffectLog()
                {
                    AppliedTargetID = target.GetEntityID(),
                    AppliedTarget = target.Stats.EntityName,
                    Buff = new BuffEffectData(SideEffectDebuffTemplate)
                });
            }

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
                if (Random.value <= SleepChance)
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
            if (MaxHPReductionPercent > 0)
            {
                float reductionPercent = hasMomentum ? MomentumMaxHPReductionPercent : MaxHPReductionPercent;
                float targetMaxHP = target.GetStat(StatType.MaxHealth);
                target.GainCorruptedHealth(targetMaxHP * reductionPercent);
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
        Damage damage = new Damage(amount, Element, isCrit, 0.05f, 1.5f);
        DamageCtx ctx = new DamageCtx(caster, target, damage, style, log);
        DamageSystem.Process(ctx, log);
    }
}
