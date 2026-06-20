using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggleUI : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;

    private bool isOpen;

    private void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        UnlockCursor();
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        if (inventoryPanel == null)
        {
            return;
        }

        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);

        UnlockCursor();
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}