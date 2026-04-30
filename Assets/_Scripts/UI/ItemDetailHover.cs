using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // จำเป็นต้องใช้เพื่อดักจับการ Hover

public class ItemDetailHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Reference")]
    public CanvasGroup detailCanvasGroup; // ลาก Right_Details มาใส่ที่นี่

    [Header("Item Info")]
    public string itemName;
    public string amount;
    public string description;
    public Sprite icon;

    [Header("Display Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI descriptionText;
    public Image iconImage;

    void Start()
    {
        detailCanvasGroup.alpha = 0;
    }

    // เมื่อเมาส์ "เริ่ม" วางทับปุ่ม (Hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 1. อัปเดตข้อมูลให้ตรงกับไอเทมชิ้นนี้
        nameText.text = itemName;
        amountText.text = amount;
        descriptionText.text = description;
        iconImage.sprite = icon;

        // 2. เปิดหน้าจอ Right_Details
        // ทำให้หน้าจอโชว์ (Alpha = 1) และ "ห้าม" รับเมาส์ (Blocks Raycasts = false)
        detailCanvasGroup.alpha = 1;
        detailCanvasGroup.blocksRaycasts = false;
    }

    // เมื่อเมาส์ "เลื่อนออกจาก" ปุ่ม
    public void OnPointerExit(PointerEventData eventData)
    {
        // ปิดหน้าจอ Right_Details
        // ทำให้หน้าจอหายไป
        detailCanvasGroup.alpha = 0;
    }
}
