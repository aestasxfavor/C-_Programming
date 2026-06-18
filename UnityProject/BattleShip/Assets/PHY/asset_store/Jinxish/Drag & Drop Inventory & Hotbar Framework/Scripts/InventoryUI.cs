using System.Collections.Generic;
using UnityEngine;

namespace InventoryFramework
{
    public class InventoryUI : MonoBehaviour
    {
        public Inventory inventory;
        public Hotbar hotbar;
        public Transform slotParent;
        public GameObject slotPrefab;
        public ItemTooltip tooltip;

        public RectTransform dragLayer;
        public Canvas rootCanvas;

        private List<InventorySlotUI> slotUIs;
        private bool isInitialized;

        private void Awake()
        {
            TryAutoAssignReferences();
        }

        private void Start()
        {
            InitializeIfNeeded();
            RefreshUI();
        }

        private void OnEnable()
        {
            InitializeIfNeeded();
            RefreshUI();
        }

        private void TryAutoAssignReferences()
        {
            if (rootCanvas == null)
            {
                rootCanvas = GetComponentInParent<Canvas>();
            }

            if (InventorySystem.instance != null)
            {
                if (inventory == null)
                {
                    inventory = InventorySystem.instance.Inventory;
                }

                if (hotbar == null)
                {
                    hotbar = InventorySystem.instance.Hotbar;
                }
            }
        }

        private void InitializeIfNeeded()
        {
            if (isInitialized)
            {
                return;
            }

            TryAutoAssignReferences();

            if (inventory == null)
            {
                Debug.LogWarning("InventoryUI: Inventory 참조가 없습니다.");
                return;
            }

            if (hotbar == null)
            {
                Debug.LogWarning("InventoryUI: Hotbar 참조가 없습니다.");
                return;
            }

            if (slotParent == null)
            {
                Debug.LogWarning("InventoryUI: Slot Parent 참조가 없습니다.");
                return;
            }

            if (slotPrefab == null)
            {
                Debug.LogWarning("InventoryUI: Slot Prefab 참조가 없습니다.");
                return;
            }

            slotUIs = new List<InventorySlotUI>();

            foreach (Transform child in slotParent)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < inventory.size; i++)
            {
                GameObject slotGO = Instantiate(slotPrefab, slotParent);
                InventorySlotUI slotUI = slotGO.GetComponent<InventorySlotUI>();

                if (slotUI == null)
                {
                    Debug.LogWarning("InventoryUI: Slot Prefab에 InventorySlotUI가 없습니다.");
                    continue;
                }

                slotUI.tooltip = tooltip;
                slotUI.Setup(inventory, hotbar, i, this);
                slotUIs.Add(slotUI);
            }

            isInitialized = true;
        }

        public void RefreshUI()
        {
            InitializeIfNeeded();

            if (!isInitialized)
            {
                return;
            }

            if (inventory == null || inventory.slots == null)
            {
                return;
            }

            if (slotUIs == null)
            {
                return;
            }

            int refreshCount = Mathf.Min(slotUIs.Count, inventory.slots.Count);

            for (int i = 0; i < refreshCount; i++)
            {
                if (slotUIs[i] == null)
                {
                    continue;
                }

                slotUIs[i].SetSlot(inventory.slots[i]);
            }
        }
    }
}