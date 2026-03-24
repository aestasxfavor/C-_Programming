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

    private bool[] holeOccupied;

    private float currentSpawnInterval;
    private float currentTrapChance;
    private float spawnStartTime;

    private void Awake()
    {
        holeOccupied = new bool[holePoints.Length];
    }

    public void StartSpawning()
    {
        if (isSpawning) return;

        ClearAllMoles();

        isSpawning = true;
        spawnStartTime = Time.time;

        currentSpawnInterval = earlySpawnInterval;
        currentTrapChance = earlyTrapChance;

        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
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
            UpdateDifficulty();

            SpawnMole();

            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    private void UpdateDifficulty()
    {
        float elapsedTime = Time.time - spawnStartTime;

        if (elapsedTime < 10f)
        {
            currentSpawnInterval = earlySpawnInterval;
            currentTrapChance = earlyTrapChance;
        }
        else if (elapsedTime < 20f)
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
        List<int> availableHoles = new List<int>();

        for (int i = 0; i < holeOccupied.Length; i++)
        {
            if (!holeOccupied[i])
            {
                availableHoles.Add(i);
            }
        }

        if (availableHoles.Count == 0) return;

        int randomListIndex = Random.Range(0, availableHoles.Count);
        int holeIndex = availableHoles[randomListIndex];

        MolePool selectedPool = Random.value < currentTrapChance ? trapMolePool : normalMolePool;
        if (selectedPool == null) return;

        Mole mole = selectedPool.GetMole();
        if (mole == null) return;

        holeOccupied[holeIndex] = true;

        Vector3 spawnPos = holePoints[holeIndex].position;
        mole.ActivateMole(spawnPos, holeIndex, ReleaseHole);
    }

    private void ReleaseHole(int holeIndex)
    {
        if (holeIndex < 0 || holeIndex >= holeOccupied.Length) return;
        holeOccupied[holeIndex] = false;
    }

    private void ClearAllMoles()
    {
        if (normalMolePool != null)
        {
            normalMolePool.ForceHideAll();
        }

        if (trapMolePool != null)
        {
            trapMolePool.ForceHideAll();
        }

        for (int i = 0; i < holeOccupied.Length; i++)
        {
            holeOccupied[i] = false;
        }
    }
}