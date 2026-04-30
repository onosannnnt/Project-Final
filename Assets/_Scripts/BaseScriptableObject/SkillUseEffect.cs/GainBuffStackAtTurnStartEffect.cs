using UnityEngine;

[CreateAssetMenu(fileName = "GainBuffStackAtTurnStartEffect", menuName = "ScriptableObjects/SkillEffect/GainBuffStackAtTurnStartEffect")]
public class GainBuffStackAtTurnStartEffect : SkillEffect
{
    [Header("Apply Settings")]
    public TargetType ApplyTo = TargetType.Self;

    [Header("Turn Start Buff Template")]
    [Tooltip("Template buff that controls per-turn stack gain.")]
    public TurnStartStackGainBuff TurnStartBuffTemplate;

    [Tooltip("Optional override for which buff receives stacks each turn. If null, template value is used.")]
    public Buff BuffToGain;

    [Tooltip("How many stacks to grant at the start of each turn.")]
    [Min(1)] public int StacksPerTurn = 3;

    [Tooltip("How many turns this effect lasts.")]
    [Min(1)] public int DurationTurns = 3;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        Entity applyTarget = ApplyTo == TargetType.Self ? caster : target;
        if (applyTarget == null || applyTarget.buffController == null || TurnStartBuffTemplate == null)
        {
            return false;
        }

        TurnStartStackGainBuff runtimeBuff = Instantiate(TurnStartBuffTemplate);
        if (BuffToGain != null)
        {
            runtimeBuff.BuffToGain = BuffToGain;
        }

        runtimeBuff.StacksPerTurn = Mathf.Max(1, StacksPerTurn);
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