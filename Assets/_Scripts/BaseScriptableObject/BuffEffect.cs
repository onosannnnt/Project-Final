using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "ScriptableObjects/SkillEffect/BuffEffect")]

public class BuffEffect : SkillEffect
{
    public List<Buff> buffs;
    public override SkillEffectPhase Phase => SkillEffectPhase.PreDamage;

    public override void Execute(Entity caster, Entity target)
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
                caster.buffController.addBuff(buff);
            }
            else if (isBuff && !isResist)
                target.buffController.addBuff(buff);
        }
    }
}