using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour
{
    [Header("UI References (ลาก UI Image มาใส่)")]
    public Image frameImage; // ลาก Image ที่เป็นพื้นหลัง/กรอบมาใส่
    public Image itemIcon;   // ลาก Image ที่อยู่ตรงกลาง (รูปรองเท้า/หมวก) มาใส่

    [Header("Global Settings")]
    [Tooltip("ลากไฟล์ GlobalRaritySettings ที่เราเพิ่งสร้างมาใส่ตรงนี้")]
    public RarityVisualSettings visualSettings;

    public void SetupSlot(EquipmentInstance equipment)
    {
        // if (equipment == null)
        // {
        //     // ถ้าไม่มีของใส่ ให้ปิดรูปทิ้งไป
        //     itemIcon.gameObject.SetActive(false);
        //     frameImage.sprite = visualSettings.NormalFrame; // กลับเป็นกรอบเปล่าๆ
        //     return;
        // }

        // 1. เปิดการแสดงผลรูปรถ/หมวก
        itemIcon.gameObject.SetActive(true);
        itemIcon.sprite = equipment.BaseItem.Icon; // ดึงรูป "รองเท้า/หมวก" จาก BaseEquipment มาใส่

        // 2. เปลี่ยนสีกรอบตามระดับความหายาก (Rarity)
        frameImage.sprite = visualSettings.GetFrameByRarity(equipment.Rarity); // ดึง "กรอบทอง/ม่วง" มาใส่
    }
}