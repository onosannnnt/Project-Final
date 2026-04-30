using System;
using UnityEngine;

public static class DamageSystem
{
    public static event Action<Entity, Entity, CombatActionLog> OnEntityKilled;

    public static void Process(DamageCtx ctx, CombatActionLog log)
    {
        float healthBefore = ctx.Target.CurrentHealth;

        foreach (var mod in ctx.Caster.OutgoingModifiers)
            mod.Modify(ctx);

        foreach (var mod in ctx.Target.IncomingModifiers)
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
}
