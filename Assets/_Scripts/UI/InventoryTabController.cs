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
        public KeyCode hotkey;
    }

    public List<Tab> allTabs;
    public GameObject inventoryUI;
    public GameObject listBackground;

    void Start()
    {
        if (inventoryUI != null) inventoryUI.SetActive(false);
    }

    void Update()
    {
        // 1. จัดการปุ่ม Esc (ปิดอย่างเดียวตามโค้ดล่าสุดของคุณ)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inventoryUI != null) inventoryUI.SetActive(false);
            return;
        }

        // 2. เช็คปุ่มลัดอื่นๆ
        for (int i = 0; i < allTabs.Count; i++)
        {
            if (allTabs[i].hotkey == KeyCode.None || allTabs[i].hotkey == KeyCode.Escape) continue;

            if (Input.GetKeyDown(allTabs[i].hotkey))
            {
                OpenInventory(i);
            }
        }
    }

    public void OpenInventory(int index)
    {
        // ป้องกัน Error: เช็คว่า Index อยู่ในขอบเขตและตัวแปรไม่เป็น Null
        if (index < 0 || index >= allTabs.Count || inventoryUI == null || allTabs[index].panel == null) 
        {
            Debug.LogWarning("ตรวจเช็ค Inspector: อาจจะลืมลาก Panel หรือตั้งค่า Index ผิด");
            return;
        }
            inventoryUI.SetActive(true);
            ShowTab(index);
    }

    public void ShowTab(int index)
    {
        for (int i = 0; i < allTabs.Count; i++)
        {
            if (allTabs[i].panel == null || allTabs[i].tabButtonImage == null) continue;

            bool isActive = (i == index);
            allTabs[i].panel.SetActive(isActive);
            allTabs[i].tabButtonImage.sprite = isActive ? allTabs[i].activeSprite : allTabs[i].inactiveSprite;
        }

        if (listBackground != null)
        {
            // เช็คตามชื่อ Tab แทนการใช้ตัวเลข Index ตายตัวจะปลอดภัยกว่าครับ
            listBackground.SetActive(allTabs[index].name != "World Map"); 
        }
    }
}