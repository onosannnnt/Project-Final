using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CleanseAndGainStackEffect", menuName = "ScriptableObjects/SkillEffect/CleanseAndGainStackEffect")]
public class CleanseAndGainStackEffect : SkillEffect
{
    [SerializeField] private Buff stackToGrant;
    [SerializeField] private int stacksPerDebuff = 2;
    [SerializeField] private int fallbackStacks = 1;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        if (target == null || stackToGrant == null) return false;

        // Cleanse Debuffs
        List<ActiveBuff> debuffs = target.buffController.GetDebuffs();
        int debuffCount = debuffs.Count;

        foreach (var debuff in debuffs)
        {
            target.buffController.RemoveBuff(debuff);
        }

        // Calculate stacks to gain
        int stacksToApply = debuffCount > 0 ? (debuffCount * stacksPerDebuff) : fallbackStacks;

        // Apply stacks
        for (int i = 0; i < stacksToApply; i++)
        {
            target.buffController.AddBuff(stackToGrant);
        }

        log.AddBuffEffectLog(new BuffEffectLog()
        {
            AppliedTargetID = target.GetEntityID(),
            AppliedTarget = target.Stats.EntityName,
            Buff = new BuffEffectData(stackToGrant)
        });

        return true;
    }
}
