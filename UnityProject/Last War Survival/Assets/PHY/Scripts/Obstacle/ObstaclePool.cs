using System.Collections.Generic;
using UnityEngine;

public class ObstaclePool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private int poolSize = 10;

    private readonly List<GameObject> pool = new();

    private void Awake()
    {
        CreatePool();
    }

    private void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateObstacle();
        }
    }

    private GameObject CreateObstacle()
    {
        GameObject obstacle = Instantiate(obstaclePrefab, transform);
        obstacle.SetActive(false);
        pool.Add(obstacle);

        return obstacle;
    }

    public GameObject GetObstacle(Vector3 position, Quaternion rotation)
    {
        GameObject obstacle = GetInactiveObstacle();

        if (obstacle == null)
        {
            obstacle = CreateObstacle();
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
        foreach (GameObject obstacle in pool)
        {
            if (!obstacle.activeInHierarchy)
            {
                return obstacle;
            }
        }

        return null;
    }
}