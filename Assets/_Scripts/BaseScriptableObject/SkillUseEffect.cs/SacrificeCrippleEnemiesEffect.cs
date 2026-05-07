using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SacrificeCrippleEnemiesEffect", menuName = "ScriptableObjects/SkillEffect/SacrificeCrippleEnemiesEffect")]
public class SacrificeCrippleEnemiesEffect : SkillEffect
{
    [Header("Self Sacrifice")]
    [Range(0f, 1f)]
    [Tooltip("Percent of max HP to sacrifice when casting this skill.")]
    public float HpSacrificePercent = 0.30f;

    [Header("Enemy Debuff")]
    public CrippleAndExposeDebuff DebuffTemplate;

    [Range(0f, 1f)]
    [Tooltip("Reduce enemy Action Speed by this percent.")]
    public float SpeedReductionPercent = 0.80f;

    [Tooltip("Enemies take this much extra flat damage while debuffed.")]
    public float ExtraDamageTaken = 200f;

    [Min(1)]
    [Tooltip("Debuff duration in turns.")]
    public int DurationTurns = 2;

    [System.NonSerialized] private CombatActionLog _lastProcessedLog = null;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (caster == null || target == null || target.buffController == null || DebuffTemplate == null)
        {
            return false;
        }

        ApplySelfSacrificeOncePerAction(caster, log);

        CrippleAndExposeDebuff runtimeDebuff = Instantiate(DebuffTemplate);
        runtimeDebuff.Duration = Mathf.Max(1, DurationTurns);
        runtimeDebuff.isPermanent = false;
        runtimeDebuff.buffType = BuffType.Debuff;
        runtimeDebuff.ExtraDamageTaken = ExtraDamageTaken;

        ApplySpeedReduction(runtimeDebuff, SpeedReductionPercent);

        target.buffController.AddBuff(runtimeDebuff);

        if (log != null)
        {
            log.AddBuffEffectLog(new BuffEffectLog()
            {
                AppliedTargetID = target.GetEntityID(),
                AppliedTarget = target.Stats != null ? target.Stats.EntityName : target.gameObject.name,
                Buff = new BuffEffectData(runtimeDebuff)
            });
        }

        return true;
    }

    private void ApplySelfSacrificeOncePerAction(Entity caster, CombatActionLog log)
    {
        if (log == null || log == _lastProcessedLog)
        {
            return;
        }
        _lastProcessedLog = log;

        int hpCost = Mathf.RoundToInt(caster.GetStat(StatType.MaxHealth) * Mathf.Clamp01(HpSacrificePercent));
        
        if (hpCost > 0)
        {
            // Use PreventDeath = true to ensure player doesn't suicide
            caster.TakeDamage(new Damage(hpCost, DamageElement.Physical, false, 0, 1.5f, true));
        }
    }

    private void ApplySpeedReduction(CrippleAndExposeDebuff debuff, float speedReductionPercent)
    {
        if (debuff.modifiers == null)
        {
            debuff.modifiers = new List<StatModifier>();
        }

        debuff.modifiers.RemoveAll(m => m != null && m.Stat == StatType.ActionSpeed && m.Type == ModifierType.Percent);

        float clampedReduction = Mathf.Clamp01(speedReductionPercent);
        debuff.modifiers.Add(new StatModifier()
        {
            Stat = StatType.ActionSpeed,
            Type = ModifierType.Percent,
            Value = -clampedReduction
        });
    }
}