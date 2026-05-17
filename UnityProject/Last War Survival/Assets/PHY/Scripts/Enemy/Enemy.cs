using UnityEngine;

[RequireComponent(typeof(Health))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private int collisionDamage = 1;

    private Health health;
    private bool hasCollided;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        hasCollided = false;

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

        PlayerUnitManager playerUnitManager = other.GetComponentInParent<PlayerUnitManager>();

        if (playerUnitManager == null)
        {
            return;
        }

        hasCollided = true;

        playerUnitManager.ReduceUnitCount(collisionDamage);

        gameObject.SetActive(false);
    }

    private void HandleDied()
    {
        gameObject.SetActive(false);
    }
}