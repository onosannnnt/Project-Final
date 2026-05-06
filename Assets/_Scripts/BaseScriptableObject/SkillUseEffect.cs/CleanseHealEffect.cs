using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CleanseHealEffect", menuName = "ScriptableObjects/SkillEffect/CleanseHealEffect")]
public class CleanseHealEffect : SkillEffect
{
    [SerializeField] private float healAmount = 500f;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (target == null) return false;

        // Cleanse Debuffs
        List<ActiveBuff> debuffs = target.buffController.GetDebuffs();
        foreach (var debuff in debuffs)
        {
            target.buffController.RemoveBuff(debuff);
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
