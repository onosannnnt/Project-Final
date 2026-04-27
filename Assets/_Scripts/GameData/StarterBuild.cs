using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewStarterBuild", menuName = "ScriptableObjects/GameData/StarterBuild")]
public class StarterBuild : ScriptableObject
{
    public string buildName;
    [TextArea(3, 10)]
    public string buildDescription;
    public List<Skill> starterSkills = new List<Skill>(); // Should be 8 skills
}
