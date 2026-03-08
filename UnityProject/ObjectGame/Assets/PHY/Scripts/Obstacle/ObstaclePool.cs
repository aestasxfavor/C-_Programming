using System.Collections.Generic;
using UnityEngine;

public class ObstaclePool : MonoBehaviour
{
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private int obstaclePoolSize = 10;

    private List<GameObject> obstaclePool = new List<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < obstaclePoolSize; i++)
        {
            GameObject obj = Instantiate(obstaclePrefab);
            obj.SetActive(false);
            obstaclePool.Add(obj);
        }
    }

    public GameObject GetObstacle()
    {
        for (int i = 0; i < obstaclePool.Count; i++)
        {
            if (!obstaclePool[i].activeSelf)
            {
                obstaclePool[i].SetActive(true);
                return obstaclePool[i];
            }
        }

        // 다 사용 중이면 하나 더 만들기
        GameObject newObj = Instantiate(obstaclePrefab);
        newObj.SetActive(false);
        obstaclePool.Add(newObj);
        return newObj;
    }
}