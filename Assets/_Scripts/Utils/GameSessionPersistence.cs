using UnityEngine;

/// <summary>
/// This script ensures that critical ScriptableObject assets (UserData, Loadouts) 
/// stay in memory during a build. Without a persistent reference like this, 
/// Unity may purge them during scene transitions, causing progress to reset.
/// </summary>
public class GameSessionPersistence : SingletonPersistent<GameSessionPersistence>
{
    [Header("Data Locks")]
    [Tooltip("Assign your UserData asset here.")]
    public UserData userData;

    [Tooltip("Assign your P1 and P2 SkillLoadout assets here.")]
    public SkillLoadout[] playerLoadouts;

    // This script doesn't need any logic; its mere existence with these 
    // references prevents the assets from being unloaded.
}
