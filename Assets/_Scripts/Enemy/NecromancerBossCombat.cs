using UnityEngine;
using System.Collections.Generic;

public class NecromancerBossCombat : BossCombat
{
    [Header("Buff Mechanics")]
    [Tooltip("ใส่ข้อมูล Buff ที่ต้องการให้บอสได้รับเมื่อลูกน้องตาย 1 ตัว")]
    [SerializeField] private Buff minionDeathBuff;

    // ใช้เช็คว่าเทิร์นที่แล้วมีลูกน้องกี่ตัว เพื่อดูว่ามีลูกน้องตายไหม
    private int previousMinionCount = 0;
    private int turnCounter = 0;

    public override Skill ChooseNextSkill()
    {
        var skills = skillManager.GetSkills();
        if (skills == null || skills.Count == 0) return null;

        // 1. ค้นหาสกิลอัญเชิญลูกน้อง
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

        // 2. นับจำนวนศัตรูที่ยังมีชีวิตอยู่ (ไม่นับตัวบอสเอง)
        int currentMinionCount = 0;
        foreach (var enemy in FindObjectsOfType<EnemyCombat>())
        {
            if (!enemy.IsDead() && enemy != this)
            {
                currentMinionCount++;
            }
        }

        // 3. ตรวจสอบว่ามีลูกน้องตายหรือไม่ (เทียบกับเทิร์นที่แล้ว)
        if (currentMinionCount < previousMinionCount)
        {
            int deadMinionsThisTurn = previousMinionCount - currentMinionCount;

            // วนลูปแอด Buff เข้าตัวบอสตามจำนวนลูกน้องที่ตาย (เพื่อให้กลายเป็น Stack)
            if (minionDeathBuff != null && buffController != null)
            {
                for (int i = 0; i < deadMinionsThisTurn; i++)
                {
                    buffController.AddBuff(minionDeathBuff);
                }
                Debug.Log($"<color=red>ลูกน้องตายไป {deadMinionsThisTurn} ตัว! บอสได้รับบัฟความโกรธ {deadMinionsThisTurn} Stack!</color>");
            }
        }

        Skill selectedSkill = null;

        // 4. เงื่อนไขการ Summon: ถ้าลูกน้องตายหมดเกลี้ยง (เหลือ 0 ตัว) ให้เรียกใหม่
        if (currentMinionCount == 0 && summonSkill != null)
        {
            selectedSkill = summonSkill;
            Debug.Log("ลูกน้องตายหมดแล้ว! บอสใช้สกิล Summon เรียกลูกน้องชุดใหม่");
        }
        else
        {
            // 5. ถ้าลูกน้องยังอยู่ ให้โจมตีตามปกติ
            do
            {
                selectedSkill = skills[turnCounter % skills.Count];
                turnCounter++;
            }
            while (selectedSkill == summonSkill); // ป้องกันไม่ให้สุ่มไปโดนสกิลเรียกซ้ำ
        }

        // 6. อัปเดตจำนวนลูกน้องไว้เช็คในเทิร์นถัดไป
        previousMinionCount = currentMinionCount;

        return selectedSkill;
    }
}