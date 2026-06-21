using UnityEngine;
using UnityEngine.InputSystem;
using InventoryFramework;

public class VendorNPC : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float detectRadius = 2.5f;
    [SerializeField] private LayerMask playerLayer;

    private bool isPlayerInRange;
    private ItemPickupHandler playerPickupHandler;

    private void Update()
    {
        CheckPlayerInRange();
        CheckInteractionInput();
    }

    private void CheckPlayerInRange()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            detectRadius,
            playerLayer,
            QueryTriggerInteraction.Collide
        );

        ItemPickupHandler foundPickupHandler = null;

        for (int i = 0; i < hits.Length; i++)
        {
            foundPickupHandler = hits[i].GetComponent<ItemPickupHandler>();

            if (foundPickupHandler == null)
            {
                foundPickupHandler = hits[i].GetComponentInParent<ItemPickupHandler>();
            }

            if (foundPickupHandler != null)
            {
                break;
            }
        }

        bool foundPlayer = foundPickupHandler != null;

        if (isPlayerInRange == foundPlayer)
        {
            if (foundPlayer)
            {
                playerPickupHandler = foundPickupHandler;
            }

            return;
        }

        isPlayerInRange = foundPlayer;
        playerPickupHandler = foundPickupHandler;

        if (isPlayerInRange)
        {
            Debug.Log("NPC 판매 범위 진입");
        }
        else
        {
            Debug.Log("NPC 판매 범위 이탈");
        }
    }

    private void CheckInteractionInput()
    {
        if (!isPlayerInRange)
        {
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame ||
            Keyboard.current.rightShiftKey.wasPressedThisFrame)
        {
            OpenVendorUI();
        }
    }

    private void OpenVendorUI()
    {
        if (VendorUIController.instance == null)
        {
            Debug.LogWarning("VendorUIController가 없습니다.");
            return;
        }

        VendorUIController.instance.Open();

        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.HidePrompt();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}