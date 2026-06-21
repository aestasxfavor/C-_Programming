using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggleUI : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference inventoryAction;
    [SerializeField] private InputActionReference cancelAction;

    [Header("UI")]
    [SerializeField] private GameObject inventoryPanel;

    private bool isOpen;

    private void OnEnable()
    {
        EnableAction(inventoryAction);
        EnableAction(cancelAction);
    }

    private void Start()
    {
        CloseInventory();
        UnlockCursor();
    }

    private void Update()
    {
        EnableAction(inventoryAction);
        EnableAction(cancelAction);

        if (isOpen &&
            cancelAction != null &&
            cancelAction.action != null &&
            cancelAction.action.WasPressedThisFrame())
        {
            CloseInventory();
            return;
        }

        if (inventoryAction != null &&
            inventoryAction.action != null &&
            inventoryAction.action.WasPressedThisFrame())
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        if (isOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    private void OpenInventory()
    {
        if (inventoryPanel == null)
        {
            return;
        }

        isOpen = true;
        inventoryPanel.SetActive(true);

        UnlockCursor();
    }

    private void CloseInventory()
    {
        isOpen = false;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        UnlockCursor();
    }

    private void EnableAction(InputActionReference actionReference)
    {
        if (actionReference != null &&
            actionReference.action != null &&
            !actionReference.action.enabled)
        {
            actionReference.action.Enable();
        }
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}