using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private int poolSize = 30;

    private readonly Queue<Bullet> pooledBullets = new Queue<Bullet>();

    private void Awake()
    {
        CreatePool();
    }

    private void CreatePool()
    {
        if (bulletPrefab == null)
        {
            return;
        }

        // 게임 중 Instantiate 호출을 줄이기 위해 총알을 미리 생성
        for (int i = 0; i < poolSize; i++)
        {
            Bullet bullet = CreateBullet();
            pooledBullets.Enqueue(bullet);
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

        if (pooledBullets.Count > 0)
        {
            bullet = pooledBullets.Dequeue();
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

        // 사용이 끝난 총알은 비활성화 후 다시 풀에 보관
        bullet.gameObject.SetActive(false);
        bullet.transform.SetParent(transform);

        pooledBullets.Enqueue(bullet);
    }
}