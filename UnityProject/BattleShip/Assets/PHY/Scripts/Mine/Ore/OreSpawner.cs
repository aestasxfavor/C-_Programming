using System.Collections.Generic;
using UnityEngine;

public class OreSpawner : MonoBehaviour
{
    [Header("Ore Prefabs")]
    [SerializeField] private GameObject[] orePrefabs;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Settings")]
    [SerializeField] private int spawnCount = 5;

    private void Start()
    {
        SpawnOres();
    }

    private void SpawnOres()
    {
        if (orePrefabs == null || orePrefabs.Length == 0)
        {
            Debug.LogWarning("OreSpawner: 광석 프리팹이 비어 있어요.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("OreSpawner: 스폰 포인트가 비어 있어요.");
            return;
        }

        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        int finalSpawnCount = Mathf.Min(spawnCount, availablePoints.Count);

        for (int i = 0; i < finalSpawnCount; i++)
        {
            int pointIndex = Random.Range(0, availablePoints.Count);
            Transform selectedPoint = availablePoints[pointIndex];

            int oreIndex = Random.Range(0, orePrefabs.Length);
            GameObject selectedOrePrefab = orePrefabs[oreIndex];

            Instantiate(selectedOrePrefab, selectedPoint.position, selectedPoint.rotation);

            availablePoints.RemoveAt(pointIndex);
        }
    }
}