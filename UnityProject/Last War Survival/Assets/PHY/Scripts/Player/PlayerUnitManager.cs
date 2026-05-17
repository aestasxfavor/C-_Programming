using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUnitManager : MonoBehaviour
{
    [Header("Unit Settings")]
    [SerializeField] private int currentUnitCount = 1;
    [SerializeField] private int minUnitCount = 0;
    [SerializeField] private int maxUnitCount = 20;

    [Header("Unit Pool")]
    [SerializeField] private PlayerUnitPool playerUnitPool;

    [Header("Unit Formation")]
    [SerializeField] private Transform unitParent;
    [SerializeField] private int maxUnitsPerRow = 5;
    [SerializeField] private float xSpacing = 0.6f;
    [SerializeField] private float zSpacing = 0.8f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI unitCountText;

    private readonly List<GameObject> unitCopies = new List<GameObject>();

    public int CurrentUnitCount => currentUnitCount;

    private void Start()
    {
        PrepareUnitParent();

        ClampUnitCount();
        UpdateUnitCopies();
        UpdateUnitCountUI();
    }

    private void LateUpdate()
    {
        UpdateFormation();
    }

    public void ApplyGate(GateType gateType, int value)
    {
        int amount = Mathf.Abs(value);

        switch (gateType)
        {
            case GateType.Plus:
                currentUnitCount += amount;
                break;

            case GateType.Minus:
                currentUnitCount -= amount;
                break;

            case GateType.Multiply:
                currentUnitCount *= amount;
                break;

            case GateType.Divide:
                if (amount != 0)
                {
                    currentUnitCount /= amount;
                }
                break;
        }

        ClampUnitCount();
        UpdateUnitCopies();
        UpdateUnitCountUI();
    }

    public void ReduceUnitCount(int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        currentUnitCount -= damage;

        ClampUnitCount();
        UpdateUnitCopies();
        UpdateUnitCountUI();
    }

    private void PrepareUnitParent()
    {
        if (unitParent != null)
        {
            return;
        }

        GameObject unitParentObject = new GameObject("UnitParent");
        unitParent = unitParentObject.transform;

        unitParent.SetParent(transform);
        unitParent.localPosition = Vector3.zero;
        unitParent.localRotation = Quaternion.identity;
        unitParent.localScale = Vector3.one;
    }

    private void UpdateUnitCopies()
    {
        if (playerUnitPool == null)
        {
            return;
        }

        // 플레이어 본체 1명은 제외하고 추가 유닛만 풀에서 관리
        int targetCopyCount = Mathf.Max(0, currentUnitCount - 1);

        while (unitCopies.Count < targetCopyCount)
        {
            AddUnitCopy();
        }

        while (unitCopies.Count > targetCopyCount)
        {
            RemoveLastUnitCopy();
        }

        UpdateFormation();
    }

    private void AddUnitCopy()
    {
        GameObject unitCopy = playerUnitPool.GetUnit();

        if (unitCopy == null)
        {
            return;
        }

        unitCopies.Add(unitCopy);
    }

    private void RemoveLastUnitCopy()
    {
        int lastIndex = unitCopies.Count - 1;

        if (lastIndex < 0)
        {
            return;
        }

        GameObject unitCopy = unitCopies[lastIndex];

        unitCopies.RemoveAt(lastIndex);
        playerUnitPool.ReturnUnit(unitCopy);
    }

    private void UpdateFormation()
    {
        if (unitParent == null)
        {
            return;
        }

        for (int i = 0; i < unitCopies.Count; i++)
        {
            if (unitCopies[i] == null)
            {
                continue;
            }

            int row = i / maxUnitsPerRow;
            int column = i % maxUnitsPerRow;

            int rowStartIndex = row * maxUnitsPerRow;
            int rowUnitCount = Mathf.Min(maxUnitsPerRow, unitCopies.Count - rowStartIndex);

            // 마지막 줄도 가운데 정렬되도록 행마다 중심값을 다시 계산
            float centerOffset = (rowUnitCount - 1) * 0.5f;

            float x = (column - centerOffset) * xSpacing;
            float z = -(row + 1) * zSpacing;

            Vector3 localOffset = new Vector3(x, 0f, z);

            unitCopies[i].transform.position = unitParent.TransformPoint(localOffset);
            unitCopies[i].transform.rotation = unitParent.rotation;
            unitCopies[i].transform.localScale = Vector3.one;
        }
    }

    private void ClampUnitCount()
    {
        currentUnitCount = Mathf.Clamp(currentUnitCount, minUnitCount, maxUnitCount);
    }

    private void UpdateUnitCountUI()
    {
        if (unitCountText == null)
        {
            return;
        }

        unitCountText.text = $"Unit : {currentUnitCount}";
    }
}