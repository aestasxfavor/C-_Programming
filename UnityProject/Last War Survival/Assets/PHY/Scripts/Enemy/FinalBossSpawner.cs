using System;
using UnityEngine;

public class FinalBossSpawner : MonoBehaviour
{
    [Header("Final Boss")]
    [SerializeField] private GameObject finalBossPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("References")]
    [SerializeField] private StageObjectSpawner stageObjectSpawner;

    private bool hasSpawned;
    private Health spawnedBossHealth;

    public bool HasSpawned => hasSpawned;

    public event Action OnFinalBossDied;

    public void SpawnFinalBoss()
    {
        Debug.Log("SpawnFinalBoss 호출됨");

        if (hasSpawned)
        {
            return;
        }

        if (finalBossPrefab == null)
        {
            Debug.LogWarning("Final Boss Prefab이 비어 있어요.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("Final Boss Spawn Point가 비어 있어요.");
            return;
        }

        hasSpawned = true;

        if (stageObjectSpawner != null)
        {
            stageObjectSpawner.enabled = false;
        }

        GameObject bossObject = Instantiate(finalBossPrefab, spawnPoint.position, spawnPoint.rotation);

        spawnedBossHealth = bossObject.GetComponent<Health>();

        if (spawnedBossHealth == null)
        {
            spawnedBossHealth = bossObject.GetComponentInChildren<Health>();
        }

        if (spawnedBossHealth != null)
        {
            spawnedBossHealth.OnDied += HandleFinalBossDied;
        }
        else
        {
            Debug.LogWarning("생성된 보스에서 Health를 찾지 못했어요.");
        }

        Debug.Log("Final Boss Spawned");
    }

    private void HandleFinalBossDied()
    {
        if (spawnedBossHealth != null)
        {
            spawnedBossHealth.OnDied -= HandleFinalBossDied;
        }

        Debug.Log("Final Boss Died");

        OnFinalBossDied?.Invoke();
    }
}