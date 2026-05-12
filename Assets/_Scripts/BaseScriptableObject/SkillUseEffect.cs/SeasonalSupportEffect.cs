using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SeasonalSupportEffect", menuName = "ScriptableObjects/SkillEffect/SeasonalSupportEffect")]
public class SeasonalSupportEffect : SkillEffect
{
    [Header("Damage")]
    public float BaseDamage = 350f;
    public DamageElement Element = DamageElement.Physical;

    [Header("Weather Requirements")]
    public WeatherType RequiredWeather;

    [Header("Rain Support (Triggered if Current Season matches)")]
    public float HealPartyAmount = 400f;
    public float ReviveHpPercent = 0.2f;

    [Header("Forecast (Next Season)")]
    public WeatherType ForecastWeather;
    public float ForecastBonusHeal = 200f;

    public override bool IsElementalAttackEffect => true;

    public override void OnSkillStarted(Entity caster, List<Entity> targets, CombatActionLog log)
    {
        if (WeatherManager.Instance == null) return;

        WeatherType current = WeatherManager.Instance.CurrentWeather;
        WeatherType next = WeatherManager.Instance.NextWeather;

        // Calculate damage with 20% matching bonus
        float finalDamage = BaseDamage;
        if (current == RequiredWeather)
        {
            finalDamage *= 1.2f; // +20% Damage for matching season
        }

        // Apply Damage to all targets in list
        foreach (var target in targets)
        {
            DealDamage(caster, target, finalDamage, log, SkillStyle.None);
        }

        // Apply Support if matching current season
        if (current == RequiredWeather)
        {
            float totalHeal = HealPartyAmount;
            if (next == ForecastWeather) totalHeal += ForecastBonusHeal;

            // 1. Try to Revive a dead ally first
            Entity reviveTarget = FindDeadAlly(caster);
            if (reviveTarget != null)
            {
                ReviveAlly(reviveTarget, ReviveHpPercent, log);
            }
            else
            {
                // 2. Otherwise heal the whole party
                HealParty(caster, totalHeal, log);
            }
        }
    }

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        // Damage is handled in OnSkillStarted for AOE consistency,
        // but since Skill.Execute loops, we return true here.
        return true;
    }

    private void DealDamage(Entity caster, Entity target, float amount, CombatActionLog log, SkillStyle style)
    {
        Damage damage = new Damage(amount, Element, false, 5f, 1.5f);
        DamageCtx ctx = new DamageCtx(caster, target, damage, style, log);
        DamageSystem.Process(ctx, log);
    }

    private Entity FindDeadAlly(Entity caster)
    {
        if (caster is PlayerEntity)
        {
            foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
            {
                if (member != null && member.CurrentHealth <= 0) return member;
            }
        }
        return null;
    }

    private void ReviveAlly(Entity target, float hpPercent, CombatActionLog log)
    {
        float healAmount = target.GetStat(StatType.MaxHealth) * hpPercent;
        target.gameObject.SetActive(true);
        target.Heal(healAmount);
        log.AddHealEffectLog(new HealEffectLog() { AppliedTarget = target.Stats.EntityName, AppliedTargetID = target.GetEntityID(), HealAmount = healAmount });
    }

    private void HealParty(Entity caster, float amount, CombatActionLog log)
    {
        List<Entity> allies = (caster is PlayerEntity) ?
            PlayerTeamManager.Instance.GetAliveMembers().ConvertAll(p => (Entity)p) :
            new List<Entity>{caster};

        foreach (var ally in allies)
        {
            ally.Heal(amount);
            log.AddHealEffectLog(new HealEffectLog() { AppliedTarget = ally.Stats.EntityName, AppliedTargetID = ally.GetEntityID(), HealAmount = amount });
        }
    }
}
