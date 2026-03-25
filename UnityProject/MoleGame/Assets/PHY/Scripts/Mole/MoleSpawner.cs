using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MolePool normalMolePool;
    [SerializeField] private MolePool trapMolePool;
    [SerializeField] private Transform[] holePoints;

    [Header("Spawn Setting")]
    [SerializeField] private float earlySpawnInterval = 1.2f;
    [SerializeField] private float midSpawnInterval = 0.9f;
    [SerializeField] private float lateSpawnInterval = 0.6f;

    [Header("Trap Chance")]
    [SerializeField, Range(0f, 1f)] private float earlyTrapChance = 0.2f;
    [SerializeField, Range(0f, 1f)] private float midTrapChance = 0.35f;
    [SerializeField, Range(0f, 1f)] private float lateTrapChance = 0.5f;

    private Coroutine spawnRoutine;
    private bool isSpawning = false;

    private bool[] isFullHole;

    private float currentSpawnInterval;
    private float currentTrapChance;
    private float spawnStartTime;

    private void Awake()
    {
        isFullHole = new bool[holePoints.Length];
    }

    public void StartSpawn()
    {
        if (isSpawning) return;

        ClearAllMoles();

        isSpawning = true;
        spawnStartTime = Time.time;

        currentSpawnInterval = earlySpawnInterval;
        currentTrapChance = earlyTrapChance;

        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawn()
    {
        if (!isSpawning) return;

        isSpawning = false;

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        ClearAllMoles();
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            IncreaseInterval();

            SpawnMole();

            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    private void IncreaseInterval()
    {
        float passedTime = Time.time - spawnStartTime;

        if (passedTime < 10f)
        {
            currentSpawnInterval = earlySpawnInterval;
            currentTrapChance = earlyTrapChance;
        }
        else if (passedTime < 20f)
        {
            currentSpawnInterval = midSpawnInterval;
            currentTrapChance = midTrapChance;
        }
        else
        {
            currentSpawnInterval = lateSpawnInterval;
            currentTrapChance = lateTrapChance;
        }
    }

    private void SpawnMole()
    {
        List<int> emptyHoles = new List<int>();

        for (int i = 0; i < isFullHole.Length; i++)
        {
            if (!isFullHole[i])
            {
                emptyHoles.Add(i);
            }
        }

        if (emptyHoles.Count == 0) return;

        int randomListIndex = Random.Range(0, emptyHoles.Count);
        int holeIndex = emptyHoles[randomListIndex];

        MolePool selectedPool = Random.value < currentTrapChance ? trapMolePool : normalMolePool;
        if (selectedPool == null) return;

        Mole mole = selectedPool.GetMole();
        if (mole == null) return;

        isFullHole[holeIndex] = true;

        Vector3 spawnPos = holePoints[holeIndex].position;
        mole.ActivateMole(spawnPos, holeIndex, EmptyHole);
    }

    private void EmptyHole(int holeIndex)
    {
        if (holeIndex < 0 || holeIndex >= isFullHole.Length) return;
        isFullHole[holeIndex] = false;
    }

    private void ClearAllMoles()
    {
        if (normalMolePool != null)
        {
            normalMolePool.HideAllMoles();
        }

        if (trapMolePool != null)
        {
            trapMolePool.HideAllMoles();
        }

        for (int i = 0; i < isFullHole.Length; i++)
        {
            isFullHole[i] = false;
        }
    }
}