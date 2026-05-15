using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private int poolSize = 100;

    private readonly List<GameObject> pool = new();

    private void Awake()
    {
        CreatePool();
    }

    private void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateUnit();
        }
    }

    private GameObject CreateUnit()
    {
        GameObject unit = Instantiate(unitPrefab, transform);
        unit.SetActive(false);
        pool.Add(unit);

        return unit;
    }

    public GameObject GetUnit()
    {
        GameObject unit = GetInactiveUnit();

        if (unit == null)
        {
            Debug.LogWarning("PlayerUnitPool에 남은 비활성 유닛이 없습니다. Pool Size를 늘려야 합니다.");
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
        foreach (GameObject unit in pool)
        {
            if (unit == null)
            {
                continue;
            }

            ReturnUnit(unit);
        }
    }

    private GameObject GetInactiveUnit()
    {
        foreach (GameObject unit in pool)
        {
            if (!unit.activeSelf)
            {
                return unit;
            }
        }

        return null;
    }
}