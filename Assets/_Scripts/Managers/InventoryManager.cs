using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int MaxEquipmentCapacity = 50;

    [Header("Items Stored")]
    [SerializeField] 
    private List<EquipmentInstance> equipmentInventory = new List<EquipmentInstance>();

    // ==========================================
    // ส่วนที่เพิ่มใหม่: สำหรับ Debug และ Mock Test
    // ==========================================
    [Header("Debug & Testing")]
    [Tooltip("ลาก BaseEquipment ที่สร้างไว้มาใส่ตรงนี้เพื่อใช้สุ่มเทส")]
    public List<BaseEquipment> testTemplates;

    // คำสั่ง [ContextMenu] จะทำให้มีเมนูโผล่มาตอนคลิกขวาที่ชื่อคอมโพเนนต์ใน Inspector
    [ContextMenu("เสกไอเทมทดสอบ (Mock Item)")]
    public void GenerateMockItem()
    {
        // 1. เช็คก่อนว่าไม่ได้ลืมใส่เทมเพลตไว้
        if (testTemplates == null || testTemplates.Count == 0)
        {
            Debug.LogWarning("กรุณาลาก BaseEquipment มาใส่ในช่อง Test Templates ก่อนครับ!");
            return;
        }

        // 2. สุ่มหยิบเทมเพลตมา 1 ชิ้นจากที่ใส่ไว้
        BaseEquipment randomTemplate = testTemplates[Random.Range(0, testTemplates.Count)];
        
        // 3. สั่งโรงงานผลิตไอเทม
        EquipmentInstance mockItem = EquipmentGenerator.GenerateDrop(randomTemplate);
        
        // 4. โยนเข้ากระเป๋า
        AddEquipment(mockItem);
    }
    // ==========================================

    // ฟังก์ชันจัดการกระเป๋า (ของเดิม)
    public bool AddEquipment(EquipmentInstance item)
    {
        if (equipmentInventory.Count >= MaxEquipmentCapacity)
        {
            Debug.LogWarning("กระเป๋า Equipment เต็มแล้ว!");
            return false;
        }
        equipmentInventory.Add(item);
        Debug.Log($"[Mock] ได้รับ: {item.BaseItem.ItemName} | Rarity: {item.Rarity} | ออฟชั่น: {item.Affixes.Count} อย่าง");
        return true;
    }

    public void RemoveEquipment(EquipmentInstance item)
    {
        if (equipmentInventory.Contains(item)) equipmentInventory.Remove(item);
    }

    public List<EquipmentInstance> GetAllEquipments() => equipmentInventory;

    public List<EquipmentInstance> GetEquipmentsBySlot(EquipmentSlot slot)
    {
        List<EquipmentInstance> filteredList = new List<EquipmentInstance>();
        foreach (var item in equipmentInventory)
        {
            if (item.BaseItem.Slot == slot) filteredList.Add(item);
        }
        return filteredList;
    }
}