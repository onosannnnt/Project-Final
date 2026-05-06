using UnityEngine;

[CreateAssetMenu(fileName = "SacrificeReviveEffect", menuName = "ScriptableObjects/SkillEffect/SacrificeReviveEffect")]
public class SacrificeReviveEffect : SkillEffect
{
    [Header("Revive Settings")]
    [Tooltip("Percentage of Max HP to restore to the target.")]
    public float ReviveHpPercent = 1.0f;

    [Header("Target Buffs")]
    [Tooltip("Buff that increases damage taken on the revived target.")]
    public Buff VulnerabilityBuffTemplate;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (caster == null) return false;

        Entity actualReviveTarget = null;

        // If the skill is TargetType.Self, we automatically look for the first dead ally.
        // If it's TargetType.Ally, we use the selected target.
        if (target == caster)
        {
            if (PlayerTeamManager.Instance != null)
            {
                foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
                {
                    if (member != null && member != caster && member.CurrentHealth <= 0)
                    {
                        actualReviveTarget = member;
                        break;
                    }
                }
            }
        }
        else
        {
            actualReviveTarget = target;
        }

        if (actualReviveTarget == null)
        {
            // Fallback: If no one is dead, reset Player 1 (the first member of the party)
            if (PlayerTeamManager.Instance != null && PlayerTeamManager.Instance.ActiveTeamMembers.Count > 0)
            {
                Entity player1 = PlayerTeamManager.Instance.ActiveTeamMembers[0];
                if (player1 != null)
                {
                    // 1. Clear Corrupted HP
                    player1.SetCorruptedHealth(0);
                    
                    // 2. Refill HP to 100%
                    player1.Heal(player1.GetStat(StatType.MaxHealth));
                    
                    // 3. Remove every buff and debuff
                    var allBuffs = new System.Collections.Generic.List<ActiveBuff>(player1.buffController.GetAllBuffs());
                    foreach (var buff in allBuffs)
                    {
                        player1.buffController.RemoveBuff(buff);
                    }

                    // 4. Apply the penalty debuff after the clear
                    if (VulnerabilityBuffTemplate != null)
                    {
                        player1.buffController.AddBuff(VulnerabilityBuffTemplate);
                    }
                }
            }
            
            // Caster is still sacrificed as per the "Exchange" theme
            caster.TakeDamage(new Damage(999999f, DamageElement.None));
            return true;
        }

        // 1. Revive the target (If someone was dead)
        float maxHP = actualReviveTarget.GetStat(StatType.MaxHealth);
        actualReviveTarget.Heal(maxHP * ReviveHpPercent);
        
        // Ensure the revived target is active (if it was deactivated)
        actualReviveTarget.gameObject.SetActive(true);

        // 2. Apply the penalty debuff to the revived target
        if (VulnerabilityBuffTemplate != null)
        {
            actualReviveTarget.buffController.AddBuff(VulnerabilityBuffTemplate);
            log.AddBuffEffectLog(new BuffEffectLog()
            {
                AppliedTargetID = actualReviveTarget.GetEntityID(),
                AppliedTarget = actualReviveTarget.Stats.EntityName,
                Buff = new BuffEffectData(VulnerabilityBuffTemplate)
            });
        }

        // 3. Sacrifice the Caster
        caster.TakeDamage(new Damage(999999f, DamageElement.None));

        return true;
    }
}
