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
            // ทำแจ้งเตือนสีแดงตัวหนาเวลาของเต็ม
            Debug.LogWarning("<b><color=#FF5555>⚠️ กระเป๋าเต็มแล้ว!</color></b> ไม่สามารถเก็บไอเทมเพิ่มได้");
            return false;
        }
        
        database.items.Add(item);

        // 1. ตั้งค่าสีตามระดับความหายาก (Rarity)
        string rarityColor = "#FFFFFF"; // สีขาว (Normal)
        switch (item.Rarity)
        {
            case EquipmentRarity.Magic: rarityColor = "#00BFFF"; break; // สีฟ้า
            case EquipmentRarity.Rare: rarityColor = "#FFD700"; break;  // สีทอง
        }

        // 2. จัดรูปแบบ Affix แต่ละอันให้อ่านง่าย
        string affixDetails = "";
        if (item.Affixes.Count > 0)
        {
            List<string> affixList = new List<string>();
            foreach (var affix in item.Affixes)
            {
                string sign = affix.Value >= 0 ? "+" : "";
                string percentMark = affix.Type == ModifierType.Percent ? "%" : "";
                
                // ถ้าค่าเป็นบวกให้ใช้สีเขียว ถ้าติดลบให้ใช้สีแดง
                string valueColor = affix.Value >= 0 ? "#55FF55" : "#FF5555"; 
                
                // ประกอบร่างข้อความย่อย เช่น <color=#55FF55>+15%</color> ActionSpeed
                string formattedAffix = $"<b><color={valueColor}>{sign}{affix.Value}{percentMark}</color></b> {affix.Stat}";
                affixList.Add(formattedAffix);
            }
            
            // เอามาต่อกัน คั่นด้วยเส้นตรง ( | ) จะดูสะอาดตากว่าลูกน้ำ
            affixDetails = string.Join(" | ", affixList);
        }
        else
        {
            affixDetails = "<color=grey><i>ไม่มีออฟชั่นเสริม</i></color>";
        }

        // 3. ประกอบร่างข้อความ Log หลัก แบ่งเป็น 2 บรรทัดให้ไม่อึดอัด
        string logMessage = $"📦 <b><color=#00FF00>ได้รับไอเทม:</color></b> <b><color=#00FFFF>{item.BaseItem.ItemName}</color></b> " +
                            $"[<b><color={rarityColor}>{item.Rarity}</color></b>]\n" +
                            $"✨ <b>ออฟชั่น ({item.Affixes.Count}):</b> {affixDetails}";

        Debug.Log(logMessage);
        
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