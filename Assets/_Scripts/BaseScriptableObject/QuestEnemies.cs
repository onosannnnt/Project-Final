using UnityEngine;

[System.Serializable]
public class EnemySpawnConfig
{
    [Tooltip("The enemy prefab to spawn")]
    public GameObject enemyPrefab;

    [Tooltip("Target spawn position (0 = Left, 1 = Middle, 2 = Right)")]
    [Range(0, 2)]
    public int positionIndex;
}

[System.Serializable]
public class WaveConfig
{
    [Tooltip("Enemies to spawn in this wave")]
    public EnemySpawnConfig[] enemies;
}

[CreateAssetMenu(fileName = "NewQuestEnemies", menuName = "Game Data/Quest Enemies")]
public class QuestEnemies : ScriptableObject
{
    public bool isTutorial;
    
    [Tooltip("Waves configuration for this quest. Index 0 = Wave 1, Index 1 = Wave 2, etc.")]
    public WaveConfig[] waves;
}