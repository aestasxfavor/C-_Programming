using UnityEngine;

[RequireComponent(typeof(Health))]
public class ObstacleBox : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [SerializeField] private int damageReward = 5;
    [SerializeField] private int collisionDamage = 1;

    [Header("References")]
    [SerializeField] private PlayerCombatStats playerStats;

    private Health health;
    private bool hasHitPlayer;
    private bool hasGivenReward;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        hasHitPlayer = false;
        hasGivenReward = false;

        if (health != null)
        {
            health.OnDied += HandleDied;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDied -= HandleDied;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHitPlayer)
        {
            return;
        }

        PlayerUnitManager playerUnitManager = other.GetComponentInParent<PlayerUnitManager>();

        if (playerUnitManager == null)
        {
            return;
        }

        hasHitPlayer = true;

        // 장애물을 부수지 못하고 직접 충돌하면 정해진 수만큼 유닛 감소
        playerUnitManager.ReduceUnitCount(collisionDamage);
        Debug.Log($"플레이어가 장애물에 부딪힘 -> 유닛 감소: {collisionDamage}.");

        gameObject.SetActive(false);
    }

    private void HandleDied()
    {
        GiveReward();

        gameObject.SetActive(false);
    }

    private void GiveReward()
    {
        if (hasGivenReward)
        {
            return;
        }

        hasGivenReward = true;

        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerCombatStats>();
        }

        if (playerStats == null)
        {
            return;
        }

        // 총알로 파괴했을 때만 공격력 보상 지급
        playerStats.IncreaseAttackDamage(damageReward);
    }
}