using System.Collections.Generic;
using UnityEngine;

public class GatePool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject[] gatePrefabs;
    [SerializeField] private int poolSizePerPrefab = 2;

    private readonly List<GameObject> pool = new();

    private void Awake()
    {
        CreatePool();
    }

    private void CreatePool()
    {
        if (gatePrefabs == null || gatePrefabs.Length == 0)
        {
            Debug.LogWarning("Gate Prefabs가 비어 있습니다.");
            return;
        }

        for (int i = 0; i < gatePrefabs.Length; i++)
        {
            for (int j = 0; j < poolSizePerPrefab; j++)
            {
                CreateGate(gatePrefabs[i]);
            }
        }
    }

    private GameObject CreateGate(GameObject gatePrefab)
    {
        GameObject gate = Instantiate(gatePrefab, transform);
        gate.SetActive(false);
        pool.Add(gate);

        return gate;
    }

    public GameObject GetGate(Vector3 position, Quaternion rotation)
    {
        GameObject gate = GetRandomInactiveGate();

        if (gate == null)
        {
            gate = CreateRandomGate();
        }

        if (gate == null)
        {
            return null;
        }

        gate.transform.SetPositionAndRotation(position, rotation);

        if (gate.TryGetComponent(out StageObjectMover mover))
        {
            mover.ResumeMove();
        }

        gate.SetActive(true);

        return gate;
    }

    private GameObject GetRandomInactiveGate()
    {
        List<GameObject> inactiveGates = new List<GameObject>();

        foreach (GameObject gate in pool)
        {
            if (!gate.activeInHierarchy)
            {
                inactiveGates.Add(gate);
            }
        }

        if (inactiveGates.Count == 0)
        {
            return null;
        }

        int index = Random.Range(0, inactiveGates.Count);
        return inactiveGates[index];
    }

    private GameObject CreateRandomGate()
    {
        if (gatePrefabs == null || gatePrefabs.Length == 0)
        {
            Debug.LogWarning("Gate Prefabs가 비어 있습니다.");
            return null;
        }

        int index = Random.Range(0, gatePrefabs.Length);
        return CreateGate(gatePrefabs[index]);
    }
}