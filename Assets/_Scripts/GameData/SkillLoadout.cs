using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillLoadout", menuName = "ScriptableObjects/GameData/SkillLoadout")]
public class SkillLoadout : ScriptableObject
{
    
    public SkillLoadout playerLoadout; // ลากไฟล์ ScriptableObject ตัวเดียวกันมาใส่ที่ Inspector
    public List<Skill> EquippedSkills = new List<Skill>();

    void Start()
    {
        // ดึงข้อมูลที่ Save ไว้จากหน้าเลือก Skill มาใส่ในตัวละคร
        if (playerLoadout != null)
        {
            EquippedSkills = new List<Skill>(playerLoadout.EquippedSkills);
        }
    }

    public List<Skill> GetSkills() 
    {
        return EquippedSkills;
    }
}