using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageObjectSpawner : MonoBehaviour
{
    [Header("Enemy Pool")]
    [SerializeField] private EnemyPool enemyPool;

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Obstacle Prefab")]
    [SerializeField] private GameObject obstaclePrefab;

    [Header("Gate Prefabs")]
    [SerializeField] private GameObject[] gatePrefabs;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2.2f;
    [SerializeField] private int minEnemyCount = 2;
    [SerializeField] private int maxEnemyCount = 3;

    [Range(0f, 1f)]
    [SerializeField] private float obstacleSpawnChance = 0.15f;

    [Header("Gate Spawn Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float gateSpawnChance = 0.2f;
    [SerializeField] private int minWavesBetweenGates = 3;

    private int waveCount;
    private int wavesSinceLastGate;
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
        wavesSinceLastGate++;

        if (CanSpawnGate())
        {
            SpawnGateWave();
            return;
        }

        SpawnEnemyObstacleWave();
    }

    private bool CanSpawnGate()
    {
        if (gatePrefabs == null || gatePrefabs.Length == 0)
        {
            return false;
        }

        if (wavesSinceLastGate < minWavesBetweenGates)
        {
            return false;
        }

        return Random.value <= gateSpawnChance;
    }

    private void SpawnGateWave()
    {
        GameObject gatePrefab = GetRandomGatePrefab();

        if (gatePrefab == null)
        {
            return;
        }

        int laneIndex = Random.Range(0, spawnPoints.Length);

        SpawnAt(gatePrefab, laneIndex);

        wavesSinceLastGate = 0;

        Debug.Log("Gate Spawned");
    }

    private void SpawnEnemyObstacleWave()
    {
        List<int> usedLanes = new List<int>();

        int enemyCount = Random.Range(minEnemyCount, maxEnemyCount + 1);
        enemyCount = Mathf.Clamp(enemyCount, 1, spawnPoints.Length);

        List<int> enemyLanes = GetRandomLaneIndices(enemyCount);

        for (int i = 0; i < enemyLanes.Count; i++)
        {
            SpawnEnemy(enemyLanes[i]);
            usedLanes.Add(enemyLanes[i]);
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

    private void SpawnEnemy(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= spawnPoints.Length)
        {
            return;
        }

        Transform spawnPoint = spawnPoints[laneIndex];

        if (enemyPool != null)
        {
            enemyPool.GetEnemy(spawnPoint.position, spawnPoint.rotation);
            return;
        }

        GameObject enemyPrefab = GetRandomEnemyPrefab();

        if (enemyPrefab == null)
        {
            return;
        }

        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
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

    private GameObject GetRandomGatePrefab()
    {
        if (gatePrefabs == null || gatePrefabs.Length == 0)
        {
            return null;
        }

        int index = Random.Range(0, gatePrefabs.Length);
        return gatePrefabs[index];
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