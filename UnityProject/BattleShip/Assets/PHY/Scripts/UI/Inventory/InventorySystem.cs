using UnityEngine;
using InventoryFramework;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem instance;

    [Header("Inventory References")]
    [SerializeField] private Hotbar hotbar;
    [SerializeField] private Inventory inventory;

    public Hotbar Hotbar => hotbar;
    public Inventory Inventory => inventory;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (hotbar == null)
        {
            hotbar = GetComponentInChildren<Hotbar>(true);
        }

        if (inventory == null)
        {
            inventory = GetComponentInChildren<Inventory>(true);
        }

        if (hotbar == null)
        {
            Debug.LogError("InventorySystem: Hotbar reference is missing.");
        }

        if (inventory == null)
        {
            Debug.LogError("InventorySystem: Inventory reference is missing.");
        }
    }
}