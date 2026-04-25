using UnityEngine;
using UnityEngine.UI;
using TMPro; // ถ้าใช้ TextMeshPro ให้เปิดบรรทัดนี้

public class SkillTabManager : MonoBehaviour
{
    [Header("Pages")]
    public GameObject mySkillPage;

    [Header("Buttons Text")]
    public TMP_Text mySkillBtnText; // เปลี่ยนเป็น Text เฉยๆ ถ้าไม่ใช้ TMP

    [Header("Colors")]
    public Color activeColor = new Color(0.5f, 0f, 0.5f); // สีม่วง
    public Color inactiveColor = Color.white;

    void Start()
    {
        // เริ่มต้นให้เปิดหน้า MySkill เป็นค่า Redirect
        ShowMySkill();
    }

    public void ShowMySkill()
    {
        // เปิด-ปิด หน้า
        mySkillPage.SetActive(true);

        // เปลี่ยนสี Text
        mySkillBtnText.color = activeColor;
    }
}
