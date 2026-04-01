using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuffEffect", menuName = "ScriptableObjects/SkillEffect/BuffEffect")]

public class BuffEffect : SkillEffect
{
    public List<Buff> buffs;

    public override void Execute(Entity caster, Entity target, CombatActionLog log)
    {
        bool isBuff = true;

        bool isResist = false;
        
        Debug.Log(caster.gameObject.name + " used BuffEffect on " + target.gameObject.name);
        foreach (var buff in buffs)
        {
            if (buff.targetType == TargetType.Self)
            {
                caster.buffController.AddBuff(buff);
            }
            else if (isBuff && !isResist)
            {
                target.buffController.AddBuff(buff);
            }
            log.AddBuffEffectLog(new BuffEffectLog()
            {
                AppliedTargetID = isBuff && !isResist ? target.GetEntityID() : caster.GetEntityID(),
                AppliedTarget = isBuff && !isResist ? target.gameObject.name : caster.gameObject.name,
                Buff = new BuffEffectData(buff)
            });
        }
    }
}