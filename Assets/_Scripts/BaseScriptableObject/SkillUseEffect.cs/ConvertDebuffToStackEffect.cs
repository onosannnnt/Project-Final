using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ConvertDebuffToStackEffect", menuName = "ScriptableObjects/SkillEffect/ConvertDebuffToStackEffect")]
public class ConvertDebuffToStackEffect : SkillEffect
{
    [SerializeField] private Buff stackToGrant;
    [SerializeField] private int stacksPerDebuff = 1;
    [SerializeField] private bool targetCaster = true;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        Entity applyTarget = targetCaster ? caster : target;
        if (applyTarget == null || applyTarget.buffController == null || stackToGrant == null) return false;

        List<ActiveBuff> debuffs = applyTarget.buffController.GetDebuffs();
        if (debuffs.Count == 0) return true;

        int totalStacksToGrant = debuffs.Count * stacksPerDebuff;

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
