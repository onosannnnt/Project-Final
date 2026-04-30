using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TeamStackScaledDamageEffect", menuName = "ScriptableObjects/SkillEffect/TeamStackScaledDamageEffect")]
public class TeamStackScaledDamageEffect : SkillEffect
{
    [Header("Buff Template")]
    [Tooltip("Template buff that adds outgoing bonus damage based on stack count.")]
    public StackScaledDamageBuff BuffTemplate;

    [Header("Stack Rule")]
    [Tooltip("Optional source buff asset that provides stack count.")]
    public Buff StackSourceBuff;

    [Tooltip("Fallback source buff name when StackSourceBuff is not assigned.")]
    public string StackSourceBuffName = "Frenzy";

    [Tooltip("Every X stacks grants one bonus step.")]
    [Min(1)] public int StacksPerStep = 1;

    [Tooltip("Flat bonus damage granted per step.")]
    public float BonusDamagePerStep = 50f;

    [Header("Duration")]
    [Tooltip("How many turns the team buff remains active.")]
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

            StackScaledDamageBuff runtimeBuff = Instantiate(BuffTemplate);
            runtimeBuff.StackSourceBuff = StackSourceBuff;
            runtimeBuff.StackSourceBuffName = StackSourceBuffName;
            runtimeBuff.StacksPerStep = Mathf.Max(1, StacksPerStep);
            runtimeBuff.BonusDamagePerStep = BonusDamagePerStep;
            runtimeBuff.Duration = Mathf.Max(1, DurationTurns);
            runtimeBuff.isPermanent = false;

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