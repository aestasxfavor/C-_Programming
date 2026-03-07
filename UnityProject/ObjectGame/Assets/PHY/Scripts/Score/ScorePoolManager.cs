using System.Collections.Generic;
using UnityEngine;

public class ScorePoolManager : MonoBehaviour
{
    [SerializeField] private GameObject scorePrefab;
    [SerializeField] private int scorePoolSize = 5;
    [SerializeField] private float spawnInterval = 3f;

    private List<GameObject> scorePool = new List<GameObject>();
    private float timer = 0f;

    private void Start()
    {
        // Ç® »ý¼º
        for (int i = 0; i < scorePoolSize; i++)
        {
            GameObject obj = Instantiate(scorePrefab);
            obj.SetActive(false);
            scorePool.Add(obj);
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

    private void SpawnScoreItem()
    {
        GameObject obj = GetPooledObject();
        if (obj == null) return;

        obj.transform.position = GetRandomPosition();
        obj.SetActive(true);
    }

    private GameObject GetPooledObject()
    {
        for (int i = 0; i < scorePool.Count; i++)
        {
            if (!scorePool[i].activeSelf)
            {
                return scorePool[i];
            }
        }
        return null;

    }

    private Vector2 GetRandomPosition()
    {
        float x = Random.Range(-7f, 7f);
        float y = Random.Range(-4f, 4f);
        return new Vector2(x, y);
    }
}