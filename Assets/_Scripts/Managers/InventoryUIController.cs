using UnityEngine;
using System.Collections.Generic;

public class InventoryUIController : MonoBehaviour
{
    [Header("Data References")]
    [Tooltip("ลากระบบกระเป๋าที่มีข้อมูลไอเทมมาใส่")]
    public InventoryManager inventoryManager;

    [Header("UI References")]
    [Tooltip("ลาก GameObject ชื่อ Content ที่อยู่ใน Scroll View มาใส่ตรงนี้")]
    public Transform contentPanel; 
    
    [Tooltip("ลาก Prefab ของช่องไอเทม (ที่มีสคริปต์ EquipmentSlotUI) มาใส่ตรงนี้")]
    public GameObject slotPrefab; 

    // ฟังก์ชันนี้ให้เรียกใช้ตอน "เปิดหน้าต่างกระเป๋า" หรือ "ตอนได้ของใหม่"
    [ContextMenu("รีเฟรชหน้ากระเป๋า (Refresh UI)")]
    public void RefreshInventoryUI()
    {
        // 1. ล้างของเก่าใน Scroll View ทิ้งก่อน (ป้องกันการแสดงผลซ้ำซ้อน)
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // 2. ดึงข้อมูลไอเทมทั้งหมดในกระเป๋ามา
        List<EquipmentInstance> allItems = inventoryManager.GetAllEquipments();

        // 3. วนลูปสร้าง Prefab ลงใน Scroll View ทีละชิ้น
        foreach (EquipmentInstance item in allItems)
        {
            // สร้าง GameObject ใหม่ โดยให้ไปอยู่ใต้ contentPanel
            GameObject newSlotObj = Instantiate(slotPrefab, contentPanel, false);

            // ดึงสคริปต์ EquipmentSlotUI ที่เราเขียนไว้ก่อนหน้านี้มาทำงาน
            EquipmentSlotUI slotUI = newSlotObj.GetComponent<EquipmentSlotUI>();
            if (slotUI != null)
            {
                // ส่งข้อมูลไอเทมชิ้นนี้ ไปให้ UI จัดการแสดงรูปและสีกรอบ
                slotUI.SetupSlot(item);
            }
        }
    }
}