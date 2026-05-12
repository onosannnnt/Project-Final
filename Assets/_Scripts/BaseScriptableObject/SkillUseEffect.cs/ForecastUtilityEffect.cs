using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ForecastUtilityEffect", menuName = "ScriptableObjects/SkillEffect/ForecastUtilityEffect")]
public class ForecastUtilityEffect : SkillEffect
{
    [Header("Healing")]
    [SerializeField] private float healAmount = 0f;
    [SerializeField] private bool healParty = false;

    [Header("Buffs / Debuffs")]
    [SerializeField] private List<Buff> buffsToApply;
    [SerializeField] private bool applyBuffsToParty = false;

    [Header("Utility")]
    [SerializeField] private bool cleanseDebuffs = false;
    [SerializeField] private int lockWeatherDuration = 0;

    [Header("Forecast Manipulation")]
    [SerializeField] private bool setNextSeasonOnUse = true;
    [SerializeField] private WeatherType nextSeasonToSet;

    public override void OnSkillStarted(Entity caster, List<Entity> targets, CombatActionLog log)
    {
        // 1. Perform Global / Party Effects FIRST
        if (ExecuteOnce)
        {
            ApplyGlobalEffects(caster, log);
        }

        // 2. Change the future season LAST
        if (WeatherManager.Instance != null && setNextSeasonOnUse)
        {
            WeatherManager.Instance.SetNextWeather(nextSeasonToSet);
        }
    }

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        if (target == null) return true;

        // 3. Perform Target Specific Effects
        if (!ExecuteOnce)
        {
            ApplyTargetEffects(caster, target, log);
        }

        return true;
    }

    private void ApplyGlobalEffects(Entity caster, CombatActionLog log)
    {
        // Lock Weather
        if (lockWeatherDuration > 0)
        {
            WeatherManager.Instance.LockWeather(lockWeatherDuration);
        }

        // Party Heal
        if (healParty && healAmount > 0)
        {
            List<Entity> allies = GetAliveAllies(caster);
            foreach (var ally in allies)
            {
                HealEntity(ally, healAmount, log);
            }
        }

        // Party Buffs
        if (applyBuffsToParty && buffsToApply != null)
        {
            List<Entity> allies = GetAliveAllies(caster);
            foreach (var ally in allies)
            {
                foreach (var buff in buffsToApply)
                {
                    AddBuff(ally, buff, log);
                }
            }
        }
    }

    private void ApplyTargetEffects(Entity caster, Entity target, CombatActionLog log)
    {
        // Target Heal
        if (!healParty && healAmount > 0)
        {
            HealEntity(target, healAmount, log);
        }

        // Target Buffs
        if (!applyBuffsToParty && buffsToApply != null)
        {
            foreach (var buff in buffsToApply)
            {
                AddBuff(target, buff, log);
            }
        }

        // Target Cleanse
        if (cleanseDebuffs)
        {
            List<ActiveBuff> debuffs = target.buffController.GetDebuffs();
            foreach (var debuff in debuffs)
            {
                target.buffController.RemoveBuff(debuff);
            }
        }
    }

    private void HealEntity(Entity target, float amount, CombatActionLog log)
    {
        target.Heal(amount);
        log.AddHealEffectLog(new HealEffectLog()
        {
            AppliedTarget = target.Stats.EntityName,
            AppliedTargetID = target.GetEntityID(),
            HealAmount = amount
        });
    }

    private void AddBuff(Entity target, Buff buff, CombatActionLog log)
    {
        target.buffController.AddBuff(buff);
        log.AddBuffEffectLog(new BuffEffectLog()
        {
            AppliedTargetID = target.GetEntityID(),
            AppliedTarget = target.Stats.EntityName,
            Buff = new BuffEffectData(buff)
        });
    }

    private List<Entity> GetAliveAllies(Entity caster)
    {
        if (caster is PlayerEntity)
        {
            return PlayerTeamManager.Instance != null ? PlayerTeamManager.Instance.GetAliveMembers().ConvertAll(p => (Entity)p) : new List<Entity> { caster };
        }
        return new List<Entity> { caster };
    }
}
