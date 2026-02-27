using UnityEngine;

public static class DamageSystem
{
    public static void Process(DamageCtx ctx)
    {
        foreach (var mod in ctx.Caster.OutgoingModifiers)
            mod.Modify(ctx);

        foreach (var mod in ctx.Target.IncomingModifiers)
            mod.Modify(ctx);

        ctx.Damage.Amount = Mathf.Max(ctx.Damage.Amount, 0);
        ctx.Target.TakeDamage(ctx.Damage);
    }
}