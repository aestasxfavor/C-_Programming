using TMPro;
using UnityEngine;

public class PlayerUnitManager : MonoBehaviour
{
    [Header("Unit Settings")]
    [SerializeField] private int currentUnitCount = 5;
    [SerializeField] private int minUnitCount = 0;
    [SerializeField] private int maxUnitCount = 100;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI unitCountText;

    public int CurrentUnitCount => currentUnitCount;

    private void Start()
    {
        ClampUnitCount();
        UpdateUnitCountUI();
    }

    public void ApplyGate(OperationType operationType, int value)
    {
        switch (operationType)
        {
            case OperationType.Plus:
                currentUnitCount += value;
                break;

            case OperationType.Minus:
                currentUnitCount -= value;
                break;

            case OperationType.Multiply:
                currentUnitCount *= value;
                break;

            case OperationType.Divide:
                if (value != 0)
                {
                    currentUnitCount /= value;
                }
                break;
        }

        ClampUnitCount();
        UpdateUnitCountUI();

        Debug.Log($"player Unit Count: {currentUnitCount}");
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

    private void AddUnit()
    {

    }

    private void RemoveUnit() 
    {

    }
}