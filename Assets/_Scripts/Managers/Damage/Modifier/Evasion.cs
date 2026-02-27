using UnityEngine;

public class EvasionModifier : IDamageModifier
{
    public void Modify(DamageCtx ctx)
    {
        float evasionChance = ctx.Target.GetStat(StatType.EvasionRate) / (ctx.Caster.GetStat(StatType.Accuracy) + ctx.Target.GetStat(StatType.EvasionRate));

        if (Random.value < evasionChance)
        {
            Debug.Log(ctx.Target.gameObject.name + " evaded the attack from " + ctx.Caster.gameObject.name + "!");
            ctx.Damage.Amount = 0;
        }
    }
}