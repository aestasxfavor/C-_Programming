using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace InventoryFramework
{
    public enum SlotOwner
    {
        Inventory,
        Hotbar,
    }

    public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Image icon;
        public TextMeshProUGUI countText;

        private Inventory inventory;
        private InventoryUI inventoryUI;

        private Hotbar hotbar;
        private HotbarUI hotbarUI;

        public int index;
        public SlotOwner owner;

        private GameObject dragIcon;
        private RectTransform dragRT;

        public ItemTooltip tooltip;

        private void Update()
        {
            if (tooltip != null && tooltip.gameObject.activeSelf)
            {
                tooltip.UpdatePosition(Input.mousePosition);
            }
        }

        public void Setup(Inventory inv, Hotbar hb, int idx, InventoryUI ui)
        {
            inventory = inv;
            hotbar = hb;
            index = idx;
            inventoryUI = ui;
            owner = SlotOwner.Inventory;
            hotbarUI = null;
        }

        public void SetupHotbar(Hotbar hb, Inventory inv, int idx, HotbarUI ui)
        {
            hotbar = hb;
            inventory = inv;
            hotbarUI = ui;
            index = idx;
            owner = SlotOwner.Hotbar;
            inventoryUI = null;
        }

        public InventorySlot GetSlot()
        {
            if (owner == SlotOwner.Inventory)
            {
                if (inventory == null || inventory.slots == null || index < 0 || index >= inventory.slots.Count)
                {
                    return null;
                }

                return inventory.slots[index];
            }

            if (hotbar == null || hotbar.slots == null || index < 0 || index >= hotbar.slots.Count)
            {
                return null;
            }

            return hotbar.slots[index];
        }

        public void SetSlot(InventorySlot slot)
        {
            if (slot == null || slot.IsEmpty)
            {
                icon.enabled = false;
                icon.sprite = null;
                countText.text = "";
                return;
            }

            icon.enabled = true;
            icon.sprite = slot.item.icon;
            countText.text = slot.count > 1 ? slot.count.ToString() : "";
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            InventorySlot slot = GetSlot();

            if (slot == null || slot.IsEmpty)
            {
                return;
            }

            int dragAmount = slot.count;

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                dragAmount = Mathf.CeilToInt(slot.count / 2f);
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                dragAmount = 1;
            }

            DragContext.draggedItem = slot.item;
            DragContext.draggedCount = dragAmount;
            DragContext.fromSlotIndex = index;
            DragContext.fromOwner = owner;

            slot.count -= dragAmount;

            if (slot.count <= 0)
            {
                ClearSlot(slot);
            }

            CreateDragIcon();

            RefreshAllUIs();
            UpdateDragPosition(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragIcon == null)
            {
                return;
            }

            UpdateDragPosition(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragIcon != null)
            {
                Destroy(dragIcon);
                dragIcon = null;
                dragRT = null;
            }

            if (DragContext.draggedItem != null && DragContext.draggedCount > 0)
            {
                ReturnToOriginalSlot();

                DragContext.draggedItem = null;
                DragContext.draggedCount = 0;
            }

            RefreshAllUIs();
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log($"OnDrop 호출됨 / owner: {owner} / index: {index}");

            if (DragContext.draggedItem == null || DragContext.draggedCount <= 0)
            {
                return;
            }

            InventorySlot targetSlot = GetSlot();

            if (targetSlot == null)
            {
                ReturnToOriginalSlot();
                ClearDragContext();
                RefreshAllUIs();
                return;
            }

            InventorySlot originalSlot = GetOriginalSlot();

            if (targetSlot == originalSlot)
            {
                ReturnToOriginalSlot();
                ClearDragContext();
                RefreshAllUIs();
                return;
            }

            if (targetSlot.IsEmpty)
            {
                targetSlot.item = DragContext.draggedItem;
                targetSlot.count = DragContext.draggedCount;

                ClearDragContext();
                RefreshAllUIs();
                return;
            }

            if (targetSlot.item == DragContext.draggedItem)
            {
                int space = targetSlot.item.maxStack - targetSlot.count;
                int addAmount = Mathf.Min(space, DragContext.draggedCount);

                targetSlot.count += addAmount;
                DragContext.draggedCount -= addAmount;

                if (DragContext.draggedCount > 0)
                {
                    ReturnToOriginalSlot();
                }

                ClearDragContext();
                RefreshAllUIs();
                return;
            }

            if (originalSlot == null)
            {
                ReturnToOriginalSlot();
                ClearDragContext();
                RefreshAllUIs();
                return;
            }

            Item tempItem = targetSlot.item;
            int tempCount = targetSlot.count;

            targetSlot.item = DragContext.draggedItem;
            targetSlot.count = DragContext.draggedCount;

            originalSlot.item = tempItem;
            originalSlot.count = tempCount;

            ClearDragContext();
            RefreshAllUIs();
        }

        private void CreateDragIcon()
        {
            RectTransform dragLayer = GetDragLayer();

            if (dragLayer == null)
            {
                Debug.LogError("DragLayer가 연결되어 있지 않습니다.");
                return;
            }

            dragIcon = new GameObject("DragIcon");
            dragIcon.transform.SetParent(dragLayer, false);
            dragIcon.transform.SetAsLastSibling();

            dragRT = dragIcon.AddComponent<RectTransform>();

            Image dragImage = dragIcon.AddComponent<Image>();
            dragImage.sprite = DragContext.draggedItem.icon;
            dragImage.color = Color.white;
            dragImage.raycastTarget = false;

            dragRT.sizeDelta = icon.rectTransform.sizeDelta;
        }

        private RectTransform GetDragLayer()
        {
            if (owner == SlotOwner.Inventory && inventoryUI != null)
            {
                return inventoryUI.dragLayer;
            }

            if (owner == SlotOwner.Hotbar && hotbarUI != null)
            {
                return hotbarUI.dragLayer;
            }

            return null;
        }

        private Canvas GetRootCanvas()
        {
            if (owner == SlotOwner.Inventory && inventoryUI != null)
            {
                return inventoryUI.rootCanvas;
            }

            if (owner == SlotOwner.Hotbar && hotbarUI != null)
            {
                return hotbarUI.rootCanvas;
            }

            return null;
        }

        private void UpdateDragPosition(PointerEventData eventData)
        {
            if (dragRT == null)
            {
                return;
            }

            Canvas canvas = GetRootCanvas();
            RectTransform dragLayer = GetDragLayer();

            if (canvas == null || dragLayer == null)
            {
                return;
            }

            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragLayer,
                eventData.position,
                cam,
                out Vector2 localPoint
            );

            dragRT.anchoredPosition = localPoint;
        }

        private InventorySlot GetOriginalSlot()
        {
            if (DragContext.fromOwner == SlotOwner.Inventory)
            {
                if (inventory == null || inventory.slots == null || DragContext.fromSlotIndex < 0 || DragContext.fromSlotIndex >= inventory.slots.Count)
                {
                    return null;
                }

                return inventory.slots[DragContext.fromSlotIndex];
            }

            if (DragContext.fromOwner == SlotOwner.Hotbar)
            {
                if (hotbar == null || hotbar.slots == null || DragContext.fromSlotIndex < 0 || DragContext.fromSlotIndex >= hotbar.slots.Count)
                {
                    return null;
                }

                return hotbar.slots[DragContext.fromSlotIndex];
            }

            return null;
        }

        private void ReturnToOriginalSlot()
        {
            InventorySlot originalSlot = GetOriginalSlot();

            if (originalSlot == null)
            {
                return;
            }

            if (originalSlot.IsEmpty)
            {
                originalSlot.item = DragContext.draggedItem;
                originalSlot.count = DragContext.draggedCount;
                return;
            }

            if (originalSlot.item == DragContext.draggedItem)
            {
                originalSlot.count += DragContext.draggedCount;
            }
        }

        private void ClearDragContext()
        {
            DragContext.draggedItem = null;
            DragContext.draggedCount = 0;
        }

        private void ClearSlot(InventorySlot slot)
        {
            slot.item = null;
            slot.count = 0;
        }

        private void RefreshAllUIs()
        {
            if (inventoryUI != null)
            {
                inventoryUI.RefreshUI();
            }

            if (hotbarUI != null)
            {
                hotbarUI.RefreshUI();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (tooltip == null)
            {
                return;
            }

            InventorySlot slot = GetSlot();

            if (slot != null && !slot.IsEmpty)
            {
                tooltip.Show(slot.item, eventData.position);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (tooltip == null)
            {
                return;
            }

            tooltip.Hide();
        }
    }
}