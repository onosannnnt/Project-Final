using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CleanseHealEffect", menuName = "ScriptableObjects/SkillEffect/CleanseHealEffect")]
public class CleanseHealEffect : SkillEffect
{
    [SerializeField] private float healAmount = 500f;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        if (target == null) return false;

        // Cleanse Negative Effects (Debuffs & CC)
        List<ActiveBuff> negativeEffects = target.buffController.GetNegativeEffects();
        foreach (var effect in negativeEffects)
        {
            target.buffController.RemoveBuff(effect);
        }

        // Heal
        target.Heal(healAmount);

        log.AddHealEffectLog(new HealEffectLog()
        {
            AppliedTarget = target.Stats.EntityName,
            AppliedTargetID = target.GetEntityID(),
            HealAmount = healAmount
        });

        return true;
    }
}
