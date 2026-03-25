using UnityEngine;

// สร้างเมนูให้คลิกขวาสร้างไฟล์ตั้งค่านี้ได้
[CreateAssetMenu(fileName = "RarityVisualSettings", menuName = "Settings/Rarity Visual Settings")]
public class RarityVisualSettings : ScriptableObject
{
    [Header("Rarity Frames (กรอบไอเทม)")]
    public Sprite NormalFrame; // เช่น กรอบเทา/น้ำตาล
    public Sprite MagicFrame;  // เช่น กรอบฟ้า/ม่วง
    public Sprite RareFrame;   // เช่น กรอบทอง

    // ฟังก์ชันสำหรับดึงกรอบไปใช้ แค่โยน Rarity เข้ามา มันจะคืนค่ารูปกรอบกลับไปให้
    public Sprite GetFrameByRarity(EquipmentRarity rarity)
    {
        switch (rarity)
        {
            case EquipmentRarity.Normal: return NormalFrame;
            case EquipmentRarity.Magic: return MagicFrame;
            case EquipmentRarity.Rare: return RareFrame;
            default: return NormalFrame;
        }
    }
}