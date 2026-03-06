using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private ObstaclePool obstaclePool;
    [SerializeField] private Transform player;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float spawnDistance = 8f;

    private float timer = 0f;

    // 난이도용
    private float playTime = 0f;
    private bool difficultyUp = false;

    [SerializeField] private float currentObstacleSpeed = 3f;
    [SerializeField] private float increaseSpeed = 6f;

    void Update()
    {
        timer += Time.deltaTime;
        playTime += Time.deltaTime;

        // 20초 후 난이도 상승
        if (!difficultyUp && playTime >= 20f)
        {
            difficultyUp = true;
            spawnInterval = 0.8f;   // 난이도 증가
            currentObstacleSpeed = increaseSpeed;
        }

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnObstacle();
        }
    }

    void SpawnObstacle()
    {
        Vector2 spawnPos = GetRandomSpawnPosition();
        GameObject obj = obstaclePool.GetObstacle();

        obj.transform.position = spawnPos;

        Vector2 dir = (player.position - obj.transform.position);
        obj.GetComponent<Obstacle>().SetDirection(dir);

        obj.GetComponent<Obstacle>().SetSpeed(currentObstacleSpeed);
    }

    Vector2 GetRandomSpawnPosition()
    {
        int side = Random.Range(0, 4);
        Vector2 pos = Vector2.zero;

        if (side == 0)
        {
            pos = new Vector2(0, 1) * spawnDistance;
        }
        else if (side == 1)
        {
            pos = new Vector2(0, -1) * spawnDistance;
        }
        else if (side == 2)
        {
            pos = new Vector2(-1, 0) * spawnDistance;
        }
        else
        {
            pos = new Vector2(1, 0) * spawnDistance;
        }

        return pos;
    }
}