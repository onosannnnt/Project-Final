using UnityEngine;

[CreateAssetMenu(fileName = "LifestealEffect", menuName = "ScriptableObjects/SkillEffect/LifestealEffect")]
public class LifestealEffect : SkillEffect
{
    [SerializeField] private float baseDamage = 100f;
    [SerializeField] private DamageElement Element = DamageElement.Physical;
    [SerializeField, Range(0f, 1f)] private float lifestealPercent = 0.5f;
    [SerializeField, Range(0f, 1f)] private float accuracy = 1f;
    [SerializeField, Range(0f, 1f)] private float criticalHitChance = 0.05f;

    public override bool IsElementalAttackEffect => true;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        if (Random.value > accuracy)
        {
            target.ShowDamage(0, Color.white);
            return false;
        }

        float variance = Random.Range(0.85f, 1.15f);
        float finalDamage = baseDamage * variance;

        Damage damage = new Damage(finalDamage, Element, false, criticalHitChance, 1.5f);
        DamageCtx ctx = new DamageCtx(caster, target, damage);
        
        // We want to know how much damage was actually dealt after modifiers
        float healthBefore = target.CurrentHealth;
        
        DamageSystem.Process(ctx, log);
        
        float actualDamageDealt = healthBefore - target.CurrentHealth;
        if (actualDamageDealt > 0)
        {
            float healAmount = actualDamageDealt * lifestealPercent;
            caster.Heal(healAmount);

            log.AddHealEffectLog(new HealEffectLog()
            {
                AppliedTarget = caster.Stats.EntityName,
                AppliedTargetID = caster.GetEntityID(),
                HealAmount = healAmount
            });
        }

        return true;
    }
}
