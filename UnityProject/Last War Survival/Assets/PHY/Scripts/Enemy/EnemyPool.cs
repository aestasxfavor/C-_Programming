using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int poolSize = 20;

    private readonly List<GameObject> pooledEnemies = new List<GameObject>();

    private void Awake()
    {
        CreatePool();
    }

    private void CreatePool()
    {
        if (enemyPrefab == null)
        {
            return;
        }

        // 게임 중 반복 생성되는 적을 미리 만들어 재사용
        for (int i = 0; i < poolSize; i++)
        {
            CreateEnemy();
        }
    }

    private GameObject CreateEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab, transform);

        enemy.SetActive(false);
        pooledEnemies.Add(enemy);

        return enemy;
    }

    public GameObject GetEnemy(Vector3 position, Quaternion rotation)
    {
        GameObject enemy = GetInactiveEnemy();

        if (enemy == null)
        {
            enemy = CreateEnemy();
        }

        if (enemy == null)
        {
            return null;
        }

        enemy.transform.SetPositionAndRotation(position, rotation);
        enemy.SetActive(true);

        return enemy;
    }

    private GameObject GetInactiveEnemy()
    {
        for (int i = 0; i < pooledEnemies.Count; i++)
        {
            if (pooledEnemies[i] == null)
            {
                continue;
            }

            if (!pooledEnemies[i].activeSelf)
            {
                return pooledEnemies[i];
            }
        }

        return null;
    }
}