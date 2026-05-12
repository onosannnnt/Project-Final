using UnityEngine;

[CreateAssetMenu(fileName = "ResetPillarsEffect", menuName = "ScriptableObjects/SkillEffect/ResetPillarsEffect")]
public class ResetPillarsEffect : SkillEffect
{
    private void Awake()
    {
        ExecuteOnce = true;
    }

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        // เช็คว่าคนที่ใช้สกิลนี้คือ PuzzleBossCombat หรือไม่
        if (caster is PuzzleBossCombat boss)
        {
            // สั่งให้บอสทำการ Shuffle และกระจายธาตุเสา
            boss.ExecutePillarReset();

            // (Optional) เก็บ Log ไว้แสดงผล
            // Debug.Log($"{caster.gameObject.name} ร่ายสกิล Reset เสาเรียบร้อยแล้ว!");
            return true;
        }

        return false;
    }
}