using UnityEngine;
using System.Collections.Generic; // อย่าลืม using System.Collections.Generic นะครับ

public enum EquipmentSlot
{
    Helmet,
    BodyArmor,
    Ring,
    Boot,
    Offhand,
    Mainhand
}

public enum CharacterSlot
{
    Helmet,
    Armor,
    LeftRing,   // วงซ้าย
    RightRing,  // วงขวา
    Boot,
    Offhand,
    MainHand
}

public enum EquipmentRarity
{
    Normal,
    Magic,
    Rare
}

[CreateAssetMenu(fileName = "NewBaseEquipment", menuName = "Items/BaseEquipment")]
public class BaseEquipment : ScriptableObject
{
    [Header("Item Info")]
    public string ItemName;
    public Sprite Icon;
    public EquipmentSlot Slot; 

    [Header("Base Stat")]
    public StatType BaseStatType; 
    public float BaseValue;

    [Header("Fixed Item Settings")]
    [Tooltip("ติ๊กถูกถ้าไอเทมชิ้นนี้เป็นของ Unique หรือของเควสต์ที่ล็อคออฟชั่นตายตัว")]
    public bool IsFixedItem;
    
    [Tooltip("ระดับความหายากของไอเทมชิ้นนี้ (ใช้เมื่อ IsFixedItem = true)")]
    public EquipmentRarity FixedRarity = EquipmentRarity.Rare;
    
    [Tooltip("รายการออฟชั่นที่ล็อคไว้ จะแสดงผลในเกมตามนี้เป๊ะๆ")]
    public List<StatModifier> FixedAffixes;
}