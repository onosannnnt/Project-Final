using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CorruptedBloodRageEffect", menuName = "ScriptableObjects/SkillEffect/CorruptedBloodRageEffect")]
public class CorruptedBloodRageEffect : SkillEffect
{
    [SerializeField] private float corruptedHealthToGain = 500f;
    [SerializeField] private List<Buff> buffsToApply;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        if (caster == null) return false;

        // 1. Gain Corrupted Health
        if (corruptedHealthToGain > 0)
        {
            caster.GainCorruptedHealth(corruptedHealthToGain);
        }

        // 2. Apply buffs (Expected to include CorruptedScalingDamageBuff)
        if (buffsToApply != null)
        {
            foreach (var buff in buffsToApply)
            {
                caster.buffController.AddBuff(buff);

                log.AddBuffEffectLog(new BuffEffectLog()
                {
                    AppliedTargetID = caster.GetEntityID(),
                    AppliedTarget = caster.Stats.EntityName,
                    Buff = new BuffEffectData(buff)
                });
            }
        }

        return true;
    }
}
