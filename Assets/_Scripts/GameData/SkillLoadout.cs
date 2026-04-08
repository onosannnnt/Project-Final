using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillLoadout", menuName = "ScriptableObjects/GameData/SkillLoadout")]
public class SkillLoadout : ScriptableObject
{

    // public SkillLoadout playerLoadout; // ลากไฟล์ ScriptableObject ตัวเดียวกันมาใส่ที่ Inspector
    public List<Skill> EquippedSkills = new List<Skill>();

    public List<Skill> GetSkills()
    {
        return EquippedSkills;
    }
}
