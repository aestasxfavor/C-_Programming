using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{
    [Header("Attack State")]
    [SerializeField] private bool canAttack = false;

    [Header("Attack Settings")]
    //[SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireInterval = 0.3f;
    [SerializeField] private float bulletSpacing = 0.25f;
    [SerializeField] private int maxBulletPerShot = 10;

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
        if (!canAttack)
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
        canAttack = true;
        fireTimer = 0f;

        Debug.Log("Player Attack Unlocked");
    }

    private void Fire()
    {
        if (bulletPool == null)
        {
            Debug.LogWarning("BulletPool이 비어 있어요.");
            return;
        }

        int unitCount = 1;

        if (playerUnitManager != null)
        {
            unitCount = playerUnitManager.CurrentUnitCount;
        }

        if (unitCount <= 0)
        {
            return;
        }

        int attackDamage = 1;

        if (playerCombatStats != null)
        {
            attackDamage = playerCombatStats.AttackDamage;
        }

        int bulletCount = Mathf.Min(unitCount, maxBulletPerShot);

        for (int i = 0; i < bulletCount; i++)
        {
            Vector3 spawnPosition = GetBulletSpawnPosition(i, bulletCount);

            bulletPool.GetBullet(spawnPosition, firePoint.rotation, attackDamage);
        }
    }

    private Vector3 GetBulletSpawnPosition(int index, int totalCount)
    {
        float centerOffset = (totalCount - 1) * 0.5f;
        float xOffset = (index - centerOffset) * bulletSpacing;

        Vector3 right = firePoint.right;
        Vector3 spawnPosition = firePoint.position + right * xOffset;

        return spawnPosition;
    }
}