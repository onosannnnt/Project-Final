using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSkillSet", menuName = "Skills/Skill Set")]
public class SkillSet : ScriptableObject
{
    public string setName;
    [Header("Max 5 Skills")]
    public List<Skill> selectedSkills = new List<Skill>();

    [Header("Set Visuals")]
    public Sprite setHoverBG;      // BG ตอน Hover เมื่อเป็นเซต
    public Sprite setSelectedBG;   // BG เมื่อเลือกเซตนี้แล้ว
    public Sprite setCheckmark;    // ติ๊กถูกแบบพิเศษสำหรับเซต

    // ฟังก์ชันช่วยตรวจสอบว่าเซตนี้เต็มหรือยัง
    public bool IsFull() => selectedSkills.Count >= 5;

    // ฟังก์ชันสำหรับล้างข้อมูลทั้งหมดในเซต
    public void ClearSet()
    {
        selectedSkills.Clear();
    }
}