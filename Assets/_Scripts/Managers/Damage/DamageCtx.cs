using System.Collections.Generic;

public class DamageCtx
{
    public Entity Caster { get; }
    public Entity Target { get; }
    public Damage Damage { get; set; }
    public SkillStyle Style { get; set; }
    public bool IsCriticalHit { get; set; }
    public CombatActionLog Log { get; set; } // Added for modifiers to use

    public DamageCtx(Entity caster, Entity target, Damage damage, SkillStyle style = SkillStyle.None, CombatActionLog log = null)
    {
        Caster = caster;
        Target = target;
        Damage = damage;
        Style = style;
        Log = log;
    }

}
