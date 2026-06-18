using UnityEngine;
using InventoryFramework;

public class VendorNPC : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.LeftShift;

    private bool isPlayerInRange;
    private ItemPickupHandler playerPickupHandler;

    private void Update()
    {
        if (!isPlayerInRange)
        {
            return;
        }

        if (Input.GetKeyDown(interactKey))
        {
            OpenVendorUI();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ItemPickupHandler pickupHandler = other.GetComponent<ItemPickupHandler>();

        if (pickupHandler == null)
        {
            pickupHandler = other.GetComponentInParent<ItemPickupHandler>();
        }

        if (pickupHandler == null)
        {
            return;
        }

        isPlayerInRange = true;
        playerPickupHandler = pickupHandler;

        Debug.Log("NPC 판매 범위 진입");
    }

    private void OnTriggerExit(Collider other)
    {
        ItemPickupHandler pickupHandler = other.GetComponent<ItemPickupHandler>();

        if (pickupHandler == null)
        {
            pickupHandler = other.GetComponentInParent<ItemPickupHandler>();
        }

        if (pickupHandler == null)
        {
            return;
        }

        if (pickupHandler == playerPickupHandler)
        {
            isPlayerInRange = false;
            playerPickupHandler = null;

            Debug.Log("NPC 판매 범위 이탈");
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
    }
}