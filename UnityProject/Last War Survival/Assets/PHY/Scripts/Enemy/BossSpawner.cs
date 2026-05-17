using System;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [Header("Final Boss")]
    [SerializeField] private GameObject finalBossPrefab;
    [SerializeField] private Transform bossSpawnPoint;

    [Header("References")]
    [SerializeField] private StageObjectSpawner stageObjectSpawner;

    private bool hasSpawnedBoss;
    private GameObject bossObject;
    private Health bossHealth;

    public bool HasSpawned => hasSpawnedBoss;

    public event Action OnFinalBossDied;

    private void OnDestroy()
    {
        if (bossHealth != null)
        {
            bossHealth.OnDied -= HandleFinalBossDied;
        }
    }

    public void SpawnFinalBoss()
    {
        if (hasSpawnedBoss)
        {
            return;
        }

        if (finalBossPrefab == null)
        {
            return;
        }

        if (bossSpawnPoint == null)
        {
            return;
        }

        hasSpawnedBoss = true;

        // 보스 등장 전 기존 적, 아이템, 게이트 흐름을 정리
        if (stageObjectSpawner != null)
        {
            stageObjectSpawner.StopAndClearStageObjects();
        }

        bossObject = Instantiate(finalBossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
        bossHealth = GetBossHealth(bossObject);

        if (bossHealth != null)
        {
            bossHealth.OnDied += HandleFinalBossDied;
        }
    }

    private Health GetBossHealth(GameObject targetBoss)
    {
        if (targetBoss == null)
        {
            return null;
        }

        Health health = targetBoss.GetComponent<Health>();

        if (health != null)
        {
            return health;
        }

        return targetBoss.GetComponentInChildren<Health>();
    }

    private void HandleFinalBossDied()
    {
        if (bossHealth != null)
        {
            bossHealth.OnDied -= HandleFinalBossDied;
        }

        if (bossObject != null)
        {
            bossObject.SetActive(false);
        }

        OnFinalBossDied?.Invoke();
    }
}