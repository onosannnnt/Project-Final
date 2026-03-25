using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Data Storage (เก็บข้อมูลข้าม Play Mode)")]
    [Tooltip("ลากไฟล์ PlayerInventoryData มาใส่ตรงนี้")]
    public InventoryData database; 

    [Header("Inventory Settings")]
    public int MaxEquipmentCapacity = 50;

    [Header("Debug & Testing")]
    public List<BaseEquipment> testTemplates;

    // ==========================================
    // ฟังก์ชันใหม่: เคลียร์กระเป๋า
    // ==========================================
    [ContextMenu("เคลียร์กระเป๋า (Clear All)")]
    public void ClearAllEquipment()
    {
        if (database != null)
        {
            database.items.Clear();
            Debug.Log("เทกระจาด! ลบไอเทมทั้งหมดในกระเป๋าเรียบร้อยแล้ว");
        }
    }

    [ContextMenu("เสกไอเทมทดสอบ (Mock Item)")]
    public void GenerateMockItem()
    {
        if (testTemplates == null || testTemplates.Count == 0) return;
        
        BaseEquipment randomTemplate = testTemplates[Random.Range(0, testTemplates.Count)];
        EquipmentInstance mockItem = EquipmentGenerator.GenerateDrop(randomTemplate);
        AddEquipment(mockItem);
    }

    // ==========================================
    // ฟังก์ชันจัดการกระเป๋า (เปลี่ยนมาใช้ database.items แทน)
    // ==========================================
    public bool AddEquipment(EquipmentInstance item)
    {
        if (database.items.Count >= MaxEquipmentCapacity)
        {
            Debug.LogWarning("กระเป๋าเต็มแล้ว!");
            return false;
        }
        database.items.Add(item);
        Debug.Log($"ได้รับ: {item.BaseItem.ItemName} | Rarity: {item.Rarity}");
        return true;
    }

    public void RemoveEquipment(EquipmentInstance item)
    {
        if (database.items.Contains(item)) database.items.Remove(item);
    }

    public List<EquipmentInstance> GetAllEquipments() => database.items;

    public List<EquipmentInstance> GetEquipmentsBySlot(EquipmentSlot slot)
    {
        List<EquipmentInstance> filteredList = new List<EquipmentInstance>();
        foreach (var item in database.items)
        {
            if (item.BaseItem.Slot == slot) filteredList.Add(item);
        }
        return filteredList;
    }
}