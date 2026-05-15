using TMPro;
using UnityEngine;

public class Gate : MonoBehaviour
{
    [Header("Gate Settings")]
    [SerializeField] private GateType gateType;
    [SerializeField] private int value = 1;

    [Header("References")]
    [SerializeField] private TextMeshPro operationText;

    private bool isUsed;

    private void OnEnable()
    {
        isUsed = false;
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

        playerUnitManager.ApplyGate(gateType, value);

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
        switch (gateType)
        {
            case GateType.Plus:
                return $"+{value}";

            case GateType.Minus:
                return $"-{value}";

            case GateType.Multiply:
                return $"¡¿{value}";

            case GateType.Divide:
                return $"¡À{value}";

            default:
                return "";
        }
    }
}