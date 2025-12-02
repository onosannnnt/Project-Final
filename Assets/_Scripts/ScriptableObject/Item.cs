using UnityEngine;

public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
    public EquipmentType equipmentType;
    public Rarity rarity;
    public int maxStack = 1;
    public string description;
    public float AmountMultiplier;
    public float AmountFlat;

    public Item Clone()
    {
        Item cloneItem = CreateInstance<Item>();
        cloneItem.itemName = itemName;
        cloneItem.icon = icon;
        cloneItem.itemType = itemType;
        cloneItem.equipmentType = equipmentType;
        cloneItem.rarity = rarity;
        cloneItem.maxStack = maxStack;
        cloneItem.description = description;
        cloneItem.AmountFlat = AmountFlat;
        cloneItem.AmountMultiplier = AmountMultiplier;
        return cloneItem;
    }
}
