using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TauntEffect", menuName = "ScriptableObjects/SkillEffect/TauntEffect")]
public class TauntEffect : SkillEffect
{
    [Tooltip("Extra damage the target takes while taunted")]
    public float extraDamageTaken = 100f;
    
    public TauntBuff tauntBuffTemplate;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        // Apply Taunt Buff with Vulnerability (+100 extra dmg while active)
        if (tauntBuffTemplate != null)
        {
            TauntBuff tauntBuff = Instantiate(tauntBuffTemplate);
            
            // Set the caster as the required target to attack
            tauntBuff.Taunter = caster;
            tauntBuff.ExtraDamageTaken = extraDamageTaken;

            target.buffController.AddBuff(tauntBuff);

            log.AddBuffEffectLog(new BuffEffectLog()
            {
                AppliedTargetID = target.GetEntityID(),
                AppliedTarget = target.gameObject.name,
                Buff = new BuffEffectData(tauntBuff)
            });
        }
        
        return true;
    }
}
