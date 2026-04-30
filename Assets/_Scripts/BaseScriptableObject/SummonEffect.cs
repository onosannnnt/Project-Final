using UnityEngine;

[CreateAssetMenu(fileName = "SummonEffect", menuName = "ScriptableObjects/SkillEffect/SummonEffect")]
public class SummonEffect : SkillEffect
{
    [Tooltip("List of enemies to summon and their spawn positions")]
    public EnemySpawnConfig[] enemiesToSummon;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (enemiesToSummon == null || enemiesToSummon.Length == 0)
        {
            Debug.LogWarning("SummonEffect has no enemies assigned to summon!");
            return false;
        }

        if (TurnManager.Instance != null && EnemyGenerator.Instance != null)
        {
            // Determine the common parent
            Transform worldParent = EnemyGenerator.Instance.transform.Find("WorldParent");
            if (worldParent == null) 
            {
                worldParent = caster.transform.parent; // Fallback
            }

            foreach (var spawnConfig in enemiesToSummon)
            {
                if (spawnConfig.enemyPrefab == null) continue;

                // Spawn the enemy at the predefined quest position index
                Vector3 spawnPos = EnemyGenerator.Instance.GetSpawnPosition(spawnConfig.positionIndex);
                
                // Set rotation specifically for enemies
                Quaternion spawnRotation = Quaternion.Euler(0, 0, 0);
                
                GameObject newEnemyObj = Instantiate(spawnConfig.enemyPrefab, spawnPos, spawnRotation);

                if (worldParent != null)
                {
                    newEnemyObj.transform.SetParent(worldParent, true);
                }

                EnemyCombat enemyCombat = newEnemyObj.GetComponent<EnemyCombat>();
                if (enemyCombat != null)
                {
                    int newId = Object.FindObjectsByType<EnemyCombat>(FindObjectsSortMode.None).Length + 1;
                    enemyCombat.SetEntityID(newId);
                    
                    Debug.Log($"{caster.gameObject.name} summoned {newEnemyObj.gameObject.name} at position {spawnConfig.positionIndex}!");
                }
            }
        }

        return true;
    }
}
