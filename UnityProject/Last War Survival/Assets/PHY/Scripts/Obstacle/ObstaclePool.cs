using System.Collections.Generic;
using UnityEngine;

public class ObstaclePool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private int poolSize = 10;

    private readonly List<GameObject> pooledObstacles = new List<GameObject>();

    private void Awake()
    {
        CreatePool();
    }

    private void CreatePool()
    {
        if (obstaclePrefab == null)
        {
            return;
        }

        // 반복해서 등장하는 장애물을 미리 생성해서 재사용
        for (int i = 0; i < poolSize; i++)
        {
            CreateObstacle();
        }
    }

    private GameObject CreateObstacle()
    {
        GameObject obstacle = Instantiate(obstaclePrefab, transform);

        obstacle.SetActive(false);
        pooledObstacles.Add(obstacle);

        return obstacle;
    }

    public GameObject GetObstacle(Vector3 position, Quaternion rotation)
    {
        GameObject obstacle = GetInactiveObstacle();

        if (obstacle == null)
        {
            obstacle = CreateObstacle();
        }

        if (obstacle == null)
        {
            return null;
        }

        obstacle.transform.SetPositionAndRotation(position, rotation);

        if (obstacle.TryGetComponent(out StageObjectMover mover))
        {
            mover.ResumeMove();
        }

        obstacle.SetActive(true);

        return obstacle;
    }

    private GameObject GetInactiveObstacle()
    {
        for (int i = 0; i < pooledObstacles.Count; i++)
        {
            if (pooledObstacles[i] == null)
            {
                continue;
            }

            if (!pooledObstacles[i].activeSelf)
            {
                return pooledObstacles[i];
            }
        }

        return null;
    }
}