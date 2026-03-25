// using UnityEngine;
// using System.Collections.Generic;

// public class InventoryUIController : MonoBehaviour
// {
//     [Header("Data References")]
//     [Tooltip("ลากระบบกระเป๋าที่มีข้อมูลไอเทมมาใส่")]
//     public InventoryManager inventoryManager;

//     [Header("UI References")]
//     [Tooltip("ลาก GameObject ชื่อ Content ที่อยู่ใน Scroll View มาใส่ตรงนี้")]
//     public Transform contentPanel; 
    
//     [Tooltip("ลาก Prefab ของช่องไอเทม (ที่มีสคริปต์ EquipmentSlotUI) มาใส่ตรงนี้")]
//     public GameObject slotPrefab; 

//     // ฟังก์ชันนี้ให้เรียกใช้ตอน "เปิดหน้าต่างกระเป๋า" หรือ "ตอนได้ของใหม่"
//     [ContextMenu("รีเฟรชหน้ากระเป๋า (Refresh UI)")]
//     public void RefreshInventoryUI()
//     {
//         // 1. ล้างของเก่าใน Scroll View ทิ้งก่อน (ป้องกันการแสดงผลซ้ำซ้อน)
//         foreach (Transform child in contentPanel)
//         {
//             Destroy(child.gameObject);
//         }

//         // 2. ดึงข้อมูลไอเทมทั้งหมดในกระเป๋ามา
//         List<EquipmentInstance> allItems = inventoryManager.GetAllEquipments();

//         // 3. วนลูปสร้าง Prefab ลงใน Scroll View ทีละชิ้น
//         foreach (EquipmentInstance item in allItems)
//         {
//             // สร้าง GameObject ใหม่ โดยให้ไปอยู่ใต้ contentPanel
//             GameObject newSlotObj = Instantiate(slotPrefab, contentPanel, false);

//             // ดึงสคริปต์ EquipmentSlotUI ที่เราเขียนไว้ก่อนหน้านี้มาทำงาน
//             EquipmentSlotUI slotUI = newSlotObj.GetComponent<EquipmentSlotUI>();
//             if (slotUI != null)
//             {
//                 // ส่งข้อมูลไอเทมชิ้นนี้ ไปให้ UI จัดการแสดงรูปและสีกรอบ
//                 slotUI.SetupSlot(item);
//             }
//         }
//     }
// }


using UnityEngine;
using System.Collections.Generic;

public class InventoryUIController : MonoBehaviour
{
    [Header("Data References")]
    public InventoryManager inventoryManager;

    [Header("UI References")]
    public Transform contentPanel; 
    public GameObject slotPrefab; 

    // เพิ่มตัวแปรเก็บว่าตอนนี้กำลังดูแท็บไหนอยู่ (เผื่อกด Refresh จะได้อยู่ที่แท็บเดิม)
    private EquipmentSlot currentActiveSlot = EquipmentSlot.Helmet;

    // ==========================================
    // ฟังก์ชันใหม่: สำหรับให้ปุ่ม UI เรียกใช้
    // ==========================================
    public void FilterBySlot(int slotIndex)
    {
        // แปลงตัวเลข int ให้กลับเป็น Enum EquipmentSlot
        currentActiveSlot = (EquipmentSlot)slotIndex;

        // เรียกฟังก์ชันวาด UI
        UpdateUI();
    }

    // แก้ไขฟังก์ชันรีเฟรช ให้มาเรียก UpdateUI แทน
    [ContextMenu("รีเฟรชหน้ากระเป๋า (Refresh UI)")]
    public void RefreshInventoryUI()
    {
        UpdateUI();
    }

    // ฟังก์ชันหลักที่ใช้วาด UI ลงใน Scroll View
    private void UpdateUI()
    {
        // 1. ล้างของเก่าทิ้ง
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // 2. ดึงของมา **เฉพาะประเภทที่เลือก (currentActiveSlot)**
        List<EquipmentInstance> filteredItems = inventoryManager.GetEquipmentsBySlot(currentActiveSlot);

        // 3. วาด UI ใหม่
        foreach (EquipmentInstance item in filteredItems)
        {
            GameObject newSlotObj = Instantiate(slotPrefab, contentPanel, false);
            EquipmentSlotUI slotUI = newSlotObj.GetComponent<EquipmentSlotUI>();
            if (slotUI != null)
            {
                slotUI.SetupSlot(item);
            }
        }
    }
}