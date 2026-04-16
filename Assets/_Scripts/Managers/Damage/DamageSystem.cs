using UnityEngine;

public static class DamageSystem
{
    public static void Process(DamageCtx ctx, CombatActionLog log)
    {
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

        log.AddDamageEffectLog(new DamageEffectLog()
        {
            AppliedTarget = ctx.Target.Stats.EntityName,
            AppliedTargetID = ctx.Target.GetEntityID(),
            Damage = new DamageEffectData(ctx.Damage)
        });
    }
}
