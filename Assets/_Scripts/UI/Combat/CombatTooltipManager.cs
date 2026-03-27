using UnityEngine;
using TMPro;

public class CombatTooltipManager : MonoBehaviour
{
    public static CombatTooltipManager instance;

    [Header("Tooltip UI")]
    public GameObject infoBox;      // ลากกล่อง Tooltip ใน Scene มาใส่ตรงนี้
    public TextMeshProUGUI infoText; // ลาก Text มาใส่ตรงนี้

    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 50, 0); // ปรับระยะความสูงของกล่องได้ที่นี่

    private void Awake()
    {
        // ตั้งค่า Singleton ให้เรียกใช้จากที่ไหนก็ได้
        if (instance == null) 
        {
            instance = this;
        } 
        else 
        {
            Destroy(gameObject);
        }

        // ซ่อนกล่องไว้ก่อนตอนเริ่มฉาก
        HideTooltip();
    }

    public void ShowTooltip(Skill skill, Vector3 buttonPosition)
    {
        if (skill == null || infoBox == null) return;

        // เปลี่ยนข้อความ
        infoText.text = skill.description;

        // ย้ายกล่องมาที่ตำแหน่งปุ่ม + ระยะ offset
        infoBox.transform.position = buttonPosition + offset;

        // เปิดกล่อง
        infoBox.SetActive(true);
    }

    public void HideTooltip()
    {
        if (infoBox != null)
        {
            infoBox.SetActive(false);
        }
    }
}