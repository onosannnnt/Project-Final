using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PuzzleBossCombat : BossCombat
{
    [Header("Boss Buffs")]
    public Buff fireBuff;
    public Buff frostBuff;
    public Buff lightningBuff;
    public Buff windBuff;

    [Header("Boss Visuals")]
    [Tooltip("ลาก Component Sprite Renderer ของตัวบอสมาใส่ช่องนี้")]
    public SpriteRenderer bossSpriteRenderer;

    [Tooltip("รูปร่างปกติ (มี 4 บัฟ)")]
    public Sprite defaultSprite;
    public Sprite fireSprite;
    public Sprite frostSprite;
    public Sprite lightningSprite;
    public Sprite windSprite;

    [Header("Boss Skills")]
    public Skill summonPillarsSkill;
    public Skill resetPillarsSkill;

    private bool hasSummonedPillars = false;

    [Header("Pillar Spawning (Dynamic)")]
    public ElementalPillar pillarPrefab;
    private ElementalPillar[] pillars = new ElementalPillar[4];

    // สถานะภายในบอส
    private List<List<DamageElement>> masterPatterns = new List<List<DamageElement>>();
    private HashSet<DamageElement> completedElements = new HashSet<DamageElement>();

    private float vulnerabilityMultiplier = 1.0f; // เริ่มที่ 1.0 (รับดาเมจ 100%)
    private int stunTimer = 0;
    private bool needsToCastResetSkill = false;
    private int turnCounter = 0;

    protected override void Awake()
    {
        base.Awake();
        // กำหนด Pattern มาตรฐาน 1-4 (ใช้ Frost ตามระบบ DamageElement ของเกมคุณ)
        masterPatterns.Add(new List<DamageElement> { DamageElement.Fire, DamageElement.Frost, DamageElement.Lightning, DamageElement.Wind });
        masterPatterns.Add(new List<DamageElement> { DamageElement.Frost, DamageElement.Wind, DamageElement.Fire, DamageElement.Lightning });
        masterPatterns.Add(new List<DamageElement> { DamageElement.Lightning, DamageElement.Fire, DamageElement.Wind, DamageElement.Frost });
        masterPatterns.Add(new List<DamageElement> { DamageElement.Wind, DamageElement.Lightning, DamageElement.Frost, DamageElement.Fire });
    }

    protected override void MarkAsDead()
    {
        base.MarkAsDead();
        // เมื่อบอสตาย ต้องเคลียร์เสาออกด้วยเพื่อให้ TurnManager รู้ว่าศัตรูหมดแล้ว
        if (pillars != null)
        {
            foreach (var pillar in pillars)
            {
                if (pillar != null)
                {
                    // ทำให้เสา "ตาย" ในทางเทคนิค (isDead = true, SetActive(false))
                    // เราสามารถเรียกกระบวนการ Die ของมันได้เลย
                    pillar.gameObject.SetActive(false);
                    // เราต้องเข้าถึง isDead ของมัน หรือทำให้มันโดนทำลาย
                    Destroy(pillar.gameObject);
                }
            }
        }
    }

    private void OnDestroy()
    {
        // ทำความสะอาดเสาถ้าสคริปต์โดนทำลาย (เผื่อไว้)
        if (pillars != null)
        {
            foreach (var pillar in pillars)
            {
                if (pillar != null)
                {
                    Destroy(pillar.gameObject);
                }
            }
        }
    }

    // ==========================================
    // 1. ระบบอัญเชิญเสา (Summon Pillars)
    // ==========================================
    public void ActivatePillars()
    {
        Debug.Log("🔮 บอสใช้สกิล: อัญเชิญเสา 4 ธาตุ!");

        if (pillarPrefab == null)
        {
            Debug.LogError("ลืมใส่ Pillar Prefab ให้กับบอส!");
            return;
        }

        // ค้นหา worldParent จาก EnemyGenerator เพื่อให้เสาถูกนับรวมใน List ศัตรู (สำหรับการทำ AoE)
        Transform worldParent = null;
        if (EnemyGenerator.Instance != null)
        {
            // ใช้ Reflection หรือเข้าถึง field ตรงๆ ถ้าเป็นไปได้ แต่ในที่นี้เราเห็นว่ามันมี field worldParent
            // เนื่องจาก worldParent เป็น private ใน EnemyGenerator (สมมติ) 
            // แต่ TurnManager ก็มี worldParent ที่เป็น public หรือเข้าถึงได้
            if (TurnManager.Instance != null)
            {
                // เข้าถึง worldParent ผ่าน TurnManager
                // (จากการ grep เราเห็น TurnManager มี [SerializeField] private Transform worldParent)
                // เราต้องใช้ตัวที่ GetAllEnemies() ใช้งาน
                
                // เพื่อความชัวร์ เราจะลองหา Transform ชื่อ "WorldParent" หรือใช้ parent ของบอส
                worldParent = transform.parent;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            // ดึงพิกัดตำแหน่งที่ 1 ถึง 4 (index 0 ถึง 3) จาก EnemyGenerator
            Vector3 spawnPos = EnemyGenerator.Instance.GetSpawnPosition(i);

            // เสกเสาออกมาบนฉากตรงตำแหน่งนั้น
            ElementalPillar newPillar = Instantiate(pillarPrefab, spawnPos, Quaternion.identity);

            // ตั้งชื่อให้ดูง่ายใน Hierarchy
            newPillar.gameObject.name = $"ElementalPillar_{i}";
            
            // สำคัญ: ต้องใส่ Tag "Enemy" เพื่อให้ TurnManager.GetAllEnemies() มองเห็น
            newPillar.gameObject.tag = "Enemy";
            
            // สำคัญ: ต้องเซ็ต Parent ให้อยู่ภายใต้ WorldParent เดียวกับศัตรูตัวอื่น
            if (worldParent != null)
            {
                newPillar.transform.SetParent(worldParent);
            }

            // เก็บเสาที่เพิ่งสร้างลงใน Array
            pillars[i] = newPillar;
        }

        FullResetPhase();
    }

    // ==========================================
    // 2. ระบบเช็ค Puzzle (Matching)
    // ==========================================
    public void OnPillarHit()
    {
        if (stunTimer > 0) return; // ติด Stun อยู่ ไม่สนใจเสา
        if (pillars == null || pillars.Length == 0) return;

        DamageElement targetElement = pillars[0].GetCurrentElement();

        // เช็คว่าเสาทุกต้นมีธาตุตรงกับเสาแรกหรือไม่
        bool isAllMatch = true;
        foreach (var pillar in pillars)
        {
            if (pillar == null || pillar.GetCurrentElement() != targetElement)
            {
                isAllMatch = false;
                break;
            }
        }

        if (isAllMatch)
        {
            ApplyMatchResult(targetElement);
        }
    }

    private void ApplyMatchResult(DamageElement matchedElement)
    {
        if (completedElements.Contains(matchedElement))
        {
            Debug.Log($"<color=orange>เสาเรียงเป็นธาตุ {matchedElement} แต่สะสมไปแล้ว! บอสเมินเฉย</color>");
            return;
        }

        Debug.Log($"<color=cyan>✅ Match! เคลียร์ธาตุใหม่: {matchedElement}</color>");

        // เพิ่มความเปราะบาง 5%
        vulnerabilityMultiplier += 0.05f;
        completedElements.Add(matchedElement);

        // เปลี่ยนธาตุบอสและจัดการบัฟ
        UpdateBossBuffs(matchedElement);

        // เช็คว่าเคลียร์ครบ 4 ธาตุ (ไม่ซ้ำ) แล้วหรือยัง?
        if (completedElements.Count >= 4)
        {
            TriggerStun();
        }
        else
        {
            // ถ้ายังไม่ครบ จองคิวร่ายสกิลรีเซ็ตในเทิร์นหน้า
            needsToCastResetSkill = true;
            Debug.Log("เตรียมตัว: บอสจะร่ายสกิล Reset เสาในเทิร์นถัดไป!");
        }
    }

    public void ExecutePillarReset()
    {
        Debug.Log("🌪️ บอสใช้ท่า: Reset เสา! (สุ่ม Pattern และกระจายธาตุเริ่มต้นให้ไม่ซ้ำกัน)");

        // 1. สุ่มลำดับของ Pattern (เพื่อให้เสาแต่ละต้นมีลำดับธาตุวนลูปไม่เหมือนกัน)
        List<int> patternIndices = new List<int> { 0, 1, 2, 3 };
        for (int i = 0; i < patternIndices.Count; i++)
        {
            int temp = patternIndices[i];
            int randomIndex = Random.Range(i, patternIndices.Count);
            patternIndices[i] = patternIndices[randomIndex];
            patternIndices[randomIndex] = temp;
        }

        // 2. เลือก Index เริ่มต้นให้เสาแต่ละต้นโดยเช็คว่าธาตุที่ได้ "ต้องไม่ซ้ำ" กับเสาต้นก่อนหน้า
        List<DamageElement> usedElements = new List<DamageElement>();

        for (int i = 0; i < pillars.Length; i++)
        {
            if (pillars[i] != null)
            {
                List<DamageElement> pattern = masterPatterns[patternIndices[i]];
                List<int> validOffsets = new List<int>();

                // ค้นหา offset ใน pattern นี้ที่ให้ธาตุที่ยังไม่ถูกใช้งานในรอบนี้
                for (int j = 0; j < pattern.Count; j++)
                {
                    if (!usedElements.Contains(pattern[j]))
                    {
                        validOffsets.Add(j);
                    }
                }

                // สุ่มเลือก offset จากลิสต์ที่ผ่านการกรองแล้วว่าไม่ซ้ำ
                int chosenOffset = 0;
                if (validOffsets.Count > 0)
                {
                    chosenOffset = validOffsets[Random.Range(0, validOffsets.Count)];
                    usedElements.Add(pattern[chosenOffset]);
                }
                else
                {
                    // Fallback (ไม่ควรเกิดขึ้นถ้าจำนวนเสาเท่ากับจำนวนธาตุใน Pattern)
                    chosenOffset = Random.Range(0, pattern.Count);
                }

                pillars[i].ResetPillar(pattern, chosenOffset);
            }
        }
    }

    // ==========================================
    // 3. ระบบจัดการ Buff และ ร่างบอส (Visual)
    // ==========================================
    private void UpdateBossBuffs(DamageElement activeElement)
    {
        // 1. ลบบัฟธาตุทั้งหมดออกก่อน
        RemoveAllElementalBuffs();

        // 2. เปลี่ยนร่างและใส่บัฟเฉพาะธาตุที่ติด
        if (bossSpriteRenderer != null)
        {
            switch (activeElement)
            {
                case DamageElement.Fire:
                    bossSpriteRenderer.sprite = fireSprite;
                    if (fireBuff != null && buffController != null) buffController.AddBuff(fireBuff);
                    break;
                case DamageElement.Frost:
                    bossSpriteRenderer.sprite = frostSprite;
                    if (frostBuff != null && buffController != null) buffController.AddBuff(frostBuff);
                    break;
                case DamageElement.Lightning:
                    bossSpriteRenderer.sprite = lightningSprite;
                    if (lightningBuff != null && buffController != null) buffController.AddBuff(lightningBuff);
                    break;
                case DamageElement.Wind:
                    bossSpriteRenderer.sprite = windSprite;
                    if (windBuff != null && buffController != null) buffController.AddBuff(windBuff);
                    break;
            }
        }
    }

    private void RemoveAllElementalBuffs()
    {
        if (buffController == null) return;

        // ดึงบัฟทั้งหมดออกมาแล้วลบเฉพาะที่ตรงกับ 4 ธาตุ
        List<ActiveBuff> activeBuffs = buffController.GetBuffs();
        if (activeBuffs == null) return;

        // วนลูปถอยหลังเพื่อป้องกัน Error เวลาลบของออกจาก List
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            Buff data = activeBuffs[i].Data;
            if (data == fireBuff || data == frostBuff || data == lightningBuff || data == windBuff)
            {
                data.OnRemove(this, activeBuffs[i]);
                buffController.RemoveBuff(activeBuffs[i]);
            }
        }
    }

    private void TriggerStun()
    {
        Debug.Log("<color=yellow>💥 เคลียร์ครบ 4 ธาตุ! บอสติด Stun 3 เทิร์น!</color>");
        stunTimer = 3;
        completedElements.Clear();

        // กลับเป็นร่างปกติ
        if (bossSpriteRenderer != null && defaultSprite != null)
        {
            bossSpriteRenderer.sprite = defaultSprite;
        }

        // ลบบัฟทุกธาตุออก
        RemoveAllElementalBuffs();
    }

    private void FullResetPhase()
    {
        Debug.Log("เริ่มเฟสใหม่: รีเซ็ตเสาและเรียก 4 บัฟกลับมา");
        ExecutePillarReset();

        // กลับเป็นร่างปกติ
        if (bossSpriteRenderer != null && defaultSprite != null)
        {
            bossSpriteRenderer.sprite = defaultSprite;
        }

        // ลบของเก่าแล้วใส่บัฟทั้ง 4 ธาตุให้บอสพร้อมกัน (โหมดโหดสุด)
        RemoveAllElementalBuffs();
        if (buffController != null)
        {
            if (fireBuff != null) buffController.AddBuff(fireBuff);
            if (frostBuff != null) buffController.AddBuff(frostBuff);
            if (lightningBuff != null) buffController.AddBuff(lightningBuff);
            if (windBuff != null) buffController.AddBuff(windBuff);
        }
    }

    // ==========================================
    // 4. ระบบตัดสินใจ (AI) และ ดาเมจ
    // ==========================================
    public override Skill ChooseNextSkill()
    {
        var skills = skillManager.GetSkills();
        if (skills == null || skills.Count == 0) return null;

        // 1. ท่าเปิดตัว เสกเสา
        if (!hasSummonedPillars && summonPillarsSkill != null)
        {
            hasSummonedPillars = true;
            return summonPillarsSkill;
        }

        // 2. เช็ค Stun
        if (stunTimer > 0)
        {
            stunTimer--;
            Debug.Log($"บอสติด Stun อยู่... (เหลือ {stunTimer} เทิร์น)");

            if (stunTimer == 0)
            {
                Debug.Log("บอสตื่นจาก Stun แล้ว!");
                FullResetPhase(); // บังคับรีเซ็ตเสาและเรียก 4 บัฟกลับมา
            }
            return null; // ข้ามเทิร์น
        }

        // 3. ท่ารีเซ็ตเสา (ถ้าโดนผู้เล่นเคลียร์ 1 ธาตุ)
        if (needsToCastResetSkill && resetPillarsSkill != null)
        {
            needsToCastResetSkill = false;
            return resetPillarsSkill;
        }

        // 4. โจมตีปกติ
        Skill normalAttack = skills[turnCounter % skills.Count];

        // กันเหนียว ไม่ให้โจมตีสุ่มโดนสกิลเสกเสา/สลับเสา
        if (normalAttack == summonPillarsSkill || normalAttack == resetPillarsSkill)
        {
            turnCounter++;
            normalAttack = skills[turnCounter % skills.Count];
        }

        turnCounter++;
        return normalAttack;
    }

    // ฟังก์ชันนี้ให้ Player ดึงไปคูณดาเมจตอนโจมตีบอส
    public float GetVulnerabilityMultiplier()
    {
        return vulnerabilityMultiplier;
    }
}