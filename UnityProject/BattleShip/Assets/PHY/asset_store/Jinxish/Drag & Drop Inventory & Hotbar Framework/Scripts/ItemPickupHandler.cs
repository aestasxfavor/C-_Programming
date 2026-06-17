using UnityEngine;

namespace InventoryFramework
{
    public class ItemPickupHandler : MonoBehaviour
    {
        public Hotbar hotbar;
        public Inventory inventory;

        private void Awake()
        {
            ResolveInventoryReferences();
        }

        private void OnEnable()
        {
            ResolveInventoryReferences();
        }

        private void ResolveInventoryReferences()
        {
            InventorySystem system = InventorySystem.instance;

            if (system == null)
            {
                system = FindFirstObjectByType<InventorySystem>();
            }

            if (system != null)
            {
                hotbar = system.Hotbar;
                inventory = system.Inventory;
                return;
            }

            if (hotbar == null || !hotbar.gameObject.activeInHierarchy)
            {
                hotbar = FindFirstObjectByType<Hotbar>();
            }

            if (inventory == null || !inventory.gameObject.activeInHierarchy)
            {
                inventory = FindFirstObjectByType<Inventory>();
            }
        }

        public void PickupItem(Item item, int amount = 1)
        {
            ResolveInventoryReferences();

            if (item == null)
            {
                Debug.LogWarning("Pickup item is null.");
                return;
            }

            if (hotbar == null && inventory == null)
            {
                Debug.LogError("ItemPickupHandler: Hotbar and Inventory references are missing.");
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