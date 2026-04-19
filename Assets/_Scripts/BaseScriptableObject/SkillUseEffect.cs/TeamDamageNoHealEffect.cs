using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TeamDamageNoHealEffect", menuName = "ScriptableObjects/SkillEffect/TeamDamageNoHealEffect")]
public class TeamDamageNoHealEffect : SkillEffect
{
    [Header("Buff Template")]
    [Tooltip("Template buff that adds flat outgoing damage.")]
    public TeamDamageNoHealBuff BuffTemplate;

    [Header("Values")]
    [Tooltip("Team deals +X damage while buff is active.")]
    public float BonusDamage = 50f;

    [Tooltip("Team cannot be healed for Y turns.")]
    [Min(1)] public int DurationTurns = 3;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (caster == null || BuffTemplate == null)
        {
            return false;
        }

        List<Entity> allies = GetAllies(caster);
        if (allies.Count == 0)
        {
            return false;
        }

        foreach (Entity ally in allies)
        {
            if (ally == null || ally.buffController == null)
            {
                continue;
            }

            TeamDamageNoHealBuff runtimeBuff = Instantiate(BuffTemplate);
            runtimeBuff.BonusDamage = BonusDamage;
            runtimeBuff.Duration = Mathf.Max(1, DurationTurns);
            runtimeBuff.isPermanent = false;
            runtimeBuff.preventHealing = true;

            ally.buffController.AddBuff(runtimeBuff);

            if (log != null)
            {
                log.AddBuffEffectLog(new BuffEffectLog()
                {
                    AppliedTargetID = ally.GetEntityID(),
                    AppliedTarget = ally.Stats != null ? ally.Stats.EntityName : ally.gameObject.name,
                    Buff = new BuffEffectData(runtimeBuff)
                });
            }
        }

        return true;
    }

    private List<Entity> GetAllies(Entity caster)
    {
        List<Entity> allies = new List<Entity>();

        if (caster is PlayerEntity)
        {
            if (PlayerTeamManager.Instance != null)
            {
                foreach (PlayerEntity member in PlayerTeamManager.Instance.GetAliveMembers())
                {
                    if (member != null)
                    {
                        allies.Add(member);
                    }
                }
            }
            else
            {
                allies.Add(caster);
            }

            return allies;
        }

        if (caster is EnemyCombat)
        {
            EnemyCombat[] enemies = Object.FindObjectsOfType<EnemyCombat>();
            foreach (EnemyCombat enemy in enemies)
            {
                if (enemy == null || enemy.IsDead() || enemy.CurrentHealth <= 0f)
                {
                    continue;
                }

                allies.Add(enemy);
            }

            return allies;
        }

        allies.Add(caster);
        return allies;
    }
}