using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour
{
    public Image frameImage; 
    public Image itemIcon;   
    public RarityVisualSettings visualSettings;

    [Header("UI ใหม่")]
    public GameObject checkmarkIcon; // ลากรูปติ๊กถูกมาใส่

    private EquipmentInstance myItem;
    private EquipmentManager eqManager; // ตัวจัดการระบบใส่ของ

    // รับ EquipmentManager เข้ามาด้วย เพื่อให้มันทำงานร่วมกันได้
    public void SetupSlot(EquipmentInstance equipment, EquipmentManager manager)
    {
        myItem = equipment;
        eqManager = manager;

        if (equipment == null)
        {
            itemIcon.gameObject.SetActive(false);
            checkmarkIcon.SetActive(false);
            frameImage.sprite = visualSettings.NormalFrame;
            return;
        }

        itemIcon.gameObject.SetActive(true);
        itemIcon.sprite = equipment.BaseItem.Icon; 
        frameImage.sprite = visualSettings.GetFrameByRarity(equipment.Rarity); 
        
        // เช็คว่าใส่อยู่หรือเปล่า ถ้าใส่ก็เปิดติ๊กถูก
        RefreshCheckmark();
    }

    public void RefreshCheckmark()
    {
        if (myItem == null || eqManager == null) return;
        
        // ถาม EquipmentManager ว่าของชิ้นนี้ใส่อยู่ไหม
        bool isEquipped = eqManager.IsEquipped(myItem);
        checkmarkIcon.SetActive(isEquipped);
    }

    // เอาฟังก์ชันนี้ไปผูกกับปุ่ม (On Click) ของ Prefab ไอเทมนี้
    public void OnSlotClicked()
    {
        if (myItem == null || eqManager == null) return;

        if (eqManager.IsEquipped(myItem))
        {
            // เปลี่ยนเป็นเรียกใช้ตัวนี้แทน
            eqManager.UnequipByItem(myItem);
        }
        else
        {
            eqManager.Equip(myItem);
        }
    }
}