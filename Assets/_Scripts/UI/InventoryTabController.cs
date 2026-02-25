using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryTabController : MonoBehaviour
{
    [System.Serializable]
    public struct Tab
    {
        public string name;
        public GameObject panel;
        public Image tabButtonImage;
        public Sprite activeSprite;
        public Sprite inactiveSprite;
        public KeyCode hotkey; // เพิ่มช่องสำหรับใส่ปุ่มบนคีย์บอร์ด
    }

    public List<Tab> allTabs;
    public GameObject inventoryUI; // ตัว Parent ใหญ่ที่เก็บทุกอย่าง (เพื่อสั่ง เปิด/ปิด เมนู)
    public GameObject listBackground;

    void Start()
    {
        // เริ่มเกมมาให้ปิด Inventory ไว้ก่อน
        inventoryUI.SetActive(false);
    }

    void Update()
{
    // 1. เช็คปุ่มลัดอื่นๆ (E, F, M...)
    for (int i = 0; i < allTabs.Count; i++)
    {
        if (Input.GetKeyDown(allTabs[i].hotkey))
        {
            OpenInventory(i);
        }
    }

    // 2. กรณีพิเศษ: ปุ่ม Esc
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        // ถ้าเมนูปิดอยู่ -> ให้เปิดหน้า Setting (สมมติ Setting คือ Index 4)
        if (!inventoryUI.activeSelf) 
        {
            OpenInventory(4); // เปลี่ยนเลข 4 เป็น Index ของหน้า Setting ใน List ของคุณ
        }
        else 
        {
            // ถ้าเมนูเปิดอยู่แล้ว -> ให้ปิดเมนู
            inventoryUI.SetActive(false);
        }
    }
}

    public void OpenInventory(int index)
{
    // ถ้าหน้าจอนั้นเปิดอยู่แล้ว แล้วกดปุ่มเดิมซ้ำ ให้ปิดเมนู (Toggle)
    if (inventoryUI.activeSelf && allTabs[index].panel.activeSelf)
    {
        inventoryUI.SetActive(false);
        return;
    }

    inventoryUI.SetActive(true);
    ShowTab(index);
}

    public void ShowTab(int index)
    {
        for (int i = 0; i < allTabs.Count; i++)
        {
            bool isActive = (i == index);
            allTabs[i].panel.SetActive(isActive);
            allTabs[i].tabButtonImage.sprite = isActive ? allTabs[i].activeSprite : allTabs[i].inactiveSprite;
        }

        // จัดการพื้นหลัง (ในรูปเห็นเป็นกรอบกระดาษ ถ้าเป็น World Map อาจจะอยากให้จางลงหรือหายไป)
        if (listBackground != null)
        {
            // สมมติ World Map คือ Index 4
            listBackground.SetActive(index != 4); 
        }
    }
}