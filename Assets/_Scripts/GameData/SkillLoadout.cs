using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillLoadout", menuName = "ScriptableObjects/GameData/SkillLoadout")]
public class SkillLoadout : ScriptableObject
{
    public List<Skill> EquippedSkills = new();
}