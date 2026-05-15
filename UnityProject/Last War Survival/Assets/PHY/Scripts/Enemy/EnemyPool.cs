using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int initialSize = 20;

    private readonly List<GameObject> enemies = new List<GameObject>();

    private void Awake()
    {
        CreateInitialEnemies();
    }

    private void CreateInitialEnemies()
    {
        for (int i = 0; i < initialSize; i++)
        {
            CreateEnemy();
        }
    }

    private GameObject CreateEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab, transform);

        enemy.name = $"PooledEnemy_{enemies.Count + 1}";
        enemy.SetActive(false);

        enemies.Add(enemy);

        return enemy;
    }

    public GameObject GetEnemy(Vector3 position, Quaternion rotation)
    {
        GameObject enemy = GetInactiveEnemy();

        if (enemy == null)
        {
            enemy = CreateEnemy();
        }

        enemy.transform.SetPositionAndRotation(position, rotation);
        enemy.SetActive(true);

        return enemy;
    }

    private GameObject GetInactiveEnemy()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == null)
            {
                continue;
            }

            if (!enemies[i].activeSelf)
            {
                return enemies[i];
            }
        }

        return null;
    }
}