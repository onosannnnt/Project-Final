using System;
using System.Collections.Generic;

[Serializable]
public class CombatSkillLoadoutLogs
{
    public int CombatID;
    public List<SkillLogs> SkillLoadut;
}

[Serializable]
public class SkillLogs
{
    public int SkillID;
    public string SkillName;

    public static implicit operator SkillLogs(List<SkillLogs> v)
    {
        throw new NotImplementedException();
    }
}
