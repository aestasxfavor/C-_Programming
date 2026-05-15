using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageObjectSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private GameObject obstaclePrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 1.2f;
    [SerializeField] private int minEnemyCount = 1;
    [SerializeField] private int maxEnemyCount = 3;
    [SerializeField] private int bossSpawnEveryWave = 6;

    [Range(0f, 1f)]
    [SerializeField] private float obstacleSpawnChance = 0.25f;

    private int waveCount;
    private Coroutine spawnRoutine;

    private void OnEnable()
    {
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnWave();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnWave()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("Spawn Points가 비어 있습니다.");
            return;
        }

        waveCount++;

        List<int> usedLanes = new List<int>();

        if (bossPrefab != null && bossSpawnEveryWave > 0 && waveCount % bossSpawnEveryWave == 0)
        {
            int bossLane = Random.Range(0, spawnPoints.Length);
            SpawnAt(bossPrefab, bossLane);
            usedLanes.Add(bossLane);
            return;
        }

        int enemyCount = Random.Range(minEnemyCount, maxEnemyCount + 1);
        enemyCount = Mathf.Clamp(enemyCount, 1, spawnPoints.Length);

        List<int> lanes = GetRandomLaneIndices(enemyCount);

        for (int i = 0; i < lanes.Count; i++)
        {
            GameObject enemyPrefab = GetRandomEnemyPrefab();

            if (enemyPrefab == null)
            {
                continue;
            }

            SpawnAt(enemyPrefab, lanes[i]);
            usedLanes.Add(lanes[i]);
        }

        if (obstaclePrefab != null && Random.value <= obstacleSpawnChance)
        {
            int obstacleLane = GetUnusedRandomLane(usedLanes);

            if (obstacleLane != -1)
            {
                SpawnAt(obstaclePrefab, obstacleLane);
            }
        }
    }

    private GameObject GetRandomEnemyPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("Enemy Prefabs가 비어 있습니다.");
            return null;
        }

        int index = Random.Range(0, enemyPrefabs.Length);
        return enemyPrefabs[index];
    }

    private List<int> GetRandomLaneIndices(int count)
    {
        List<int> lanes = new List<int>();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            lanes.Add(i);
        }

        for (int i = 0; i < lanes.Count; i++)
        {
            int randomIndex = Random.Range(i, lanes.Count);

            int temp = lanes[i];
            lanes[i] = lanes[randomIndex];
            lanes[randomIndex] = temp;
        }

        if (count < lanes.Count)
        {
            lanes.RemoveRange(count, lanes.Count - count);
        }

        return lanes;
    }

    private int GetUnusedRandomLane(List<int> usedLanes)
    {
        List<int> availableLanes = new List<int>();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!usedLanes.Contains(i))
            {
                availableLanes.Add(i);
            }
        }

        if (availableLanes.Count == 0)
        {
            return -1;
        }

        int index = Random.Range(0, availableLanes.Count);
        return availableLanes[index];
    }

    private void SpawnAt(GameObject prefab, int laneIndex)
    {
        if (prefab == null)
        {
            return;
        }

        if (laneIndex < 0 || laneIndex >= spawnPoints.Length)
        {
            return;
        }

        Instantiate(prefab, spawnPoints[laneIndex].position, spawnPoints[laneIndex].rotation);
    }
}