using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private int poolSize = 100;

    private readonly List<GameObject> pooledUnits = new List<GameObject>();

    private void Awake()
    {
        CreatePool();
    }

    private void CreatePool()
    {
        if (unitPrefab == null)
        {
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            CreateUnit();
        }
    }

    private GameObject CreateUnit()
    {
        GameObject unit = Instantiate(unitPrefab, transform);

        unit.SetActive(false);
        pooledUnits.Add(unit);

        return unit;
    }

    public GameObject GetUnit()
    {
        GameObject unit = GetInactiveUnit();

        if (unit == null)
        {
            return null;
        }

        unit.SetActive(true);

        return unit;
    }

    public void ReturnUnit(GameObject unit)
    {
        if (unit == null)
        {
            return;
        }

        unit.SetActive(false);
        unit.transform.SetParent(transform);
        unit.transform.localPosition = Vector3.zero;
        unit.transform.localRotation = Quaternion.identity;
        unit.transform.localScale = Vector3.one;
    }

    public void ReturnAllUnits()
    {
        for (int i = 0; i < pooledUnits.Count; i++)
        {
            if (pooledUnits[i] == null)
            {
                continue;
            }

            ReturnUnit(pooledUnits[i]);
        }
    }

    private GameObject GetInactiveUnit()
    {
        // 미리 만들어둔 유닛 중 현재 사용하지 않는 오브젝트를 재사용
        for (int i = 0; i < pooledUnits.Count; i++)
        {
            if (!pooledUnits[i].activeSelf)
            {
                return pooledUnits[i];
            }
        }

        return null;
    }
}