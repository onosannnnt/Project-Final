using System.Collections.Generic;
using UnityEngine;

public static class EquipmentGenerator
{
    public static EquipmentInstance GenerateDrop(BaseEquipment baseItem)
    {
        // ==========================================
        // ส่วนที่ 1: ถ้าเป็นไอเทมล็อคผลจาก Editor (Unique/Quest)
        // ==========================================
        if (baseItem.IsFixedItem)
        {
            EquipmentInstance fixedItem = new EquipmentInstance(baseItem, baseItem.FixedRarity);

            foreach (StatModifier affix in baseItem.FixedAffixes)
            {
                fixedItem.Affixes.Add(new StatModifier { 
                    Stat = affix.Stat, 
                    Value = affix.Value, 
                    Type = affix.Type 
                });
            }

            return fixedItem;
        }

        // ==========================================
        // ส่วนที่ 2: ถ้าเป็นไอเทมปกติ ให้สุ่ม (ระบบเดิม)
        // ==========================================
        EquipmentRarity rolledRarity = RollRarity();
        EquipmentInstance randomItem = new EquipmentInstance(baseItem, rolledRarity);

        int maxAffixes = randomItem.GetMaxAffixes();
        int actualAffixCount = Random.Range(1, maxAffixes + 1); 

        for (int i = 0; i < actualAffixCount; i++)
        {
            StatModifier randomAffix = GetRandomAffix();
            randomItem.Affixes.Add(randomAffix);
        }

        return randomItem;
    }

    private static EquipmentRarity RollRarity()
    {
        float chance = Random.Range(0f, 100f);
        if (chance <= 10f) return EquipmentRarity.Rare;      
        if (chance <= 40f) return EquipmentRarity.Magic;     
        return EquipmentRarity.Normal;                       
    }

    private static StatModifier GetRandomAffix()
    {
        StatType[] possibleStats = { StatType.MaxHealth, StatType.PhysicalAttack, StatType.ActionSpeed, StatType.Strength };
        StatType rolledStat = possibleStats[Random.Range(0, possibleStats.Length)];
        float rolledValue = Random.Range(5f, 20f);
        
        return new StatModifier { 
            Stat = rolledStat, 
            Value = rolledValue, 
            Type = ModifierType.Flat 
        };
    }
}