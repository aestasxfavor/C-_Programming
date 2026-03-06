using System.Collections.Generic;
using UnityEngine;

public class ScorePoolManager : MonoBehaviour
{
    [SerializeField] private GameObject scorePrefab;
    [SerializeField] private int poolSize = 5;
    [SerializeField] private float spawnInterval = 3f;

    private List<GameObject> pool = new List<GameObject>();
    private float timer = 0f;

    private void Start()
    {
        // Ç® »ý¼º
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(scorePrefab);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnScoreItem();
        }
    }

    void SpawnScoreItem()
    {
        GameObject obj = GetPooledObject();
        if (obj == null) return;

        obj.transform.position = GetRandomPosition();
        obj.SetActive(true);
    }

    GameObject GetPooledObject()
    {
        foreach (var obj in pool)
        {
            if (!obj.activeSelf)
            {
                return obj;
            }
        }
        return null;
    }

    Vector2 GetRandomPosition()
    {
        float x = Random.Range(-7f, 7f);
        float y = Random.Range(-4f, 4f);
        return new Vector2(x, y);
    }
}