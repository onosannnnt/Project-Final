using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AllyKillRewardBuff", menuName = "ScriptableObjects/Buff/AllyKillRewardBuff")]
public class AllyKillRewardBuff : Buff
{
    [Header("Heal Reward")]
    public float TeamHealAmount = 100f;

    [Header("Stack Reward")]
    public Buff StackToGrant;
    public int StacksToGrant = 2;

    private const string KillCallbackStateKey = "AllyKillRewardBuff_OnEntityKilledCallback";

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);

        if (owner == null) return;

        Action<Entity, Entity, CombatActionLog> callback = (killer, victim, log) =>
        {
            if (owner == null)
            {
                DamageSystem.OnEntityKilled -= (Action<Entity, Entity, CombatActionLog>)buffState.CustomState[KillCallbackStateKey];
                return;
            }

            if (killer == null || victim == null) return;
            if (!AreAllies(owner, killer)) return;
            if (!AreEnemies(owner, victim)) return;

            // 1. Heal Allies
            if (TeamHealAmount > 0) HealAllies(owner, TeamHealAmount, log);

            // 2. Grant Stacks to Party
            if (StackToGrant != null) GrantStacksToParty(owner, StackToGrant, StacksToGrant, log);
        };

        DamageSystem.OnEntityKilled += callback;
        buffState.CustomState[KillCallbackStateKey] = callback;
    }

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);
        if (buffState.CustomState.TryGetValue(KillCallbackStateKey, out object callbackObj) &&
            callbackObj is Action<Entity, Entity, CombatActionLog> callback)
        {
            DamageSystem.OnEntityKilled -= callback;
            buffState.CustomState.Remove(KillCallbackStateKey);
        }
    }

    private static bool AreAllies(Entity a, Entity b)
    {
        bool bothPlayers = a is PlayerEntity && b is PlayerEntity;
        bool bothEnemies = a is EnemyCombat && b is EnemyCombat;
        return bothPlayers || bothEnemies;
    }

    private static bool AreEnemies(Entity a, Entity b)
    {
        bool playerVsEnemy = a is PlayerEntity && b is EnemyCombat;
        bool enemyVsPlayer = a is EnemyCombat && b is PlayerEntity;
        return playerVsEnemy || enemyVsPlayer;
    }

    private void HealAllies(Entity owner, float healAmount, CombatActionLog log)
    {
        var targets = (owner is PlayerEntity) ? 
            (PlayerTeamManager.Instance != null ? PlayerTeamManager.Instance.GetAliveMembers().ConvertAll(p => (Entity)p) : new System.Collections.Generic.List<Entity>{owner}) :
            new System.Collections.Generic.List<Entity>{owner};

        foreach (var target in targets)
        {
            target.Heal(healAmount);
            if (log != null) log.AddHealEffectLog(new HealEffectLog() { AppliedTarget = target.Stats.EntityName, AppliedTargetID = target.GetEntityID(), HealAmount = healAmount });
        }
    }

    private void GrantStacksToParty(Entity owner, Buff stack, int count, CombatActionLog log)
    {
        var targets = (owner is PlayerEntity) ? 
            (PlayerTeamManager.Instance != null ? PlayerTeamManager.Instance.GetAliveMembers().ConvertAll(p => (Entity)p) : new System.Collections.Generic.List<Entity>{owner}) :
            new System.Collections.Generic.List<Entity>{owner};

        foreach (var target in targets)
        {
            for (int i = 0; i < count; i++) target.buffController.AddBuff(stack);
            if (log != null) log.AddBuffEffectLog(new BuffEffectLog() { AppliedTargetID = target.GetEntityID(), AppliedTarget = target.Stats.EntityName, Buff = new BuffEffectData(stack) });
        }
    }
}