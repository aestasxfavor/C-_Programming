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
    [SerializeField] private int unitsPerRow = 5;
    [SerializeField] private float spacingX = 0.6f;
    [SerializeField] private float spacingZ = 0.8f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI unitCountText;

    private readonly List<GameObject> activeUnitCopies = new();

    public int CurrentUnitCount => currentUnitCount;

    private void Start()
    {
        PrepareUnitParent();

        ClampUnitCount();
        SyncUnitCopies();
        UpdateUnitCountUI();
    }

    private void LateUpdate()
    {
        UpdateFormation();
    }

    public void ApplyGate(GateType gateType, int value)
    {
        int amount = Mathf.Abs(value);

        Debug.Log($"Gate Applied: {gateType} {value}");
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

        Debug.Log($"After Gate Unit Count: {currentUnitCount}");

        SyncUnitCopies();
        UpdateUnitCountUI();

        Debug.Log($"Player Unit Count: {currentUnitCount}");

        if (currentUnitCount <= 0)
        {
            HandleUnitCountZero();
        }
    }

    public void ReduceUnitCount(int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        currentUnitCount -= damage;

        ClampUnitCount();
        SyncUnitCopies();
        UpdateUnitCountUI();

        Debug.Log($"Player Unit Count: {currentUnitCount}");

        if (currentUnitCount <= 0)
        {
            HandleUnitCountZero();
        }
    }

    private void HandleUnitCountZero()
    {
        Debug.Log("Game Over: All player units are dead.");
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

    private void SyncUnitCopies()
    {
        if (playerUnitPool == null)
        {
            Debug.LogError("PlayerUnitPool이 PlayerUnitManager에 연결되지 않았습니다.");
            return;
        }

        int copyCount = Mathf.Max(0, currentUnitCount - 1);

        Debug.Log($"Need Copy Count: {copyCount}, Current Copy Count: {activeUnitCopies.Count}");

        while (activeUnitCopies.Count < copyCount)
        {
            AddUnitCopy();
        }

        while (activeUnitCopies.Count > copyCount)
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

        activeUnitCopies.Add(unitCopy);

        Debug.Log($"Unit Copy Activated: {unitCopy.name}");
    }

    private void RemoveLastUnitCopy()
    {
        int lastIndex = activeUnitCopies.Count - 1;

        if (lastIndex < 0)
        {
            return;
        }

        GameObject unitCopy = activeUnitCopies[lastIndex];

        activeUnitCopies.RemoveAt(lastIndex);
        playerUnitPool.ReturnUnit(unitCopy);

        Debug.Log("Unit Copy Returned To Pool");
    }

    private void UpdateFormation()
    {
        if (unitParent == null)
        {
            return;
        }

        for (int i = 0; i < activeUnitCopies.Count; i++)
        {
            if (activeUnitCopies[i] == null)
            {
                continue;
            }

            int row = i / unitsPerRow;
            int column = i % unitsPerRow;

            int rowStartIndex = row * unitsPerRow;
            int countInThisRow = Mathf.Min(unitsPerRow, activeUnitCopies.Count - rowStartIndex);

            float centerOffset = (countInThisRow - 1) * 0.5f;

            float x = (column - centerOffset) * spacingX;
            float z = -(row + 1) * spacingZ;

            Vector3 localOffset = new Vector3(x, 0f, z);

            activeUnitCopies[i].transform.position = unitParent.TransformPoint(localOffset);
            activeUnitCopies[i].transform.rotation = unitParent.rotation;
            activeUnitCopies[i].transform.localScale = Vector3.one;
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