using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DispelEnemyBuffGainStackEffect", menuName = "ScriptableObjects/SkillEffect/DispelEnemyBuffGainStackEffect")]
public class DispelEnemyBuffGainStackEffect : SkillEffect
{
    [Header("Stack Reward")]
    [Tooltip("Buff applied to the caster after removing enemy buffs.")]
    public Buff BuffToGain;

    [Tooltip("Gain X stacks for each enemy buff removed.")]
    [Min(1)] public int StacksPerBuffRemoved = 1;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (caster == null || target == null || target.buffController == null)
        {
            return false;
        }

        List<ActiveBuff> removableBuffs = target.buffController.GetBuffs();
        if (removableBuffs == null || removableBuffs.Count == 0)
        {
            return true;
        }

        int removedBuffCount = 0;
        foreach (ActiveBuff activeBuff in removableBuffs)
        {
            if (activeBuff == null || activeBuff.Data == null)
            {
                continue;
            }

            // Ensure modifier cleanup before force-removing the buff.
            activeBuff.Data.OnRemove(target, activeBuff);
            target.buffController.RemoveBuff(activeBuff);
            removedBuffCount += 1;
        }

        int totalStacksToGain = removedBuffCount * Mathf.Max(1, StacksPerBuffRemoved);
        if (BuffToGain != null && totalStacksToGain > 0 && caster.buffController != null)
        {
            for (int i = 0; i < totalStacksToGain; i++)
            {
                caster.buffController.AddBuff(BuffToGain);
            }

            if (log != null)
            {
                log.AddBuffEffectLog(new BuffEffectLog()
                {
                    AppliedTargetID = caster.GetEntityID(),
                    AppliedTarget = caster.Stats != null ? caster.Stats.EntityName : caster.gameObject.name,
                    Buff = new BuffEffectData(BuffToGain)
                });
            }
        }

        return true;
    }
}