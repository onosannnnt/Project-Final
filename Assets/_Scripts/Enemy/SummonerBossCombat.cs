using UnityEngine;

public class SummonerBossCombat : BossCombat
{
    private int turnCounter = 0;
    private int summonCooldown = 0; // 0 means ready to use

    public override Skill ChooseNextSkill()
    {
        var skills = skillManager.GetSkills();
        if (skills == null || skills.Count == 0) return null;

        // Reduce cooldown if it's counting down
        if (summonCooldown > 0)
        {
            summonCooldown--;
        }

        // Count how many enemies are currently alive
        int aliveEnemies = 0;
        foreach (var enemy in FindObjectsOfType<EnemyCombat>())
        {
            if (!enemy.IsDead())
            {
                aliveEnemies++;
            }
        }

        // Find if the boss has a summon skill
        Skill summonSkill = null;
        foreach (var skill in skills)
        {
            foreach (var effect in skill.SkillEffects)
            {
                if (effect is SummonEffect)
                {
                    summonSkill = skill;
                    break;
                }
            }
            if (summonSkill != null) break;
        }

        // Priority Cooldown > Condition
        // Condition: Only 1 enemy alive (the boss itself)
        if (summonCooldown == 0 && aliveEnemies == 1 && summonSkill != null)
        {
            summonCooldown = 2; // 1 turn cooldown (takes 2 turns to be ready again since it ticks down once per turn)
            return summonSkill;
        }

        // Otherwise pick a non-summon skill randomly or by rotation
        Skill selectedSkill;
        do
        {
            selectedSkill = skills[turnCounter % skills.Count];
            turnCounter++;
        }
        while (selectedSkill == summonSkill); // Make sure we don't accidentally do a normal attack rotation with the summon skill

        return selectedSkill;
    }
}
