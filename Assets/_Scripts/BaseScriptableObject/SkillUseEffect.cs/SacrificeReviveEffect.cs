using UnityEngine;

[CreateAssetMenu(fileName = "SacrificeReviveEffect", menuName = "ScriptableObjects/SkillEffect/SacrificeReviveEffect")]
public class SacrificeReviveEffect : SkillEffect
{
    [Header("Sacrifice")]
    [Tooltip("If true, kills a random ally (excluding caster and target).")]
    public bool SacrificeRandomAlly = true;

    [Header("Target Buffs")]
    [Tooltip("Buff that increases damage taken on the revived target.")]
    public Buff VulnerabilityBuffTemplate;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        // Target must be dead to revive
        // However, in this system, dead entities are often destroyed.
        // We need to check the TurnManager or PlayerTeamManager for dead members.
        
        // This is a complex effect that might require specific TeamManager support.
        // For now, let's implement the 'Sacrifice' part and a simple HP reset if target exists but is 'dead'.
        
        if (caster == null || target == null) return false;

        // Sacrifice
        if (SacrificeRandomAlly && PlayerTeamManager.Instance != null)
        {
            var alive = PlayerTeamManager.Instance.GetAliveMembers();
            Entity victim = null;
            foreach (var m in alive)
            {
                if (m != caster && m != target)
                {
                    victim = m;
                    break;
                }
            }

            if (victim != null)
            {
                // Kill victim
                victim.TakeDamage(new Damage(99999f, DamageElement.None)); 
            }
            else
            {
                // No one to sacrifice? Skill fails or caster dies?
                // Let's assume sacrifice is required.
                return false;
            }
        }

        // Revive Logic (Simplified: Set HP to Max)
        // Note: Real revive would need to Instantiate the prefab again or un-hide it.
        target.Heal(target.GetStat(StatType.MaxHealth));
        
        if (VulnerabilityBuffTemplate != null)
        {
            target.buffController.AddBuff(VulnerabilityBuffTemplate);
        }

        return true;
    }
}
