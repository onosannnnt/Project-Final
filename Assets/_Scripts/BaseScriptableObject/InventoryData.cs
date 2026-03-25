using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInventoryData", menuName = "Inventory/Inventory Data")]
public class InventoryData : ScriptableObject
{
    [Header("Items Stored")]
    public List<EquipmentInstance> items = new List<EquipmentInstance>();
}