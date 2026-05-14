using UnityEngine;

[RequireComponent(typeof(Health))]
public class ObstacleBox : MonoBehaviour
{
    [SerializeField] private int damageReward = 5;
    [SerializeField] private PlayerCombatStats playerStats;

    private Health health;
    private bool hasCollided;
    private bool rewardGiven;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        hasCollided = false;
        rewardGiven = false;

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
        if (hasCollided)
        {
            return;
        }

        PlayerCombatStats player = other.GetComponentInParent<PlayerCombatStats>();

        if (player == null)
        {
            return;
        }

        hasCollided = true;

        if (health.CurrentHealth > 0)
        {
            Debug.Log($"장애물 미파괴 충돌 -> 유닛 수 -{health.CurrentHealth}");
            player.ReduceUnitCount(health.CurrentHealth);
        }

        gameObject.SetActive(false);
    }

    private void HandleDied()
    {
        Debug.Log("장애물 파괴됨 -> 아이템 얻음");
        GiveReward();
        gameObject.SetActive(false);
    }

    private void GiveReward()
    {
        if (rewardGiven)
        {
            return;
        }

        rewardGiven = true;

        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerCombatStats>();
        }

        if (playerStats == null)
        {
            return;
        }

        playerStats.IncreaseAttackDamage(damageReward);
        Debug.Log($"공격력 +{damageReward} 보상 지급");
    }
}