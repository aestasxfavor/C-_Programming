using UnityEngine;
using InventoryFramework;

public class MineableOre : MonoBehaviour
{
    [Header("Ore Item")]
    [SerializeField] private Item oreItem;
    [SerializeField] private int amount = 1;

    private bool isMined;

    public bool IsMined => isMined;

    public void Mine(ItemPickupHandler pickupHandler)
    {
        if (isMined)
        {
            return;
        }

        if (pickupHandler == null)
        {
            Debug.LogWarning("ItemPickupHandler is missing on player.");
            return;
        }

        if (oreItem == null)
        {
            Debug.LogWarning("Ore item is not assigned.");
            return;
        }

        isMined = true;

        pickupHandler.PickupItem(oreItem, amount);

        gameObject.SetActive(false);
    }
}