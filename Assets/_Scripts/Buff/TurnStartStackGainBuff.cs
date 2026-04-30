using UnityEngine;

[CreateAssetMenu(fileName = "TurnStartStackGainBuff", menuName = "ScriptableObjects/Buff/TurnStartStackGainBuff")]
public class TurnStartStackGainBuff : Buff
{
    [Header("Turn Start Stack Gain")]
    [Tooltip("Buff that will gain stacks at the start of each turn while this buff is active.")]
    public Buff BuffToGain;

    [Tooltip("How many stacks of BuffToGain are granted each turn start.")]
    [Min(1)] public int StacksPerTurn = 3;

    public override void OnTurnStart(Entity owner, CombatActionLog log, ActiveBuff buffState)
    {
        base.OnTurnStart(owner, log, buffState);

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