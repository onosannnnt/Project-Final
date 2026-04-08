using UnityEngine;
using System.Collections.Generic;

public class PlayerTeamManager : Singleton<PlayerTeamManager>
{
    [SerializeField] private Transform worldParent; // Parent location for characters
    [Header("Player Settings")]
    public GameObject mainPlayerPrefab; // Assuming you have a prefab for the player
    
    // Position logic similar to enemy
    private Vector3[] spawnPositions = new Vector3[]
    {
        new Vector3(0.911f, -0.66f, 0f), // Position 1 (Main Player)
        new Vector3(1.341f, -0.696f, 0.72f), // Position 2 (Ally 1)
        new Vector3(0.91f, -0.21f, 0.72f), // Position 3 (Ally 2 - future proofing)
        new Vector3(1.5f, 0.03f, 0f),      // Position 4 (Ally 3 - future proofing)
        new Vector3(1f, 0.03f, 0.5f)       // Position 5 (Ally 4 - future proofing)
    };

    // Rotation logic (Euler angles)
    private Vector3[] spawnRotations = new Vector3[]
    {
        new Vector3(0f, 180f,0f), // Rotation 1 (Main Player)
        new Vector3(0f, 180f,0f), // Rotation 2 (Ally 1)
        new Vector3(0f, 180f, 0f), // Rotation 3 (Ally 2)
        new Vector3(0f, 180f, 0f), // Rotation 4 (Ally 3)
        new Vector3(0f, 180f, 0f)  // Rotation 5 (Ally 4)
    };

    public List<Entity> ActiveTeamMembers = new List<Entity>();

    private void Start()
    {
        // For testing/starting, we can spawn them directly, 
        // or TurnManager can call this when loading the battle.
        Invoke(nameof(SpawnTeam), 0.1f);
    }

    public void SpawnTeam()
    {
        // Just an example framework. To fully integrate, we would Instantiate 
        // the player here instead of having them pre-placed in the scene.
        
        // Let's assume Main Player is pre-placed, so we find them
        if (PlayerCombat.instance != null && !ActiveTeamMembers.Contains(PlayerCombat.instance))
        {
            ActiveTeamMembers.Add(PlayerCombat.instance);
            // Assign explicitly to the first spawn position and rotation
            PlayerCombat.instance.transform.position = spawnPositions[0];
            PlayerCombat.instance.transform.rotation = Quaternion.Euler(spawnRotations[0]);
        }
    }
}
