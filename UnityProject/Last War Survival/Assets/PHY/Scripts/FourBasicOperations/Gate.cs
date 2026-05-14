using TMPro;
using UnityEngine;

public class Gate : MonoBehaviour
{
    [Header("Gate Settings")]
    [SerializeField] private OperationType operationType;
    [SerializeField] private int value = 1;

    [Header("References")]
    [SerializeField] private TextMeshPro operationText;

    private bool isUsed;

    private void Start()
    {
        UpdateGateText();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isUsed)
        {
            return;
        }

        PlayerUnitManager playerUnitManager = other.GetComponent<PlayerUnitManager>();

        if (playerUnitManager == null)
        {
            return;
        }

        isUsed = true;
        playerUnitManager.ApplyGate(operationType, value);

        gameObject.SetActive(false);
    }

    private void UpdateGateText()
    {
        if (operationText == null)
        {
            return;
        }

        operationText.text = GetOperationText();
    }

    private string GetOperationText()
    {
        switch (operationType)
        {
            case OperationType.Plus:
                return $"+{value}";

            case OperationType.Minus:
                return $"-{value}";

            case OperationType.Multiply:
                return $"¡¿{value}";

            case OperationType.Divide:
                return $"¡À{value}";

            default:
                return "";
        }
    }
}