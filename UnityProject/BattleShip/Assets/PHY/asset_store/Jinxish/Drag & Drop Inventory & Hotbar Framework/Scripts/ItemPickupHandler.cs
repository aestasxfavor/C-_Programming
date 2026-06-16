using UnityEngine;

namespace InventoryFramework
{
    public class ItemPickupHandler : MonoBehaviour
    {
        public Hotbar hotbar;
        public Inventory inventory;

        public void PickupItem(Item item, int amount = 1)
        {
            if (item == null)
            {
                Debug.LogWarning("Pickup item is null.");
                return;
            }

            bool addedToHotbar = false;

            if (hotbar != null)
            {
                addedToHotbar = hotbar.AddItem(item, amount);
            }

            if (!addedToHotbar && inventory != null)
            {
                bool addedToInventory = inventory.AddItem(item, amount);

                if (!addedToInventory)
                {
                    Debug.Log("Hotbar and Inventory are full.");
                    return;
                }
            }

            HotbarUI hotbarUI = FindAnyObjectByType<HotbarUI>();

            if (hotbarUI != null)
            {
                hotbarUI.RefreshUI();
            }

            InventoryUI inventoryUI = FindAnyObjectByType<InventoryUI>();

            if (inventoryUI != null)
            {
                inventoryUI.RefreshUI();
            }

            Debug.Log($"Picked up {item.itemName} x {amount}");
        }
    }
}