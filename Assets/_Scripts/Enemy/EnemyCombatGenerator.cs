using UnityEngine;

public class EnemyGenerator : Singleton<EnemyGenerator>
{
    [SerializeField] private Transform worldParent;

    [Header("Quest/Phase Setups")]
    public QuestEnemies[] quests;

    private Vector3[] spawnPositions = new Vector3[]
    {
        new Vector3(-2.033f, -0.941f, 0f), // ตำแหน่งที่ 1 (ซ้ายสุด)
        new Vector3(-1.507f, -0.663f, 0f), // ตำแหน่งที่ 2 (ซ้าย)
        new Vector3(-0.516f, 0.039f, 0f),  // ตำแหน่งที่ 3 (ขวาสุด)
        new Vector3(-1.027f, -0.267f, 0f),  // ตำแหน่งที่ 4 (ขวา)
        new Vector3(-1.966f, -0.031f, 0f)  // ตำแหน่งที่ 5 (กลาง)
    };

    [SerializeField] private UserData userData; // Injected globally via Inspector (ScriptableObject)

    public Vector3 GetSpawnPosition(int index)
    {
        if (index >= 0 && index < spawnPositions.Length)
        {
            return spawnPositions[index];
        }
        return Vector3.zero; // default
    }

    void Start()
    {
        // แนะนำให้ลบส่วนนี้ออก แล้วให้ TurnManager เป็นคนเรียกแทนเมื่อพร้อม
        // EnemyGenerator.Instance.GenerateInitialEnemy(); 
        Invoke(nameof(GenerateInitialEnemy), 0.1f);
    }

    public void GenerateInitialEnemy() // เปลี่ยนเป็น public เพื่อให้ระบบอื่นเรียกได้
    {
        if (worldParent != null)
        {
            foreach (Transform child in worldParent)
            {
                if (child != null && child.CompareTag("Enemy"))
                {
                    return;
                }
            }
        }

        if (TurnManager.Instance != null && TurnManager.Instance.currentWave == 1)
        {
            GenerateEnemy();
        }
    }

    public QuestEnemies GetCurrentQuest()
    {
        if (quests == null || quests.Length == 0) return null;
        int questIndex = 0;

        if (userData != null)
        {
            questIndex = userData.SelectedQuestIndex;
        }

        questIndex = Mathf.Clamp(questIndex, 0, quests.Length - 1);
        return quests[questIndex];
    }

    public void GenerateEnemy()
    {
        // ป้องกัน Error กรณีไม่ได้ใส่ข้อมูลใน Inspector
        if (quests == null || quests.Length == 0)
        {
            Debug.LogError("[EnemyGenerator] Quests array is empty! Cannot spawn enemies.");
            return;
        }

        int currentWave = 1;
        int maxWave = 3;
        int questIndex = 0;

        if (TurnManager.Instance != null && userData != null)
        {
            questIndex = userData.SelectedQuestIndex;
            currentWave = TurnManager.Instance.currentWave;
            maxWave = (int)TurnManager.Instance.GetMaxWave();
        }

        questIndex = Mathf.Clamp(questIndex, 0, quests.Length - 1);
        QuestEnemies currentQuest = quests[questIndex];

        if (currentQuest.waves == null || currentQuest.waves.Length == 0)
        {
            Debug.LogError("[EnemyGenerator] Quest " + currentQuest.name + " has no waves defined. Cannot spawn enemies.");
            return;
        }

        // เช็ค index wave ให้อยู่ในขอบเขต (ลดลง 1 เพราะ wave เริ่มจาก 1 แต่ index เริ่มจาก 0)
        int waveIndex = Mathf.Clamp(currentWave - 1, 0, currentQuest.waves.Length - 1);
        WaveConfig currentWaveConfig = currentQuest.waves[waveIndex];

        Quaternion spawnRotation = Quaternion.Euler(0, 0, 0);

        for (int i = 0; i < currentWaveConfig.enemies.Length; i++)
        {
            EnemySpawnConfig spawnConfig = currentWaveConfig.enemies[i];

            if (spawnConfig.enemyPrefab == null) continue;

            // บังคับเกิดตรงจุดที่กำหนด
            int spawnIndex = Mathf.Clamp(spawnConfig.positionIndex, 0, spawnPositions.Length - 1);

            GameObject newEnemy = Instantiate(spawnConfig.enemyPrefab, spawnPositions[spawnIndex], spawnRotation, worldParent);

            EnemyCombat enemy = newEnemy.GetComponent<EnemyCombat>();
            if (enemy != null)
            {
                enemy.SetEntityID(i + 1);
            }
        }
    }
}
