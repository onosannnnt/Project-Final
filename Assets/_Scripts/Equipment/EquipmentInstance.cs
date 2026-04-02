using System.Collections.Generic;

[System.Serializable]
public class EquipmentInstance
{
    public BaseEquipment BaseItem;
    public EquipmentRarity Rarity;
    public List<StatModifier> Affixes;

    public EquipmentInstance(BaseEquipment baseItem, EquipmentRarity rarity)
    {
        BaseItem = baseItem;
        Rarity = rarity;
        Affixes = new List<StatModifier>();
    }

    public int GetMaxAffixes()
    {
        switch (Rarity)
        {
            case EquipmentRarity.Normal: 
                return 2;
            case EquipmentRarity.Magic: 
                return 4;
            case EquipmentRarity.Rare: 
                return 6;
            default: 
                return 2;
        }
    }
}