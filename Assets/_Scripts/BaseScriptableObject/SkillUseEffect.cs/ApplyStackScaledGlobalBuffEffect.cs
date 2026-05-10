using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ApplyStackScaledGlobalBuffEffect", menuName = "ScriptableObjects/SkillEffect/ApplyStackScaledGlobalBuffEffect")]
public class ApplyStackScaledGlobalBuffEffect : SkillEffect
{
    [SerializeField] private StackScaledGlobalDamageBuff globalBuffTemplate;
    [SerializeField] private int duration = 3;

    private void Awake()
    {
        ExecuteOnce = true;
    }

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (caster == null || globalBuffTemplate == null) return false;

        List<Entity> allies = GetAliveAllies(caster);
        foreach (var ally in allies)
        {
            StackScaledGlobalDamageBuff runtimeBuff = Instantiate(globalBuffTemplate);
            runtimeBuff.Duration = duration;
            runtimeBuff.isPermanent = false;

            ActiveBuff state = new ActiveBuff(runtimeBuff);
            runtimeBuff.SetSource(state, caster); // Crucial: sets the source for scaling
            
            // Manual AddBuff since we need to pass our custom state
            // Or we just add it and then set it, but OnApply might need it immediately.
            // Let's use the standard flow:
            ally.buffController.AddBuff(runtimeBuff);
            ActiveBuff added = ally.buffController.GetBuffByName(runtimeBuff.BuffName);
            if (added != null)
            {
                runtimeBuff.SetSource(added, caster);
            }

            log.AddBuffEffectLog(new BuffEffectLog()
            {
                AppliedTargetID = ally.GetEntityID(),
                AppliedTarget = ally.Stats.EntityName,
                Buff = new BuffEffectData(runtimeBuff)
            });
        }

        return true;
    }

    private List<Entity> GetAliveAllies(Entity caster)
    {
        if (caster is PlayerEntity)
        {
            return PlayerTeamManager.Instance != null ? PlayerTeamManager.Instance.GetAliveMembers().ConvertAll(p => (Entity)p) : new List<Entity> { caster };
        }
        return new List<Entity> { caster };
    }
}
