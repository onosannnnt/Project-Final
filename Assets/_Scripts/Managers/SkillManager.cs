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

    public void UseSkill(Skill skill, Entity target, CombatActionLog log)
    {
        Debug.Log(owner.gameObject.name + " is using skill: " + skill.skillName + " on " + target.gameObject.name);
        if (skill == null || target == null) return;
        skill.Execute(owner, target, log);
        Debug.Log(owner.gameObject.name + " has " + owner.CurrentHealth + " HP and " + owner.CurrentSP + " SP after using skill: " + skill.skillName);
    }
    public List<Skill> GetSkills()
    {
        return skills;
    }
    public void SetSkills(List<Skill> skills)
    {
        this.skills = skills;
    }
    public Skill RandomSkill()
    {
        if (skills == null || skills.Count == 0) return null;
        int index = Random.Range(0, skills.Count);
        return skills[index];
    }
}