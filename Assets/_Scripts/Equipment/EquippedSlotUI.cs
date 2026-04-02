using UnityEngine;
using UnityEngine.UI;

public class EquippedSlotUI : MonoBehaviour
{
    [Header("ตั้งค่าช่องนี้")]
    public CharacterSlot mySlotType; // ระบุไปเลยว่ากล่องนี้คือ Helmet, Armor, ฯลฯ

    [Header("UI References")]
    public Image frameImage; 
    public Image itemIcon;   
    public RarityVisualSettings visualSettings;

    private EquipmentManager eqManager;

    public void Initialize(EquipmentManager manager)
    {
        eqManager = manager;
        // บอกว่าถ้ามีของเปลี่ยน ให้รันฟังก์ชัน RefreshSlot ทันที
        eqManager.OnEquipmentChanged += RefreshSlot; 
        RefreshSlot();
    }

    private void OnDestroy()
    {
        if (eqManager != null) eqManager.OnEquipmentChanged -= RefreshSlot;
    }

    public void RefreshSlot()
    {
        // ไปดึงของที่ใส่อยู่ในช่องนี้มาดู
        EquipmentInstance equippedItem = eqManager.GetEquippedItem(mySlotType);

        if (equippedItem == null)
        {
            // ถ้าไม่ได้ใส่ของ ปิดรูปโชว์กรอบเปล่า
            itemIcon.gameObject.SetActive(false);
            frameImage.sprite = visualSettings.NormalFrame;
        }
        else
        {
            // ถ้ามีของใส่ ก็โชว์รูปและกรอบ
            itemIcon.gameObject.SetActive(true);
            itemIcon.sprite = equippedItem.BaseItem.Icon;
            frameImage.sprite = visualSettings.GetFrameByRarity(equippedItem.Rarity);
        }
    }

    // เอาฟังก์ชันนี้ไปผูกกับปุ่ม (On Click) ของช่องด้านบน
    public void OnClickUnequip()
    {
        if (eqManager != null && eqManager.GetEquippedItem(mySlotType) != null)
        {
            eqManager.Unequip(mySlotType);
        }
    }
}