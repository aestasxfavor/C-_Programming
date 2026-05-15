using System.Collections.Generic;
using UnityEngine;

public class BulletBoxPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject bulletBoxPrefab;
    [SerializeField] private int poolSize = 1;

    private readonly List<GameObject> pool = new();

    private void Awake()
    {
        CreatePool();
    }

    private void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateBulletBox();
        }
    }

    private GameObject CreateBulletBox()
    {
        GameObject bulletBox = Instantiate(bulletBoxPrefab, transform);
        bulletBox.SetActive(false);
        pool.Add(bulletBox);

        return bulletBox;
    }

    public GameObject GetBulletBox(Vector3 position, Quaternion rotation)
    {
        GameObject bulletBox = GetInactiveBulletBox();

        if (bulletBox == null)
        {
            bulletBox = CreateBulletBox();
        }

        bulletBox.transform.SetPositionAndRotation(position, rotation);

        if (bulletBox.TryGetComponent(out StageObjectMover mover))
        {
            mover.ResumeMove();
        }

        bulletBox.SetActive(true);

        return bulletBox;
    }

    private GameObject GetInactiveBulletBox()
    {
        foreach (GameObject bulletBox in pool)
        {
            if (!bulletBox.activeInHierarchy)
            {
                return bulletBox;
            }
        }

        return null;
    }
}