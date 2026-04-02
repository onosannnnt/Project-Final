using System.Collections.Generic;
using UnityEngine;

// 1. สร้างคลาสเล็กๆ เพื่อจับคู่ "ช่อง" กับ "ไอเทม" (เพราะ Unity เซฟ Dictionary ตรงๆ ไม่ได้)
[System.Serializable]
public class EquippedRecord
{
    public CharacterSlot slot;
    public EquipmentInstance equipment;
}

[CreateAssetMenu(fileName = "PlayerInventoryData", menuName = "Inventory/Inventory Data")]
public class InventoryData : ScriptableObject
{
    [Header("Items Stored (ของในกระเป๋า)")]
    public List<EquipmentInstance> items = new List<EquipmentInstance>();

    // 2. เพิ่ม List สำหรับจำว่าช่องไหนใส่อะไรอยู่
    [Header("Equipped Items (ของที่ใส่อยู่)")]
    public List<EquippedRecord> equippedItems = new List<EquippedRecord>();
}