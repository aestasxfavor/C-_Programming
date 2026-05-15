using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private int damage = 1;

    private float lifeTimer;
    private bool hasHit;
    private BulletPool bulletPool;

    private void OnEnable()
    {
        lifeTimer = 0f;
        hasHit = false;
    }

    private void Update()
    {
        Move();
        CheckLifeTime();
    }

    public void SetPool(BulletPool pool)
    {
        bulletPool = pool;
    }

    public void SetDamage(int value)
    {
        damage = value;
    }

    private void Move()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    private void CheckLifeTime()
    {
        lifeTimer += Time.deltaTime;

        if (lifeTimer >= lifeTime)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit)
        {
            return;
        }

        Health targetHealth = other.GetComponentInParent<Health>();

        if (targetHealth == null)
        {
            return;
        }

        hasHit = true;

        targetHealth.TakeDamage(damage);

        Debug.Log($"Bullet Hit: {other.name}, Damage: {damage}");

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (bulletPool != null)
        {
            bulletPool.ReturnBullet(this);
            return;
        }

        gameObject.SetActive(false);
    }
}