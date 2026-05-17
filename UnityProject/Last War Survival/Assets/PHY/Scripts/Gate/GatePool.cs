using System.Collections.Generic;
using UnityEngine;

public class GatePool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject[] gatePrefabs;
    [SerializeField] private int poolSizePerGate = 2;

    private readonly List<GameObject> pooledGates = new List<GameObject>();

    private void Awake()
    {
        CreatePool();
    }

    private void CreatePool()
    {
        if (gatePrefabs == null || gatePrefabs.Length == 0)
        {
            return;
        }

        // 게이트 세트 프리팹마다 사용할 오브젝트를 미리 생성
        for (int i = 0; i < gatePrefabs.Length; i++)
        {
            for (int j = 0; j < poolSizePerGate; j++)
            {
                CreateGate(gatePrefabs[i]);
            }
        }
    }

    private GameObject CreateGate(GameObject gatePrefab)
    {
        if (gatePrefab == null)
        {
            return null;
        }

        GameObject gate = Instantiate(gatePrefab, transform);

        gate.SetActive(false);
        pooledGates.Add(gate);

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
        int inactiveCount = CountInactiveGates();

        if (inactiveCount == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, inactiveCount);
        int currentIndex = 0;

        for (int i = 0; i < pooledGates.Count; i++)
        {
            if (pooledGates[i].activeSelf)
            {
                continue;
            }

            if (currentIndex == randomIndex)
            {
                return pooledGates[i];
            }

            currentIndex++;
        }

        return null;
    }

    private int CountInactiveGates()
    {
        int count = 0;

        for (int i = 0; i < pooledGates.Count; i++)
        {
            if (!pooledGates[i].activeSelf)
            {
                count++;
            }
        }

        return count;
    }

    private GameObject CreateRandomGate()
    {
        if (gatePrefabs == null || gatePrefabs.Length == 0)
        {
            return null;
        }

        int index = Random.Range(0, gatePrefabs.Length);

        return CreateGate(gatePrefabs[index]);
    }
}