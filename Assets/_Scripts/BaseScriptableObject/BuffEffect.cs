using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuffEffect", menuName = "ScriptableObjects/SkillEffect/BuffEffect")]

public class BuffEffect : SkillEffect
{
    public List<Buff> buffs;

    public override void Execute(Entity caster, Entity target, CombatActionLog log)
    {
        bool isBuff =
            Random.Range(0f, 100f) < caster.GetStat(StatType.StatusHitChance) * 100;

        bool isResist =
            Random.Range(0f, 100f) < target.GetStat(StatType.StatusEffectResistance);
        Debug.Log(caster.gameObject.name + " used BuffEffect on " + target.gameObject.name +
        (isBuff ? " and the buff landed!" : " but the buff missed!") +
        (isResist ? " However, the target resisted the buff." : ""));
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