using UnityEngine;

[CreateAssetMenu(fileName = "TurnEndStackGainBuff", menuName = "ScriptableObjects/Buff/TurnEndStackGainBuff")]
public class TurnEndStackGainBuff : Buff
{
    [Header("Turn End Stack Gain")]
    [Tooltip("Buff that will gain stacks at the end of each turn while this buff is active.")]
    public Buff BuffToGain;

    [Tooltip("How many stacks of BuffToGain are granted each turn end.")]
    [Min(1)] public int StacksPerTurn = 1;

    public override void OnTurnEndAction(Entity owner, CombatActionLog log, ActiveBuff buffState)
    {
        base.OnTurnEndAction(owner, log, buffState);

        if (owner == null || owner.buffController == null || BuffToGain == null)
        {
            return;
        }

        int stacksToApply = Mathf.Max(1, StacksPerTurn);
        for (int i = 0; i < stacksToApply; i++)
        {
            owner.buffController.AddBuff(BuffToGain);
        }

        if (log != null)
        {
            log.AddBuffEffectLog(new BuffEffectLog()
            {
                AppliedTargetID = owner.GetEntityID(),
                AppliedTarget = owner.Stats != null ? owner.Stats.EntityName : owner.gameObject.name,
                Buff = new BuffEffectData(BuffToGain)
            });
        }
    }
}