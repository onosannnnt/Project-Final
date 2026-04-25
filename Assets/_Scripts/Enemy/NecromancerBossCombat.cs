using UnityEngine;
using System.Collections.Generic;

public class NecromancerBossCombat : BossCombat
{
    [Header("Buff Mechanics")]
    [Tooltip("เปอร์เซ็นต์ดาเมจที่เพิ่มขึ้นต่อลูกน้อง 1 ตัวที่ตาย (เช่น 0.1 = 10%)")]
    [SerializeField] private float damageBuffPerDeath = 0.1f;

    // เก็บค่า Multiplier พลังโจมตีปัจจุบัน (เริ่มต้นที่ 1.0 คือดาเมจปกติ 100%)
    private float currentDamageMultiplier = 1.0f;

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
            // ตรวจสอบว่ายังไม่ตาย และไม่ใช่ตัวบอสเอง
            if (!enemy.IsDead() && enemy != this)
            {
                currentMinionCount++;
            }
        }

        // 3. ตรวจสอบว่ามีลูกน้องตายหรือไม่ (เทียบกับเทิร์นที่แล้ว)
        if (currentMinionCount < previousMinionCount)
        {
            int deadMinionsThisTurn = previousMinionCount - currentMinionCount;

            // เพิ่มความเก่งให้บอสตามจำนวนลูกน้องที่ตาย
            currentDamageMultiplier += (damageBuffPerDeath * deadMinionsThisTurn);

            Debug.Log($"<color=red>ลูกน้องตายไป {deadMinionsThisTurn} ตัว! บอสโกรธขึ้น ดาเมจตอนนี้: {currentDamageMultiplier * 100}%</color>");
        }

        Skill selectedSkill = null;

        // 4. เงื่อนไขการ Summon: ถ้าลูกน้องตายหมดเกลี้ยง (เหลือ 0 ตัว) ให้เรียกใหม่
        if (currentMinionCount == 0 && summonSkill != null)
        {
            selectedSkill = summonSkill;
            Debug.Log("ลูกน้องตายหมดแล้ว! บอสใช้สกิล Summon เรียกลูกน้องชุดใหม่");

            // หมายเหตุ: เราไม่จำเป็นต้องใส่ Cooldown แบบตัวเก่า เพราะเงื่อนไขบังคับว่าต้อง "ลูกน้องหมด" ถึงจะเรียกได้
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
        // (ถ้าเทิร์นนี้ใช้สกิล Summon เทิร์นหน้าจำนวนลูกน้องจะเพิ่มขึ้นเอง ทำให้ไม่ติดบัคเข้าเงื่อนไขตาย)
        previousMinionCount = currentMinionCount;

        return selectedSkill;
    }

    /// <summary>
    /// ฟังก์ชันนี้เอาไว้ให้ระบบคำนวณดาเมจ (เช่น ใน Skill หรือ Effect) ดึงไปคูณกับดาเมจพื้นฐาน
    /// </summary>
    public float GetDamageMultiplier()
    {
        return currentDamageMultiplier;
    }
}