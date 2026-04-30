using UnityEngine;

[CreateAssetMenu(fileName = "SummonPillarsEffect", menuName = "ScriptableObjects/SkillEffect/SummonPillarsEffect")]
public class SummonPillarsEffect : SkillEffect
{
    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (caster is PuzzleBossCombat boss)
        {
            // สั่งให้บอสเปิดใช้งานเสา
            boss.ActivatePillars();
            return true;
        }
        return false;
    }
}