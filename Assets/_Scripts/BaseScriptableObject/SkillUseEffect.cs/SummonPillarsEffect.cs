using UnityEngine;

[CreateAssetMenu(fileName = "SummonPillarsEffect", menuName = "ScriptableObjects/SkillEffect/SummonPillarsEffect")]
public class SummonPillarsEffect : SkillEffect
{
    private void Awake()
    {
        ExecuteOnce = true;
    }

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
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