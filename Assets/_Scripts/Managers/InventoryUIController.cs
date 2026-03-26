using UnityEngine;
using System.Collections.Generic;

public class InventoryUIController : MonoBehaviour
{
    [Header("Data References")]
    public InventoryManager inventoryManager;
    public EquipmentManager equipmentManager; // เพิ่มตัวนี้เข้ามา

    [Header("UI References")]
    public Transform contentPanel; 
    public GameObject slotPrefab; 

    [Header("ช่องใส่ของด้านบน (ลากกล่องบน 6 กล่องมาใส่)")]
    public List<EquippedSlotUI> topEquippedSlots; 

    private EquipmentSlot currentActiveSlot = EquipmentSlot.Helmet;
    private List<EquipmentSlotUI> currentBottomSlots = new List<EquipmentSlotUI>();

    private void Start()
    {
        // สั่งให้กล่องด้านบนทั้ง 6 กล่อง เริ่มทำงานและเชื่อมกับ EquipmentManager
        foreach (var slot in topEquippedSlots)
        {
            slot.Initialize(equipmentManager);
        }

        // เมื่อมีการใส่/ถอดของ ให้รีเฟรชติ๊กถูกของกล่องด้านล่างทั้งหมด
        equipmentManager.OnEquipmentChanged += RefreshAllCheckmarks;
    }

    private void OnDestroy()
    {
        if (equipmentManager != null) equipmentManager.OnEquipmentChanged -= RefreshAllCheckmarks;
    }

    public void FilterBySlot(int slotIndex)
    {
        currentActiveSlot = (EquipmentSlot)slotIndex;
        UpdateUI();
    }

    [ContextMenu("รีเฟรชหน้ากระเป๋า (Refresh UI)")]
    public void RefreshInventoryUI()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        foreach (Transform child in contentPanel) Destroy(child.gameObject);
        currentBottomSlots.Clear();

        List<EquipmentInstance> filteredItems = inventoryManager.GetEquipmentsBySlot(currentActiveSlot);

        foreach (EquipmentInstance item in filteredItems)
        {
            GameObject newSlotObj = Instantiate(slotPrefab, contentPanel, false);
            EquipmentSlotUI slotUI = newSlotObj.GetComponent<EquipmentSlotUI>();
            if (slotUI != null)
            {
                // ส่ง EquipmentManager ไปให้ปุ่มด้านล่างรู้จักด้วย
                slotUI.SetupSlot(item, equipmentManager);
                currentBottomSlots.Add(slotUI);
            }
        }
    }

    // ฟังก์ชันสั่งอัปเดตติ๊กถูก
    private void RefreshAllCheckmarks()
    {
        foreach (var slotUI in currentBottomSlots)
        {
            slotUI.RefreshCheckmark();
        }
    }
}