using System;
using System.Collections.Generic;
using UnityEngine;

public static class DamageSystem
{
    public static event Action<Entity, Entity, CombatActionLog> OnEntityKilled;

    public static void Process(DamageCtx ctx, CombatActionLog log)
    {
        ctx.Log = log; // Ensure modifiers can access the log
        float healthBefore = ctx.Target.CurrentHealth;

        // Perform Critical Roll if not already determined
        if (!ctx.Damage.IsCriticalHit && ctx.Damage.CritChance > 0)
        {
            if (UnityEngine.Random.value <= ctx.Damage.CritChance)
            {
                ctx.Damage.IsCriticalHit = true;
                ctx.Damage.Amount *= ctx.Damage.CritMultiplier;
            }
        }

        var outgoing = new List<IDamageModifier>(ctx.Caster.OutgoingModifiers);
        foreach (var mod in outgoing)
            mod.Modify(ctx);

        var incoming = new List<IDamageModifier>(ctx.Target.IncomingModifiers);
        foreach (var mod in incoming)
            mod.Modify(ctx);

        ctx.Damage.Amount = Mathf.Max(ctx.Damage.Amount, 0);
        ctx.Target.TakeDamage(ctx.Damage);

        if (ctx.Target is EnemyCombat enemyTarget)
        {
            enemyTarget.ApplyElementalBreakOnHitEffects(ctx.Damage.Amount);
        }

        bool didKillTarget = healthBefore > 0f && ctx.Target.CurrentHealth <= 0f;
        if (didKillTarget)
        {
            OnEntityKilled?.Invoke(ctx.Caster, ctx.Target, log);
        }

        log.AddDamageEffectLog(new DamageEffectLog()
        {
            AppliedTarget = ctx.Target.Stats.EntityName,
            AppliedTargetID = ctx.Target.GetEntityID(),
            Damage = new DamageEffectData(ctx.Damage)
        });
    }

    /// <summary>
    /// Specialized process for reflected damage to ensure it is logged correctly.
    /// </summary>
    public static void ProcessReflect(Entity caster, Entity target, Damage damage, CombatActionLog log)
    {
        if (target == null || target.CurrentHealth <= 0) return;

        float healthBefore = target.CurrentHealth;
        target.TakeDamage(damage);

        bool didKillTarget = healthBefore > 0f && target.CurrentHealth <= 0f;
        if (didKillTarget)
        {
            OnEntityKilled?.Invoke(caster, target, log);
        }

        if (log != null)
        {
            log.AddDamageEffectLog(new DamageEffectLog()
            {
                AppliedTarget = target.Stats.EntityName,
                AppliedTargetID = target.GetEntityID(),
                Damage = new DamageEffectData(damage)
            });
        }
    }
}
