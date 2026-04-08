using UnityEngine;

/// <summary>
/// Base class for all Boss enemies. It enforces that any boss
/// MUST implement its own customized skill selection logic.
/// </summary>
public abstract class BossCombat : EnemyCombat
{
    public abstract Skill ChooseNextSkill();
}
