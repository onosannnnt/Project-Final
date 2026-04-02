using UnityEngine;

public class EnemyGenerator : Singleton<EnemyGenerator>
{
    [SerializeField] private Transform worldParent;
    
    [Header("Quest/Phase Setups")]
    public QuestEnemies[] quests;

    private Vector3[] spawnPositions = new Vector3[]
    {
        new Vector3(-1.38f, 0.03f, -0.57f), // ตำแหน่งที่ 1 (ซ้าย)
        new Vector3(-0.93f, 0.03f, -1.52f), // ตำแหน่งที่ 2 (กลาง)
        new Vector3(-0.91f, -0.21f, 0.72f)  // ตำแหน่งที่ 3 (ขวา)
    };

    void Start()
    {
        // แนะนำให้ลบส่วนนี้ออก แล้วให้ TurnManager เป็นคนเรียกแทนเมื่อพร้อม
        // EnemyGenerator.Instance.GenerateInitialEnemy(); 
        Invoke(nameof(GenerateInitialEnemy), 0.1f); 
    }

    public void GenerateInitialEnemy() // เปลี่ยนเป็น public เพื่อให้ระบบอื่นเรียกได้
    {
        if (TurnManager.Instance != null && TurnManager.Instance.currentWave == 1)
        {
            GenerateEnemy();
        }
    }

    public QuestEnemies GetCurrentQuest()
    {
        if (quests == null || quests.Length == 0) return null;
        int questIndex = 0;
        
        if (PlayerCombat.instance != null && PlayerCombat.instance.GetUserData() != null)
        {
            questIndex = PlayerCombat.instance.GetUserData().SelectedQuestIndex;
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

        if (TurnManager.Instance != null && PlayerCombat.instance != null && PlayerCombat.instance.GetUserData() != null)
        {
            questIndex = PlayerCombat.instance.GetUserData().SelectedQuestIndex;
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

        Quaternion spawnRotation = Quaternion.Euler(0, -50, 0);

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