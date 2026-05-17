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

    [Header("Wave Settings")]
    [SerializeField] private float waveInterval = 2.2f;
    [SerializeField] private int minEnemyCount = 2;
    [SerializeField] private int maxEnemyCount = 3;

    [Range(0f, 1f)]
    [SerializeField] private float obstacleChance = 0.15f;

    [Header("Intro Settings")]
    [SerializeField] private bool useIntro = true;
    [SerializeField] private float introGateDelay = 3f;
    [SerializeField] private float introBoxDelay = 2f;
    [SerializeField] private float waveStartDelay = 1f;
    [SerializeField] private float introBoxZOffset = -8f;

    [Header("Gate Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float gateChance = 0.2f;
    [SerializeField] private int gateWaveGap = 3;

    private int waveCountAfterGate;
    private Coroutine spawnCoroutine;

    private void OnEnable()
    {
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        StopSpawnLoop();
    }

    private IEnumerator SpawnLoop()
    {
        // 시작 구간에서 게이트와 BulletBox를 먼저 보여준 뒤 일반 웨이브를 시작함
        if (useIntro)
        {
            yield return new WaitForSeconds(introGateDelay);

            SpawnIntroGate();

            yield return new WaitForSeconds(introBoxDelay);

            SpawnIntroBulletBox();

            yield return new WaitForSeconds(waveStartDelay);
        }

        while (true)
        {
            SpawnWave();

            yield return new WaitForSeconds(waveInterval);
        }
    }

    private void StopSpawnLoop()
    {
        if (spawnCoroutine == null)
        {
            return;
        }

        StopCoroutine(spawnCoroutine);
        spawnCoroutine = null;
    }

    private void SpawnIntroGate()
    {
        int laneIndex = GetMiddleLaneIndex();

        SpawnGate(laneIndex);

        waveCountAfterGate = 0;
    }

    private void SpawnIntroBulletBox()
    {
        int laneIndex = GetMiddleLaneIndex();

        SpawnBulletBox(laneIndex, introBoxZOffset);
    }

    private void SpawnWave()
    {
        if (!HasSpawnPoints())
        {
            return;
        }

        waveCountAfterGate++;

        if (CanSpawnGate())
        {
            SpawnGateWave();
            return;
        }

        SpawnNormalWave();
    }

    private bool CanSpawnGate()
    {
        if (gatePool == null)
        {
            return false;
        }

        // 게이트가 너무 자주 나오지 않도록 일정 웨이브 간격을 둠
        if (waveCountAfterGate < gateWaveGap)
        {
            return false;
        }

        return Random.value <= gateChance;
    }

    private void SpawnGateWave()
    {
        int laneIndex = GetMiddleLaneIndex();

        SpawnGate(laneIndex);

        waveCountAfterGate = 0;
    }

    private void SpawnNormalWave()
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

        if (Random.value > obstacleChance)
        {
            return;
        }

        // 적이 생성된 라인을 피해서 장애물 위치를 고름
        int obstacleLane = GetUnusedLaneIndex(usedLanes);

        if (obstacleLane != -1)
        {
            SpawnObstacle(obstacleLane);
        }
    }

    private void SpawnGate(int laneIndex)
    {
        if (!IsValidLaneIndex(laneIndex))
        {
            return;
        }

        if (gatePool == null)
        {
            return;
        }

        Transform spawnPoint = spawnPoints[laneIndex];

        gatePool.GetGate(spawnPoint.position, spawnPoint.rotation);
    }

    private void SpawnEnemy(int laneIndex)
    {
        if (!IsValidLaneIndex(laneIndex))
        {
            return;
        }

        if (enemyPool == null)
        {
            return;
        }

        Transform spawnPoint = spawnPoints[laneIndex];

        enemyPool.GetEnemy(spawnPoint.position, spawnPoint.rotation);
    }

    private void SpawnObstacle(int laneIndex)
    {
        if (!IsValidLaneIndex(laneIndex))
        {
            return;
        }

        if (obstaclePool == null)
        {
            return;
        }

        Transform spawnPoint = spawnPoints[laneIndex];

        obstaclePool.GetObstacle(spawnPoint.position, spawnPoint.rotation);
    }

    private void SpawnBulletBox(int laneIndex, float zOffset = 0f)
    {
        if (!IsValidLaneIndex(laneIndex))
        {
            return;
        }

        if (bulletBoxPool == null)
        {
            return;
        }

        Transform spawnPoint = spawnPoints[laneIndex];

        Vector3 spawnPosition = spawnPoint.position;
        spawnPosition.z += zOffset;

        bulletBoxPool.GetBulletBox(spawnPosition, spawnPoint.rotation);
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

    private int GetUnusedLaneIndex(List<int> usedLanes)
    {
        List<int> unusedLanes = new List<int>();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!usedLanes.Contains(i))
            {
                unusedLanes.Add(i);
            }
        }

        if (unusedLanes.Count == 0)
        {
            return -1;
        }

        int randomIndex = Random.Range(0, unusedLanes.Count);

        return unusedLanes[randomIndex];
    }

    private int GetMiddleLaneIndex()
    {
        if (!HasSpawnPoints())
        {
            return -1;
        }

        return spawnPoints.Length / 2;
    }

    private bool HasSpawnPoints()
    {
        return spawnPoints != null && spawnPoints.Length > 0;
    }

    private bool IsValidLaneIndex(int laneIndex)
    {
        if (!HasSpawnPoints())
        {
            return false;
        }

        return laneIndex >= 0 && laneIndex < spawnPoints.Length;
    }

    public void StopAndClearStageObjects()
    {
        StopSpawnLoop();

        StageObjectMover[] movers = FindObjectsByType<StageObjectMover>(FindObjectsSortMode.None);

        for (int i = 0; i < movers.Length; i++)
        {
            if (movers[i] == null)
            {
                continue;
            }

            movers[i].gameObject.SetActive(false);
        }

        // 보스 등장 전 일반 스테이지 오브젝트 스폰을 완전히 멈춤
        enabled = false;
    }
}