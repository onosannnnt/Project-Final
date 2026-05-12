using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuffEffect", menuName = "ScriptableObjects/SkillEffect/BuffEffect")]

public class BuffEffect : SkillEffect
{
    public List<Buff> buffs;

    private void Awake()
    {
        Phase = SkillEffectPhase.PreDamage;
    }

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        bool isBuff = true;

        bool isResist = false;
        
// // Debug.Log(caster.gameObject.name + " used BuffEffect on " + target.gameObject.name);
        foreach (var buff in buffs)
        {
            Entity actualTarget = null;
            if (buff.targetType == TargetType.Self)
            {
                caster.buffController.AddBuff(buff);
                actualTarget = caster;
            }
            else if (isBuff && !isResist)
            {
                target.buffController.AddBuff(buff);
                actualTarget = target;
            }

            if (actualTarget != null)
            {
                log.AddBuffEffectLog(new BuffEffectLog()
                {
                    AppliedTargetID = actualTarget.GetEntityID(),
                    AppliedTarget = actualTarget.Stats.EntityName,
                    Buff = new BuffEffectData(buff)
                });
            }
        }
        return true;
    }
}
