using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ConvertDebuffToStackEffect", menuName = "ScriptableObjects/SkillEffect/ConvertDebuffToStackEffect")]
public class ConvertDebuffToStackEffect : SkillEffect
{
    [SerializeField] private Buff stackToGrant;
    [SerializeField] private int stacksPerDebuff = 2;
    [SerializeField] private int fallbackStacks = 1; // Gain this if no debuffs were removed
    [SerializeField] private bool targetCaster = true;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        Entity applyTarget = targetCaster ? caster : target;
        if (applyTarget == null || applyTarget.buffController == null || stackToGrant == null) return false;

        List<ActiveBuff> debuffs = applyTarget.buffController.GetDebuffs();
        
        int totalStacksToGrant = debuffs.Count > 0 ? (debuffs.Count * stacksPerDebuff) : fallbackStacks;

        // Cleanse debuffs
        foreach (var debuff in debuffs)
        {
            applyTarget.buffController.RemoveBuff(debuff);
        }

        // Grant stacks
        for (int i = 0; i < totalStacksToGrant; i++)
        {
            applyTarget.buffController.AddBuff(stackToGrant);
        }

        log.AddBuffEffectLog(new BuffEffectLog()
        {
            AppliedTargetID = applyTarget.GetEntityID(),
            AppliedTarget = applyTarget.Stats.EntityName,
            Buff = new BuffEffectData(stackToGrant)
        });

        return true;
    }
}
