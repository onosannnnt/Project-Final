using UnityEngine;

[CreateAssetMenu(fileName = "GainBuffAndSPEffect", menuName = "ScriptableObjects/SkillEffect/GainBuffAndSPEffect")]
public class GainBuffAndSPEffect : SkillEffect
{
    [Header("Resource Restore")]
    [Tooltip("Amount of SP / MP to restore.")]
    public int SPRestoreAmount = 20;

    [Header("Buff Details")]
    public Buff BuffToApply;
    [Tooltip("Number of stacks to gain per execution.")]
    public int StacksToGain = 2;
    public TargetType ApplyTo = TargetType.Self;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        Entity applyTarget = ApplyTo == TargetType.Self ? caster : target;
        
        if (applyTarget == null) return false;

        // 1. Restore SP (MP)
        if (SPRestoreAmount > 0)
        {
            applyTarget.SetSP(SPRestoreAmount);
// // Debug.Log($"{applyTarget.gameObject.name} restored {SPRestoreAmount} SP.");
        }

        // 2. Add Buff Stacks
        if (BuffToApply != null && StacksToGain > 0)
        {
            for (int i = 0; i < StacksToGain; i++)
            {
                applyTarget.buffController.AddBuff(BuffToApply);
            }
            
// // Debug.Log($"{applyTarget.gameObject.name} gained {StacksToGain} stacks of {BuffToApply.BuffName}.");

            if (log != null)
            {
                log.AddBuffEffectLog(new BuffEffectLog()
                {
                    AppliedTarget = applyTarget.Stats.EntityName,
                    AppliedTargetID = applyTarget.GetEntityID(),
                    Buff = new BuffEffectData(BuffToApply)
                });
            }
        }

        return true;
    }
}
