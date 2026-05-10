using System.Collections.Generic;
using UnityEngine;

public class SkillManager
{
    private Entity owner;
    List<Skill> skills;
    public SkillManager(Entity owner)
    {
        this.owner = owner;
    }

    public void UseSkill(Skill skill, List<Entity> targets, CombatActionLog log, bool allowWindSpread = true)
    {
        if (skill == null || targets == null || targets.Count == 0) return;

        // --- Prepare Damage Context for all targets ---
        List<EnemyCombat> enemyTargets = new List<EnemyCombat>();
        foreach (var t in targets)
        {
            if (t is EnemyCombat enemy)
            {
                enemyTargets.Add(enemy);
                enemy.BeginIncomingSkillDamageContext(allowWindSpread);
            }
        }

        try
        {
            bool skillHit = skill.Execute(owner, targets, log);
            DamageElement breakElement = skill.GetElement();

            // --- Break Mechanic ---
            if (skillHit && owner is PlayerEntity)
            {
                foreach (var target in targets)
                {
                    if (target is EnemyCombat enemyTarget && skill.reducesArmor)
                    {
                        if (skill.TargetType == TargetType.Enemy)
                        {
                            if (skill.TargetCount == TargetCount.Single)
                            {
                                enemyTarget.ReduceArmor(2, breakElement);
                            }
                            else if (skill.TargetCount == TargetCount.All)
                            {
                                enemyTarget.ReduceArmor(1, breakElement);
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            foreach (var enemy in enemyTargets)
            {
                enemy.EndIncomingSkillDamageContext();
            }
        }
    }
    public List<Skill> GetSkills()
    {
        return skills;
    }
    public void SetSkills(List<Skill> skills)
    {
        this.skills = new List<Skill>();
        if (skills != null)
        {
            foreach (var skill in skills)
            {
                if (skill != null)
                {
                    this.skills.Add(skill.Clone());
                }
            }
        }
    }
    public Skill RandomSkill()
    {
        if (skills == null || skills.Count == 0) return null;
        int index = Random.Range(0, skills.Count);
        return skills[index];
    }
}
