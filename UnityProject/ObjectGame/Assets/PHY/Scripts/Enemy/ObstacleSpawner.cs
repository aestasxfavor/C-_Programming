using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private ObstaclePool obstaclePool;
    [SerializeField] private Transform player;

    // 기본 생성 설정
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float spawnDistance = 8f;
    private float timer = 0f;

    // 난이도 조절
    [SerializeField] private float currentObstacleSpeed = 3f;
    [SerializeField] private float speedIncrease = 0.5f;      // 속도 증가량
    [SerializeField] private float intervalDecrease = 0.1f;   // 생성 간격 감소량
    [SerializeField] private float minSpawnInterval = 0.4f;   // 최소 생성 간격
    private float nextDifficultyTime = 10f;                   // 10초마다 증가

    void Update()
    {
        timer += Time.deltaTime;

        // 20초 후 난이도 상승 -> 10초마다 상승으로 변경
        if (Time.time >= nextDifficultyTime)
        {
            IncreaseDifficulty();
            nextDifficultyTime += 10f;   // 다음 상승 시간 예약
        }

        // 스폰 처리
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnObstacle();
        }
    }

    private void IncreaseDifficulty()
    {
        currentObstacleSpeed += speedIncrease;

        spawnInterval -= intervalDecrease;
        if (spawnInterval < minSpawnInterval)
        {
            spawnInterval = minSpawnInterval;

        }

        Debug.Log("난이도 상승! 현재 속도: " + currentObstacleSpeed + ", 인터벌: " + spawnInterval);
    }

    private void SpawnObstacle()
    {
        Vector2 spawnPos = GetRandomSpawnPosition();
        GameObject obj = obstaclePool.GetObstacle();

        obj.transform.position = spawnPos;

        Vector2 dir = (player.position - obj.transform.position);
        obj.GetComponent<Obstacle>().SetDirection(dir);

        obj.GetComponent<Obstacle>().SetSpeed(currentObstacleSpeed);
    }

    private Vector2 GetRandomSpawnPosition()
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