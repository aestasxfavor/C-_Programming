using TMPro;
using UnityEngine;

public class Gate : MonoBehaviour
{
    [Header("Gate Settings")]
    [SerializeField] private GateType gateType;
    [SerializeField] private int gateValue = 1;

    [Header("References")]
    [SerializeField] private TextMeshPro gateText;

    private bool hasTriggered;

    private void OnEnable()
    {
        hasTriggered = false;
        UpdateGateText();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
        {
            return;
        }

        PlayerUnitManager playerUnitManager = other.GetComponentInParent<PlayerUnitManager>();

        if (playerUnitManager == null)
        {
            return;
        }

        hasTriggered = true;

        playerUnitManager.ApplyGate(gateType, gateValue);

        DisableGateGroup();
    }

    private void UpdateGateText()
    {
        if (gateText == null)
        {
            return;
        }

        gateText.text = GetGateText();
    }

    private string GetGateText()
    {
        switch (gateType)
        {
            case GateType.Plus:
                return $"+{gateValue}";

            case GateType.Minus:
                return $"-{gateValue}";

            case GateType.Multiply:
                return $"×{gateValue}";

            case GateType.Divide:
                return $"÷{gateValue}";

            default:
                return "";
        }
    }

    private void DisableGateGroup()
    {
        StageObjectMover gateGroupMover = GetComponentInParent<StageObjectMover>();

        if (gateGroupMover != null)
        {
            // 자식 게이트만 끄면 풀 재사용 때 한쪽 게이트만 남을 수 있음
            gateGroupMover.gameObject.SetActive(false);
            return;
        }

        transform.root.gameObject.SetActive(false);
    }
}