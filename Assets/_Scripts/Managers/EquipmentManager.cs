using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public EntitiesBaseStat characterBaseStat; 
    public InventoryManager inventoryManager;
    
    [Header("Data Storage")]
    [Tooltip("ลากไฟล์ PlayerInventoryData ตัวเดียวกับกระเป๋ามาใส่ตรงนี้")]
    public InventoryData database; 

    public event Action OnEquipmentChanged;

    private Dictionary<CharacterSlot, EquipmentInstance> equippedItems = new Dictionary<CharacterSlot, EquipmentInstance>();

    // ==========================================
    // โหลดข้อมูลตอนเริ่มเกม
    // ==========================================
    private void Awake()
    {
        // พอเริ่มเกมปุ๊บ ให้ดึงข้อมูลจากไฟล์เซฟ มาใส่ใน Dictionary ทันที
        equippedItems.Clear();
        if (database != null)
        {
            foreach (var record in database.equippedItems)
            {
                if (record.equipment != null)
                {
                    equippedItems[record.slot] = record.equipment;
                }
            }
        }
    }

    // ==========================================
    // ระบบใส่ของ (เพิ่มคำสั่ง Save)
    // ==========================================
    public void Equip(EquipmentInstance newItem)
    {
        if (newItem == null) return;
        CharacterSlot targetSlot = GetTargetSlot(newItem);

        if (equippedItems.ContainsKey(targetSlot) && equippedItems[targetSlot] != null)
        {
            Unequip(targetSlot);
        }

        equippedItems[targetSlot] = newItem;
        
        // 🌟 เซฟข้อมูลลงไฟล์ทันที!
        SaveToDatabase();

        Debug.Log($"สวมใส่: {newItem.BaseItem.ItemName} ที่ช่อง {targetSlot}");
        OnEquipmentChanged?.Invoke();
    }

    // ==========================================
    // ระบบถอดของ (เพิ่มคำสั่ง Save)
    // ==========================================
    public void Unequip(CharacterSlot slot)
    {
        if (equippedItems.ContainsKey(slot))
        {
            equippedItems.Remove(slot); 
            
            // 🌟 เซฟข้อมูลลงไฟล์ทันที!
            SaveToDatabase();
            
            OnEquipmentChanged?.Invoke();
        }
    }

    public void UnequipByItem(EquipmentInstance item)
    {
        foreach (var kvp in equippedItems)
        {
            if (kvp.Value == item)
            {
                Unequip(kvp.Key);
                break; 
            }
        }
    }

    // ==========================================
    // ฟังก์ชันผู้ช่วย: ดันข้อมูลจาก Dictionary กลับไปที่ไฟล์เซฟ
    // ==========================================
    private void SaveToDatabase()
    {
        if (database == null) return;

        database.equippedItems.Clear();
        foreach (var kvp in equippedItems)
        {
            database.equippedItems.Add(new EquippedRecord { slot = kvp.Key, equipment = kvp.Value });
        }
    }

    // ==========================================
    // ฟังก์ชันอื่นๆ (ปล่อยไว้เหมือนเดิมครับ)
    // ==========================================
    private CharacterSlot GetTargetSlot(EquipmentInstance item)
    {
        if (item.BaseItem.Slot == EquipmentSlot.Ring)
        {
            if (!equippedItems.ContainsKey(CharacterSlot.LeftRing)) return CharacterSlot.LeftRing;
            if (!equippedItems.ContainsKey(CharacterSlot.RightRing)) return CharacterSlot.RightRing;
            return CharacterSlot.LeftRing; 
        }

        switch (item.BaseItem.Slot)
        {
            case EquipmentSlot.Helmet: return CharacterSlot.Helmet;
            case EquipmentSlot.BodyArmor: return CharacterSlot.Armor;
            case EquipmentSlot.Boot: return CharacterSlot.Boot;
            case EquipmentSlot.Mainhand: return CharacterSlot.MainHand;
            case EquipmentSlot.Offhand: return CharacterSlot.Offhand;
            default: return CharacterSlot.Helmet;
        }
    }

    public bool IsEquipped(EquipmentInstance item)
    {
        return equippedItems.ContainsValue(item);
    }

    public EquipmentInstance GetEquippedItem(CharacterSlot slot)
    {
        if (equippedItems.ContainsKey(slot)) return equippedItems[slot];
        return null;
    }

    public float GetTotalStat(StatType statType)
    {
        float baseValue = characterBaseStat.GetCalculatedStat(statType);
        float flatBonus = 0f;
        float percentBonus = 0f;

        foreach (var item in equippedItems.Values)
        {
            if (item == null) continue;
            if (item.BaseItem.BaseStatType == statType) flatBonus += item.BaseItem.BaseValue;

            foreach (var affix in item.Affixes)
            {
                if (affix.Stat == statType)
                {
                    if (affix.Type == ModifierType.Flat) flatBonus += affix.Value;
                    else if (affix.Type == ModifierType.Percent) percentBonus += affix.Value;
                }
            }
        }
        return (baseValue + flatBonus) * (1f + percentBonus);
    }

    [ContextMenu("เช็คสเตตัส HP และ ATK ตอนนี้")]
    public void DebugCheckStats()
    {
        float totalHP = GetTotalStat(StatType.MaxHealth);
        float totalATK = GetTotalStat(StatType.PhysicalAttack);
        float totalSpeed = GetTotalStat(StatType.ActionSpeed);

        // รวบเป็น String ก้อนเดียว ใช้ \n ในการขึ้นบรรทัดใหม่ และตกแต่งสีสันให้ชัดเจน
        string statLog = "\n" + // ขึ้นบรรทัดใหม่หลบเวลา Console มันจัดบรรทัดติดกันเกินไป
                         "<b><color=#FFD700>=== 📊 สเตตัสตัวละคร (รวมสวมใส่) ===</color></b>\n" +
                         $"❤️ <b><color=#FF5555>Max HP:</color></b>      <color=white>{totalHP}</color>\n" +
                         $"⚔️ <b><color=#55FF55>Physical ATK:</color></b>  <color=white>{totalATK}</color>\n" +
                         $"⚡ <b><color=#00FFFF>Action Speed:</color></b>  <color=white>{totalSpeed}</color>";

        Debug.Log(statLog);
    }
}