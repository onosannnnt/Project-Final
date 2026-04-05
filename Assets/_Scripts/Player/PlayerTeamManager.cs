using UnityEngine;
using System.Collections.Generic;

public class PlayerTeamManager : Singleton<PlayerTeamManager>
{
    [SerializeField] private Transform worldParent; // Parent location for characters
    [Header("Player Settings")]
    public GameObject mainPlayerPrefab; // Assuming you have a prefab for the player
    public GameObject allyPrefab;       // Assuming you have a prefab for the ally
    
    [Header("Team Speed Distribution")]
    [Tooltip("How much of the total ActionSpeed goes to the Main Player vs the Ally. 1 = 100% Player, 0 = 100% Ally.")]
    [Range(0f, 1f)] 
    public float mainPlayerSpeedRatio = 0.7f; // Default 70% to Player, 30% to Ally
    private float originalTotalSpeed = -1f;
    
    public float GetOriginalTotalSpeed()
    {
        if (PlayerCombat.instance == null) return 100f; // Safebase

        // Lock in the total speed the first time we check it so we don't recalculate off a scaled version
        if (originalTotalSpeed < 0)
        {
            originalTotalSpeed = PlayerCombat.instance.GetStat(StatType.ActionSpeed);
        }

        return originalTotalSpeed;
    }

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
        if (PlayerCombat.instance != null)
        {
            ActiveTeamMembers.Add(PlayerCombat.instance);
            // Assign explicitly to the first spawn position and rotation
            PlayerCombat.instance.transform.position = spawnPositions[0];
            PlayerCombat.instance.transform.rotation = Quaternion.Euler(spawnRotations[0]);

            SpawnAlly();
        }
    }

    public void SpawnAlly()
    {
        if (allyPrefab == null) return;

        // Spawn second character at Position 2 and Rotation 2
        GameObject allyObj = Instantiate(allyPrefab, spawnPositions[1], Quaternion.Euler(spawnRotations[1]), worldParent);
        
        PlayerAlly allyCombat = allyObj.GetComponent<PlayerAlly>();
        if (allyCombat != null)
        {
            // Sync with PlayerCombat before adding
            allyCombat.SyncStatsWithMainPlayer();
            ActiveTeamMembers.Add(allyCombat);
            Debug.Log($"[PlayerTeamManager] Spawned {allyCombat.Stats.EntityName}");
        }
    }
}
