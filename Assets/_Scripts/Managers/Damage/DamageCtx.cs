using System.Collections.Generic;

public class DamageCtx
{
    public Entity Caster { get; }
    public Entity Target { get; }
    public Damage Damage { get; set; }
    public bool IsCriticalHit { get; set; }

    public DamageCtx(Entity caster, Entity target, Damage damage)
    {
        Caster = caster;
        Target = target;
        Damage = damage;
    }

}