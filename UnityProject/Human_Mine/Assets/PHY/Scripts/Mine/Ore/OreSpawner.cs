using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OreSpawner : MonoBehaviour
{
    [Header("Ore Prefabs")]
    [SerializeField] private GameObject[] orePrefabs;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Settings")]
    [SerializeField] private int spawnCount = 8;
    [SerializeField] private float respawnDelay = 5f;
    [SerializeField] private float minSpawnDistance = 2.5f;

    [Header("Pool Settings")]
    [SerializeField] private int poolSize = 20;

    [Header("Respawn Retry Settings")]
    [SerializeField] private float retryDelay = 1f;

    private readonly List<PoolingOre> orePool = new();
    private readonly List<Transform> occupiedSpawnPoints = new();

    private void Start()
    {
        CreatePool();
        SpawnInitialOres();
    }

    private void CreatePool()
    {
        if (orePrefabs == null || orePrefabs.Length == 0)
        {
            Debug.LogWarning("OreSpawner: 광석 프리팹이 비어 있어요.");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject prefab = orePrefabs[Random.Range(0, orePrefabs.Length)];
            GameObject oreObject = Instantiate(prefab, transform);

            PoolingOre pooledOre = oreObject.GetComponent<PoolingOre>();

            if (pooledOre == null)
            {
                pooledOre = oreObject.AddComponent<PoolingOre>();
            }

            pooledOre.Init(this);
            oreObject.SetActive(false);
            orePool.Add(pooledOre);
        }
    }

    private void SpawnInitialOres()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("OreSpawner: 스폰 포인트가 비어 있어요.");
            return;
        }

        int finalSpawnCount = Mathf.Min(spawnCount, spawnPoints.Length);

        for (int i = 0; i < finalSpawnCount; i++)
        {
            bool spawned = SpawnOreAtRandomPoint();

            if (!spawned)
            {
                Debug.LogWarning("OreSpawner: 거리 조건 때문에 초기 스폰을 더 이상 할 수 없어요.");
                return;
            }
        }
    }

    private bool SpawnOreAtRandomPoint()
    {
        PoolingOre ore = GetRandomInactiveOre();

        if (ore == null)
        {
            Debug.LogWarning("OreSpawner: 사용 가능한 풀 광석이 없어요.");
            return false;
        }

        Transform spawnPoint = GetRandomEmptySpawnPoint();

        if (spawnPoint == null)
        {
            return false;
        }

        occupiedSpawnPoints.Add(spawnPoint);

        ore.SetSpawnPoint(spawnPoint);
        ore.gameObject.SetActive(true);

        return true;
    }

    private PoolingOre GetRandomInactiveOre()
    {
        List<PoolingOre> inactiveOres = new();

        foreach (PoolingOre ore in orePool)
        {
            if (ore == null)
            {
                continue;
            }

            if (ore.gameObject.activeInHierarchy)
            {
                continue;
            }

            inactiveOres.Add(ore);
        }

        if (inactiveOres.Count == 0)
        {
            return null;
        }

        return inactiveOres[Random.Range(0, inactiveOres.Count)];
    }

    private Transform GetRandomEmptySpawnPoint()
    {
        List<Transform> availablePoints = new();

        foreach (Transform point in spawnPoints)
        {
            if (point == null)
            {
                continue;
            }

            if (occupiedSpawnPoints.Contains(point))
            {
                continue;
            }

            if (IsTooCloseToActiveOre(point))
            {
                continue;
            }

            availablePoints.Add(point);
        }

        if (availablePoints.Count == 0)
        {
            return null;
        }

        return availablePoints[Random.Range(0, availablePoints.Count)];
    }

    private bool IsTooCloseToActiveOre(Transform spawnPoint)
    {
        foreach (PoolingOre ore in orePool)
        {
            if (ore == null)
            {
                continue;
            }

            if (!ore.gameObject.activeInHierarchy)
            {
                continue;
            }

            float distance = GetHorizontalDistance(spawnPoint.position, ore.transform.position);

            if (distance < minSpawnDistance)
            {
                return true;
            }
        }

        return false;
    }

    private float GetHorizontalDistance(Vector3 a, Vector3 b)
    {
        Vector2 pointA = new Vector2(a.x, a.z);
        Vector2 pointB = new Vector2(b.x, b.z);

        return Vector2.Distance(pointA, pointB);
    }

    public void ReleaseOre(PoolingOre ore)
    {
        if (ore == null)
        {
            return;
        }

        Transform spawnPoint = ore.CurrentSpawnPoint;

        if (spawnPoint != null)
        {
            occupiedSpawnPoints.Remove(spawnPoint);
        }

        ore.gameObject.SetActive(false);

        StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        while (!SpawnOreAtRandomPoint())
        {
            yield return new WaitForSeconds(retryDelay);
        }
    }
}