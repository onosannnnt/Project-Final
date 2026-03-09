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
        return CreateInstance<Item>();
    }
}
