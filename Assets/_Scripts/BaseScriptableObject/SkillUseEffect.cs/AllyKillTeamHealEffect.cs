using UnityEngine;

[CreateAssetMenu(fileName = "AllyKillTeamHealEffect", menuName = "ScriptableObjects/SkillEffect/AllyKillTeamHealEffect")]
public class AllyKillTeamHealEffect : SkillEffect
{
    [Header("Apply Settings")]
    public TargetType ApplyTo = TargetType.Self;

    [Header("Buff Template")]
    [Tooltip("Buff template that listens for ally kill events.")]
    public AllyKillTeamHealBuff BuffTemplate;

    [Header("Effect Values")]
    [Tooltip("Each ally heals this amount when an ally kills an enemy while buff is active.")]
    [Min(0f)] public float TeamHealAmount = 100f;

    [Tooltip("Duration of the applied buff in turns.")]
    [Min(1)] public int DurationTurns = 3;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        Entity applyTarget = ApplyTo == TargetType.Self ? caster : target;
        if (applyTarget == null || applyTarget.buffController == null || BuffTemplate == null)
        {
            return false;
        }

        AllyKillTeamHealBuff runtimeBuff = Instantiate(BuffTemplate);
        runtimeBuff.TeamHealAmount = Mathf.Max(0f, TeamHealAmount);
        runtimeBuff.Duration = Mathf.Max(1, DurationTurns);
        runtimeBuff.isPermanent = false;

        applyTarget.buffController.AddBuff(runtimeBuff);

        if (log != null)
        {
            log.AddBuffEffectLog(new BuffEffectLog()
            {
                AppliedTargetID = applyTarget.GetEntityID(),
                AppliedTarget = applyTarget.Stats != null ? applyTarget.Stats.EntityName : applyTarget.gameObject.name,
                Buff = new BuffEffectData(runtimeBuff)
            });
        }

        return true;
    }
}