using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField] private GameObject Enemy;
    [SerializeField] private Transform worldParent;
    [SerializeField] private int enemy = 1;
    void Start()
    {
        GenerateEnemy();
    }

    // private void GenerateEnemy()
    // {
    //     int enemyCount = Random.Range(3, enemy + 1); // Generate between 1 to enemy
    //     if (enemyCount <= 0) return;
    //     switch (enemyCount)
    //     {
    //         case 1:
    //             Instantiate(Enemy, new Vector3(-1f, 0.6f, 0), Quaternion.Euler(0, -50, 0), worldParent);
    //             break;
    //         case 2:
    //             Instantiate(Enemy, new Vector3(-1, 0.6f, -1), Quaternion.Euler(0, -50, 0), worldParent);
    //             Instantiate(Enemy, new Vector3(-0.6f, 0.6f, 0.8f), Quaternion.Euler(0, -50, 0), worldParent);
    //             break;
    //         case 3:
    //             Instantiate(Enemy, new Vector3(-2, 0.6f, 1.5f), Quaternion.Euler(0, -50, 0), worldParent);
    //             Instantiate(Enemy, new Vector3(-1.8f, 0.6f, 0), Quaternion.Euler(0, -50, 0), worldParent);
    //             Instantiate(Enemy, new Vector3(-1.6f, 0.6f, -1.2f), Quaternion.Euler(0, -50, 0), worldParent);
    //             break;
    //     }
    // }
    // แนะนำให้สร้างตัวแปรเก็บตำแหน่งแยกไว้ด้านนอก เพื่อให้แก้ค่าง่ายๆ ครับ
    private Vector3[] spawnPositions = new Vector3[]
    {
    new Vector3(-1.38f, 0.03f, -0.57f), // ตำแหน่งที่ 1
    new Vector3(-0.93f, 0.03f, -1.52f), // ตำแหน่งที่ 2
    new Vector3(-0.91f, -0.21f, 0.72f)  // ตำแหน่งที่ 3
    };

    private void GenerateEnemy()
    {
        // แก้ให้สุ่มตั้งแต่ 1 ถึงค่า enemy (ตามคอมเมนต์ของคุณ)
        int enemyCount = Random.Range(3, enemy + 1);

        // ป้องกันไม่ให้สร้างศัตรูเกินจำนวนตำแหน่งที่เรามี (เดี๋ยว Error Out of Bounds)
        if (enemyCount > spawnPositions.Length)
        {
            enemyCount = spawnPositions.Length;
        }

        if (enemyCount <= 0) return;

        // เก็บค่ามุมหมุนไว้ตัวเดียวเลย เพราะใช้ค่าเดียวกันหมด
        Quaternion spawnRotation = Quaternion.Euler(0, -50, 0);

        // วนลูปสร้างศัตรูตามจำนวนที่สุ่มได้
        for (int i = 0; i < enemyCount; i++)
        {
            // ใช้ i เป็นตัวบอก index ของตำแหน่งใน Array
            // 1. เก็บตัวแปร Object ที่ถูกสร้างขึ้นมา
            GameObject newEnemy = Instantiate(Enemy, spawnPositions[i], spawnRotation, worldParent);

            // 2. ดึงสคริปต์ที่ชื่อ EnemyScript ออกมาจาตัวที่สร้าง
            EnemyCombat enemy = newEnemy.GetComponent<EnemyCombat>();

            // 3. ถ้าเจอสคริปต์ ให้ส่งค่า ID เข้าไป (i + 1 เพื่อให้เริ่มที่ 1, 2, 3)
            if (enemy != null)
            {
                enemy.SetEntityID(i + 1);
            }

        }
    }
}
