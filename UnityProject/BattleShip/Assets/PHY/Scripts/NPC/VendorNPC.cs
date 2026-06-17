using UnityEngine;
using InventoryFramework;

public class VendorNPC : MonoBehaviour
{
    [Header("Sell Settings")]
    [SerializeField] private int defaultOrePrice = 10;

    [Header("Interaction")]
    [SerializeField] private KeyCode sellKey = KeyCode.LeftShift;

    private bool isPlayerInRange;
    private ItemPickupHandler playerPickupHandler;

    private void Update()
    {
        if (!isPlayerInRange)
        {
            return;
        }

        if (Input.GetKeyDown(sellKey))
        {
            SellOres();
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

    private void SellOres()
    {
        Debug.Log("판매 시도");

        if (CoinManager.instance == null)
        {
            Debug.LogWarning("CoinManager가 없습니다.");
            return;
        }

        CoinManager.instance.AddCoin(defaultOrePrice);

        Debug.Log($"테스트 판매 완료: +{defaultOrePrice} Coin");
    }
}