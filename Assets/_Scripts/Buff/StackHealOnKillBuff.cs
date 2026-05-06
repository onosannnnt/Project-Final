using UnityEngine;

[CreateAssetMenu(fileName = "StackHealOnKillBuff", menuName = "ScriptableObjects/Buff/StackHealOnKillBuff")]
public class StackHealOnKillBuff : Buff
{
    [Header("Kill Rewards")]
    [SerializeField] private Buff stackToGrant;
    [SerializeField] private int stacksOnKill = 2;
    [SerializeField] private float healOnKill = 200f;

    private const string HandlerKey = "StackHealOnKillHandler";

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);
        
        KillHandler handler = new KillHandler(owner, this);
        buffState.CustomState[HandlerKey] = handler;
        DamageSystem.OnEntityKilled += handler.OnKilled;
    }

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);

        if (buffState.CustomState.TryGetValue(HandlerKey, out object obj) && obj is KillHandler handler)
        {
            DamageSystem.OnEntityKilled -= handler.OnKilled;
            buffState.CustomState.Remove(HandlerKey);
        }
    }

    private class KillHandler
    {
        private Entity owner;
        private StackHealOnKillBuff data;

        public KillHandler(Entity owner, StackHealOnKillBuff data)
        {
            this.owner = owner;
            this.data = data;
        }

        public void OnKilled(Entity caster, Entity target, CombatActionLog log)
        {
            if (caster != owner) return;

            // Grant Stacks
            if (data.stackToGrant != null)
            {
                for (int i = 0; i < data.stacksOnKill; i++)
                {
                    owner.buffController.AddBuff(data.stackToGrant);
                }
            }

            // Heal
            owner.Heal(data.healOnKill);

            if (log != null)
            {
                log.AddHealEffectLog(new HealEffectLog()
                {
                    AppliedTarget = owner.Stats.EntityName,
                    AppliedTargetID = owner.GetEntityID(),
                    HealAmount = data.healOnKill
                });
            }
        }
    }
}
