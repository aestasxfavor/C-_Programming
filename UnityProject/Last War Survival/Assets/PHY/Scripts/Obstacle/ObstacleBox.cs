using UnityEngine;

[RequireComponent(typeof(Health))]
public class ObstacleBox : MonoBehaviour
{
    [SerializeField] private int damageReward = 5;

    // 프리팹 참조가 비어 있을 경우 보상 지급 시 런타임에서 자동 탐색
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
            Debug.LogWarning("PlayerCombatStats를 찾지 못해서 공격력 보상 지급 실패");
            return;
        }

        playerStats.IncreaseAttackDamage(damageReward);
        Debug.Log($"공격력 +{damageReward} 보상 지급 완료");
    }
}