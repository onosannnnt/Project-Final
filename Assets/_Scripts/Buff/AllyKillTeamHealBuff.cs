using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AllyKillTeamHealBuff", menuName = "ScriptableObjects/Buff/AllyKillTeamHealBuff")]
public class AllyKillTeamHealBuff : Buff
{
    [Header("Team Heal On Ally Kill")]
    [Tooltip("Heal amount applied to each alive ally when an ally kills an enemy while this buff is active.")]
    [Min(0f)] public float TeamHealAmount = 100f;

    private const string KillCallbackStateKey = "AllyKillTeamHealBuff_OnEntityKilledCallback";

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);

        if (owner == null || TeamHealAmount <= 0f)
        {
            return;
        }

        if (buffState.CustomState.TryGetValue(KillCallbackStateKey, out object existingObj) &&
            existingObj is Action<Entity, Entity, CombatActionLog>)
        {
            return;
        }

        Action<Entity, Entity, CombatActionLog> callback = null;
        callback = (killer, victim, log) =>
        {
            if (owner == null)
            {
                DamageSystem.OnEntityKilled -= callback;
                buffState.CustomState.Remove(KillCallbackStateKey);
                return;
            }

            if (killer == null || victim == null)
            {
                return;
            }

            if (!AreAllies(owner, killer))
            {
                return;
            }

            if (!AreEnemies(owner, victim))
            {
                return;
            }

            HealAllies(owner, TeamHealAmount, log);
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
        if (a == null || b == null)
        {
            return false;
        }

        bool bothPlayers = a is PlayerEntity && b is PlayerEntity;
        bool bothEnemies = a is EnemyCombat && b is EnemyCombat;
        return bothPlayers || bothEnemies;
    }

    private static bool AreEnemies(Entity a, Entity b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        bool playerVsEnemy = a is PlayerEntity && b is EnemyCombat;
        bool enemyVsPlayer = a is EnemyCombat && b is PlayerEntity;
        return playerVsEnemy || enemyVsPlayer;
    }

    private static void HealAllies(Entity owner, float healAmount, CombatActionLog log)
    {
        if (healAmount <= 0f)
        {
            return;
        }

        if (owner is PlayerEntity)
        {
            if (PlayerTeamManager.Instance == null)
            {
                HealEntity(owner, healAmount, log);
                return;
            }

            foreach (PlayerEntity ally in PlayerTeamManager.Instance.GetAliveMembers())
            {
                HealEntity(ally, healAmount, log);
            }

            return;
        }

        if (owner is EnemyCombat)
        {
            EnemyCombat[] enemies = UnityEngine.Object.FindObjectsOfType<EnemyCombat>();
            foreach (EnemyCombat ally in enemies)
            {
                if (ally == null || ally.IsDead() || ally.CurrentHealth <= 0f)
                {
                    continue;
                }

                HealEntity(ally, healAmount, log);
            }

            return;
        }

        HealEntity(owner, healAmount, log);
    }

    private static void HealEntity(Entity target, float healAmount, CombatActionLog log)
    {
        if (target == null || target.CurrentHealth <= 0f || healAmount <= 0f)
        {
            return;
        }

        target.Heal(healAmount);

        if (log != null)
        {
            log.AddHealEffectLog(new HealEffectLog()
            {
                AppliedTargetID = target.GetEntityID(),
                AppliedTarget = target.Stats != null ? target.Stats.EntityName : target.gameObject.name,
                HealAmount = healAmount
            });
        }
    }
}