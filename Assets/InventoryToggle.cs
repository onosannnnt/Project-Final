using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject inventoryPanel; // ลาก InventoryPanel จาก Hierarchy มาใส่ช่องนี้

    private bool isOpen = false;

    void Start()
    {
        // เริ่มเกมมาให้ปิด Inventory ไว้ก่อน
        inventoryPanel.SetActive(false);
    }

    void Update()
    {
        // ตรวจสอบว่ากดปุ่ม E หรือไม่
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isOpen = !isOpen; // สลับค่า true/false
        inventoryPanel.SetActive(isOpen);

        if (isOpen)
        {
            // ถ้าเปิด: หยุดเวลาเกม (แบบ HSR) และแสดงเมาส์
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            // ถ้าปิด: ให้เวลาเดินต่อ และซ่อนเมาส์ (ถ้าเป็นเกม 3D)
            Time.timeScale = 1f;
            // Cursor.visible = false;
            // Cursor.lockState = CursorLockMode.Locked;
        }
    }
}