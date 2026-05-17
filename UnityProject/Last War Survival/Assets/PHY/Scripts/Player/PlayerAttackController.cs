using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{
    [Header("Attack State")]
    [SerializeField] private bool isAttackUnlocked = false;

    [Header("Attack Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireInterval = 0.3f;
    [SerializeField] private float bulletSpacing = 0.25f;
    [SerializeField] private int maxBulletsPerShot = 10;

    [Header("References")]
    [SerializeField] private PlayerUnitManager playerUnitManager;
    [SerializeField] private PlayerCombatStats playerCombatStats;
    [SerializeField] private BulletPool bulletPool;

    private float fireTimer;

    private void Start()
    {
        if (playerUnitManager == null)
        {
            playerUnitManager = GetComponent<PlayerUnitManager>();
        }

        if (playerCombatStats == null)
        {
            playerCombatStats = GetComponent<PlayerCombatStats>();
        }

        if (firePoint == null)
        {
            firePoint = transform;
        }

        if (bulletPool == null)
        {
            bulletPool = FindFirstObjectByType<BulletPool>();
        }
    }

    private void Update()
    {
        if (!isAttackUnlocked)
        {
            return;
        }

        fireTimer += Time.deltaTime;

        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            Fire();
        }
    }

    public void UnlockAttack()
    {
        isAttackUnlocked = true;
        fireTimer = 0f;
    }

    private void Fire()
    {
        if (bulletPool == null)
        {
            return;
        }

        int currentUnitCount = 1;

        if (playerUnitManager != null)
        {
            currentUnitCount = playerUnitManager.CurrentUnitCount;
        }

        if (currentUnitCount <= 0)
        {
            return;
        }

        int damage = 1;

        if (playerCombatStats != null)
        {
            damage = playerCombatStats.AttackDamage;
        }

        // 유닛 수만큼 발사하되, 한 번에 나가는 총알 수는 제한
        int bulletCount = Mathf.Min(currentUnitCount, maxBulletsPerShot);

        for (int i = 0; i < bulletCount; i++)
        {
            Vector3 spawnPosition = GetBulletSpawnPosition(i, bulletCount);

            bulletPool.GetBullet(spawnPosition, firePoint.rotation, damage);
        }
    }

    private Vector3 GetBulletSpawnPosition(int index, int totalCount)
    {
        float centerOffset = (totalCount - 1) * 0.5f;
        float xOffset = (index - centerOffset) * bulletSpacing;

        Vector3 spawnPosition = firePoint.position + firePoint.right * xOffset;

        return spawnPosition;
    }
}