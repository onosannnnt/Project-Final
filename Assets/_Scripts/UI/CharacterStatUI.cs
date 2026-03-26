using UnityEngine;
using TMPro; // ใช้สำหรับ UI Text
using UnityEngine.UI;

public class CharacterStatUI : MonoBehaviour
{
    [Header("Manager Reference")]
    public EquipmentManager equipmentManager;

    [Header("Basic Information UI")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI expText; // เอาไว้โชว์ 4,900 / 6,000

    [Tooltip("ลาก Image หลอด EXP สีเหลืองมาใส่ตรงนี้ (ต้องตั้ง Image Type เป็น Filled ด้วยนะ)")]
    public Image expFillBar;

    [Header("Core Attributes UI")]
    public TextMeshProUGUI strengthText;
    public TextMeshProUGUI intelligenceText;
    public TextMeshProUGUI agilityText;

    [Header("Base Stats UI")]
    public TextMeshProUGUI maxHealthText;
    public TextMeshProUGUI maxSkillPointText;
    public TextMeshProUGUI physicalAttackText;
    public TextMeshProUGUI magicAttackText;
    public TextMeshProUGUI armourText;
    public TextMeshProUGUI actionSpeedText;
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI evasionRateText;

    [Header("Multiplier & Percentage Stats UI (โชว์เป็น %)")]
    public TextMeshProUGUI fireDamageMultiplierText;
    public TextMeshProUGUI coldDamageMultiplierText;
    public TextMeshProUGUI lightningDamageMultiplierText;
    public TextMeshProUGUI fireResistanceText;
    public TextMeshProUGUI coldResistanceText;
    public TextMeshProUGUI lightningResistanceText;
    public TextMeshProUGUI criticalHitChanceText;
    public TextMeshProUGUI criticalHitDamageMultiplierText;
    public TextMeshProUGUI statusEffectResistanceText;
    public TextMeshProUGUI statusHitChanceText;

    private void Start()
    {
        if (equipmentManager != null)
        {
            equipmentManager.OnEquipmentChanged += UpdateAllStatUI;
            UpdateAllStatUI(); // อัปเดตทันทีตอนเริ่มเกม
        }
    }

    private void OnDestroy()
    {
        if (equipmentManager != null)
        {
            equipmentManager.OnEquipmentChanged -= UpdateAllStatUI;
        }
    }

    [ContextMenu("รีเฟรชสเตตัสบน UI (Refresh UI)")]
    public void UpdateAllStatUI()
    {
        if (equipmentManager == null || equipmentManager.characterBaseStat == null) return;

        // ดึง Base Stat ตัวจริงมาใช้อ้างอิง (สำหรับพวกชื่อ เลเวล EXP ที่ไม่ได้รับผลจากชุดสวมใส่)
        var baseStat = equipmentManager.characterBaseStat;

        // =====================================
        // 1. Basic Information (ไม่รวมของสวมใส่)
        // =====================================
        if (nameText != null) nameText.text = baseStat.entityName;
        if (levelText != null) levelText.text = $"Level {baseStat.Level}";

        if (expText != null) 
        {
            expText.text = $"{baseStat.ExperiencePoints:N0} / {baseStat.MaxExperiencePoints:N0}";
        }

        // 🌟 จัดการหลอดภาพ (Percentage)
        if (expFillBar != null)
        {
            // ดัก Error ไว้ก่อน: ถ้า Max EXP เป็น 0 (เช่น เวลตัน) จะได้ไม่เกิดบั๊กหารด้วยศูนย์
            float maxExp = baseStat.MaxExperiencePoints > 0 ? baseStat.MaxExperiencePoints : 1;
            
            // คำนวณหา % (ค่าจะได้ออกมาเป็น 0.0f ถึง 1.0f)
            float expPercent = baseStat.ExperiencePoints / maxExp;
            
            // สั่งให้หลอดภาพแสดงผลตาม % ที่คำนวณได้
            expFillBar.fillAmount = expPercent;
        }
        
        // ใช้ :N0 เพื่อให้มีลูกน้ำคั่นหลักพัน เช่น 4,900 / 6,000
        if (expText != null) expText.text = $"{baseStat.ExperiencePoints:N0} / {baseStat.MaxExperiencePoints:N0}";

        // =====================================
        // 2. Core Attributes (ดึงจาก EquipmentManager เผื่อมีของสวมใส่ +STR, INT, AGI)
        // =====================================
        if (strengthText != null) strengthText.text = equipmentManager.GetTotalStat(StatType.Strength).ToString("F0");
        if (intelligenceText != null) intelligenceText.text = equipmentManager.GetTotalStat(StatType.Intelligence).ToString("F0");
        if (agilityText != null) agilityText.text = equipmentManager.GetTotalStat(StatType.Agility).ToString("F0");

        // =====================================
        // 3. Base Stats (ตัวเลขปกติ)
        // =====================================
        if (maxHealthText != null) maxHealthText.text = equipmentManager.GetTotalStat(StatType.MaxHealth).ToString("F0");
        // หมายเหตุ: ถ้าไม่มี Enum StatType.MaxSkillPoint ให้ดึงจาก baseStat.MaxSkillPoint.ToString() แทนได้ครับ
        if (maxSkillPointText != null) maxSkillPointText.text = equipmentManager.GetTotalStat(StatType.MaxSkillPoint).ToString("F0");
        
        if (physicalAttackText != null) physicalAttackText.text = equipmentManager.GetTotalStat(StatType.PhysicalAttack).ToString("F0");
        if (magicAttackText != null) magicAttackText.text = equipmentManager.GetTotalStat(StatType.MagicAttack).ToString("F0");
        if (armourText != null) armourText.text = equipmentManager.GetTotalStat(StatType.Armour).ToString("F0");
        if (actionSpeedText != null) actionSpeedText.text = equipmentManager.GetTotalStat(StatType.ActionSpeed).ToString("F0");
        if (accuracyText != null) accuracyText.text = equipmentManager.GetTotalStat(StatType.Accuracy).ToString("F0");
        if (evasionRateText != null) evasionRateText.text = equipmentManager.GetTotalStat(StatType.EvasionRate).ToString("F0");

        // =====================================
        // 4. Multiplier & Percentages (คูณ 100 และเติม %)
        // =====================================
        // F1 คือโชว์ทศนิยม 1 ตำแหน่ง (เช่น 15.5%) ถ้าอยากได้เลขกลมๆ ให้แก้เป็น F0 ครับ
        if (fireDamageMultiplierText != null) fireDamageMultiplierText.text = (equipmentManager.GetTotalStat(StatType.FireDamageMultiplier) * 100).ToString("F0") + "%";
        if (coldDamageMultiplierText != null) coldDamageMultiplierText.text = (equipmentManager.GetTotalStat(StatType.ColdDamageMultiplier) * 100).ToString("F0") + "%";
        if (lightningDamageMultiplierText != null) lightningDamageMultiplierText.text = (equipmentManager.GetTotalStat(StatType.LightningDamageMultiplier) * 100).ToString("F0") + "%";
        
        if (fireResistanceText != null) fireResistanceText.text = (equipmentManager.GetTotalStat(StatType.FireResistance) * 100).ToString("F0") + "%";
        if (coldResistanceText != null) coldResistanceText.text = (equipmentManager.GetTotalStat(StatType.ColdResistance) * 100).ToString("F0") + "%";
        if (lightningResistanceText != null) lightningResistanceText.text = (equipmentManager.GetTotalStat(StatType.LightningResistance) * 100).ToString("F0") + "%";
        
        if (criticalHitChanceText != null) criticalHitChanceText.text = (equipmentManager.GetTotalStat(StatType.CriticalHitChance) * 100).ToString("F1") + "%";
        if (criticalHitDamageMultiplierText != null) criticalHitDamageMultiplierText.text = (equipmentManager.GetTotalStat(StatType.CriticalDamageMultiplier) * 100).ToString("F0") + "%";
        
        // Status Resistance ไม่ได้เขียนว่า divide by 100 ผมเลยให้ค่าดั้งเดิมโชว์ % เลย
        if (statusEffectResistanceText != null) statusEffectResistanceText.text = equipmentManager.GetTotalStat(StatType.StatusEffectResistance).ToString("F0") + "%";
        if (statusHitChanceText != null) statusHitChanceText.text = equipmentManager.GetTotalStat(StatType.StatusHitChance).ToString("F0") + "%";
    }
}