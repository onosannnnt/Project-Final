using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField] private GameObject Enemy;
    [SerializeField] private Transform worldParent;
    void Start()
    {
        GenerateEnemy();
    }

    private void GenerateEnemy()
    {
        int enemyCount = Random.Range(1, 4); // Generate between 1 to 3 enemies
        if (enemyCount <= 0) return;
        switch (enemyCount)
        {
            case 1:
                Instantiate(Enemy, new Vector3(-1.8f, 0.6f, 0), Quaternion.Euler(0, -50, 0), worldParent);
                break;
            case 2:
                Instantiate(Enemy, new Vector3(-2, 0.6f, 0.8f), Quaternion.Euler(0, -50, 0), worldParent);
                Instantiate(Enemy, new Vector3(-1.8f, 0.6f, -1), Quaternion.Euler(0, -50, 0), worldParent);
                break;
            case 3:
                Instantiate(Enemy, new Vector3(-2, 0.6f, 1.5f), Quaternion.Euler(0, -50, 0), worldParent);
                Instantiate(Enemy, new Vector3(-1.8f, 0.6f, 0), Quaternion.Euler(0, -50, 0), worldParent);
                Instantiate(Enemy, new Vector3(-1.6f, 0.6f, -1.2f), Quaternion.Euler(0, -50, 0), worldParent);
                break;
        }
    }
}
