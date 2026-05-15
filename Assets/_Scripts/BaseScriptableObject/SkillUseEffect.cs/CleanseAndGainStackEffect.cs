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

        // Cleanse Negative Effects
        List<ActiveBuff> negativeEffects = target.buffController.GetNegativeEffects();
        int negativeEffectCount = negativeEffects.Count;

        foreach (var effect in negativeEffects)
        {
            target.buffController.RemoveBuff(effect);
        }

        // Calculate stacks to gain
        int stacksToApply = negativeEffectCount > 0 ? (negativeEffectCount * stacksPerDebuff) : fallbackStacks;

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
