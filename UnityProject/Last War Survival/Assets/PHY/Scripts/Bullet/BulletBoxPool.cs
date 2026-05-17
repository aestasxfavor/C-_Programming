using System.Collections.Generic;
using UnityEngine;

public class BulletBoxPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject bulletBoxPrefab;
    [SerializeField] private int poolSize = 1;

    private readonly List<GameObject> pooledBulletBoxes = new List<GameObject>();

    private void Awake()
    {
        CreatePool();
    }

    private void CreatePool()
    {
        if (bulletBoxPrefab == null)
        {
            return;
        }

        // 시작 아이템은 자주 생성되지 않지만, 풀링 구조를 맞추기 위해 미리 생성
        for (int i = 0; i < poolSize; i++)
        {
            CreateBulletBox();
        }
    }

    private GameObject CreateBulletBox()
    {
        GameObject bulletBox = Instantiate(bulletBoxPrefab, transform);

        bulletBox.SetActive(false);
        pooledBulletBoxes.Add(bulletBox);

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
        for (int i = 0; i < pooledBulletBoxes.Count; i++)
        {
            if (!pooledBulletBoxes[i].activeSelf)
            {
                return pooledBulletBoxes[i];
            }
        }

        return null;
    }
}