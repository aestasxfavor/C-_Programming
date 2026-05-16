using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private int initialSize = 30;

    private readonly Queue<Bullet> bulletPool = new Queue<Bullet>();

    private void Awake()
    {
        CreateInitialBullets();
    }

    private void CreateInitialBullets()
    {
        for (int i = 0; i < initialSize; i++)
        {
            Bullet bullet = CreateBullet();
            bulletPool.Enqueue(bullet);
        }
    }

    private Bullet CreateBullet()
    {
        Bullet bullet = Instantiate(bulletPrefab, transform);

        bullet.gameObject.SetActive(false);
        bullet.SetPool(this);

        return bullet;
    }

    public Bullet GetBullet(Vector3 position, Quaternion rotation, int damage)
    {
        Bullet bullet;

        if (bulletPool.Count > 0)
        {
            bullet = bulletPool.Dequeue();
        }
        else
        {
            bullet = CreateBullet();
        }

        bullet.transform.SetPositionAndRotation(position, rotation);
        bullet.SetDamage(damage);
        bullet.gameObject.SetActive(true);

        return bullet;
    }

    public void ReturnBullet(Bullet bullet)
    {
        if (bullet == null)
        {
            return;
        }

        bullet.gameObject.SetActive(false);
        bullet.transform.SetParent(transform);

        bulletPool.Enqueue(bullet);
    }
}