using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MolePool molePool;
    [SerializeField] private Transform[] holePoints;

    [Header("Spawn Setting")]
    [SerializeField] private float spawnInterval = 1.2f;

    private Coroutine spawnRoutine;
    private bool isSpawning = false;

    private bool[] holeOccupied;

    private void Awake()
    {
        holeOccupied = new bool[holePoints.Length];
    }

    public void StartSpawning()
    {
        if (isSpawning) return;

        ClearAllMoles();

        isSpawning = true;
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
            SpawnMole();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnMole()
    {
        Mole mole = molePool.GetMole();
        if (mole == null) return;

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
        if (molePool != null)
        {
            molePool.ForceHideAll();
        }

        for (int i = 0; i < holeOccupied.Length; i++)
        {
            holeOccupied[i] = false;
        }
    }
}