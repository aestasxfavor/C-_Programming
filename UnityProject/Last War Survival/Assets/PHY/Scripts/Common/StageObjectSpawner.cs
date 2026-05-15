using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageObjectSpawner : MonoBehaviour
{
    [Header("Enemy Pool")]
    [SerializeField] private EnemyPool enemyPool;

    [Header("Obstacle Pool")]
    [SerializeField] private ObstaclePool obstaclePool;

    [Header("BulletBox Pool")]
    [SerializeField] private BulletBoxPool bulletBoxPool;

    [Header("Gate Pool")]
    [SerializeField] private GatePool gatePool;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2.2f;
    [SerializeField] private int minEnemyCount = 2;
    [SerializeField] private int maxEnemyCount = 3;

    [Range(0f, 1f)]
    [SerializeField] private float obstacleSpawnChance = 0.15f;

    [Header("Intro Spawn Settings")]
    [SerializeField] private bool useIntroSequence = true;
    [SerializeField] private float firstGateDelay = 3f;
    [SerializeField] private float bulletBoxDelayAfterGate = 2f;
    [SerializeField] private float normalWaveDelayAfterBulletBox = 1f;
    [SerializeField] private float introBulletBoxZOffset = -8f;

    [Header("Gate Spawn Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float gateSpawnChance = 0.2f;
    [SerializeField] private int minWavesBetweenGates = 3;

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
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        if (useIntroSequence)
        {
            yield return new WaitForSeconds(firstGateDelay);

            SpawnIntroGate();

            yield return new WaitForSeconds(bulletBoxDelayAfterGate);

            SpawnIntroBulletBox();

            yield return new WaitForSeconds(normalWaveDelayAfterBulletBox);
        }

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
        if (gatePool == null)
        {
            return false;
        }

        if (wavesSinceLastGate < minWavesBetweenGates)
        {
            return false;
        }

        return Random.value <= gateSpawnChance;
    }

    private void SpawnIntroGate()
    {
        int laneIndex = GetCenterLaneIndex();

        SpawnGate(laneIndex);

        wavesSinceLastGate = 0;

        Debug.Log("Intro Gate Spawned");
    }

    private void SpawnIntroBulletBox()
    {
        int laneIndex = GetCenterLaneIndex();

        if (laneIndex < 0 || laneIndex >= spawnPoints.Length)
        {
            Debug.LogError("BulletBox를 생성할 Lane Index가 잘못되었습니다.");
            return;
        }

        if (bulletBoxPool == null)
        {
            Debug.LogError("BulletBoxPool이 StageObjectSpawner에 연결되지 않았습니다.");
            return;
        }

        Transform spawnPoint = spawnPoints[laneIndex];

        Vector3 spawnPosition = spawnPoint.position;
        spawnPosition.z += introBulletBoxZOffset;

        bulletBoxPool.GetBulletBox(spawnPosition, spawnPoint.rotation);

        Debug.Log($"Intro BulletBox Spawned / Position: {spawnPosition}");
    }

    private void SpawnGateWave()
    {
        int laneIndex = Random.Range(0, spawnPoints.Length);

        SpawnGate(laneIndex);

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

        if (Random.value <= obstacleSpawnChance)
        {
            int obstacleLane = GetUnusedRandomLane(usedLanes);

            if (obstacleLane != -1)
            {
                SpawnObstacle(obstacleLane);
            }
        }
    }

    private void SpawnGate(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= spawnPoints.Length)
        {
            return;
        }

        if (gatePool == null)
        {
            Debug.LogError("GatePool이 StageObjectSpawner에 연결되지 않았습니다.");
            return;
        }

        Transform spawnPoint = spawnPoints[laneIndex];

        gatePool.GetGate(spawnPoint.position, spawnPoint.rotation);
    }

    private void SpawnEnemy(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= spawnPoints.Length)
        {
            return;
        }

        if (enemyPool == null)
        {
            Debug.LogError("EnemyPool이 StageObjectSpawner에 연결되지 않았습니다.");
            return;
        }

        Transform spawnPoint = spawnPoints[laneIndex];

        enemyPool.GetEnemy(spawnPoint.position, spawnPoint.rotation);
    }

    private void SpawnObstacle(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= spawnPoints.Length)
        {
            return;
        }

        if (obstaclePool == null)
        {
            Debug.LogError("ObstaclePool이 StageObjectSpawner에 연결되지 않았습니다.");
            return;
        }

        Transform spawnPoint = spawnPoints[laneIndex];

        obstaclePool.GetObstacle(spawnPoint.position, spawnPoint.rotation);
    }

    private void SpawnBulletBox(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= spawnPoints.Length)
        {
            return;
        }

        if (bulletBoxPool == null)
        {
            Debug.LogError("BulletBoxPool이 StageObjectSpawner에 연결되지 않았습니다.");
            return;
        }

        Transform spawnPoint = spawnPoints[laneIndex];

        bulletBoxPool.GetBulletBox(spawnPoint.position, spawnPoint.rotation);
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

    private int GetCenterLaneIndex()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return -1;
        }

        return spawnPoints.Length / 2;
    }
}