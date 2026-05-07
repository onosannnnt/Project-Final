using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BloodRage", menuName = "ScriptableObjects/SkillEffect/BloodRageEffect")]
public class BloodRageEffect : SkillEffect
{
    public float maxHpCostPercentage = 0.20f; // 20%
    public List<Buff> buffsToApply;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        // 1. Lose 20% of Max HP
        int hpCost = Mathf.RoundToInt(caster.GetStat(StatType.MaxHealth) * maxHpCostPercentage);
        
        if (hpCost > 0) 
        {
            // Use PreventDeath = true to ensure player doesn't suicide
            Damage selfDamage = new Damage((float)hpCost, DamageElement.Physical, false, 0, 1.5f, true);
            caster.TakeDamage(selfDamage); 
        }

        // 2. Apply buffs (Gain damage for 3 turns)
        if (buffsToApply != null)
        {
            foreach (var buff in buffsToApply)
            {
                caster.buffController.AddBuff(buff);

                log.AddBuffEffectLog(new BuffEffectLog()
                {
                    AppliedTargetID = caster.GetEntityID(),
                    AppliedTarget = caster.gameObject.name,
                    Buff = new BuffEffectData(buff)
                });
            }
        }
        
        return true;
    }
}
