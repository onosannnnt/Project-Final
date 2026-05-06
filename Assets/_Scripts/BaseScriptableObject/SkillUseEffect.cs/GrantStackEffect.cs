using UnityEngine;

[CreateAssetMenu(fileName = "GrantStackEffect", menuName = "ScriptableObjects/SkillEffect/GrantStackEffect")]
public class GrantStackEffect : SkillEffect
{
    [SerializeField] private Buff buffToGrant;
    [SerializeField, Min(1)] private int stacksToGrant = 1;
    [SerializeField] private bool targetCaster = false;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        Entity applyTarget = targetCaster ? caster : target;
        if (applyTarget == null || applyTarget.buffController == null || buffToGrant == null) return false;

        for (int i = 0; i < stacksToGrant; i++)
        {
            applyTarget.buffController.AddBuff(buffToGrant);
        }

        log.AddBuffEffectLog(new BuffEffectLog()
        {
            AppliedTargetID = applyTarget.GetEntityID(),
            AppliedTarget = applyTarget.Stats.EntityName,
            Buff = new BuffEffectData(buffToGrant)
        });

        return true;
    }
}
