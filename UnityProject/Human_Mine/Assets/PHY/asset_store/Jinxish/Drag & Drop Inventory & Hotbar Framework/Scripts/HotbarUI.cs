using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InventoryFramework
{
    public class HotbarUI : MonoBehaviour
    {
        public Hotbar hotbar;
        public Inventory inventory;
        public Transform slotParent;
        public GameObject slotPrefab;
        public ItemTooltip tooltip;
        public Transform toolsParent;

        private List<InventorySlotUI> slotUIs = new();
        private int selectedIndex = 0;

        public RectTransform dragLayer;
        public Canvas rootCanvas;

        private void Start()
        {
            ResolveReferences();
            BuildSlots();
            RefreshUI();
        }

        private void OnEnable()
        {
            ResolveReferences();
        }

        private void Update()
        {
            ResolveReferences();

            if (hotbar == null)
            {
                return;
            }

            for (int i = 0; i < hotbar.size; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    selectedIndex = i;
                    RefreshUI();
                }
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll > 0f)
            {
                selectedIndex = (selectedIndex + 1) % hotbar.size;
                RefreshUI();
            }
            else if (scroll < 0f)
            {
                selectedIndex = (selectedIndex - 1 + hotbar.size) % hotbar.size;
                RefreshUI();
            }
        }

        private void ResolveReferences()
        {
            if (InventorySystem.instance != null)
            {
                hotbar = InventorySystem.instance.Hotbar;
                inventory = InventorySystem.instance.Inventory;
            }

            if (rootCanvas == null)
            {
                rootCanvas = GetComponentInParent<Canvas>();
            }

            if (toolsParent == null)
            {
                Transform foundToolsParent = transform.Find("ToolsParent");

                if (foundToolsParent == null)
                {
                    foundToolsParent = FindChildByName(transform, "ToolsParent");
                }

                if (foundToolsParent != null)
                {
                    toolsParent = foundToolsParent;
                }
                else
                {
                    GameObject runtimeToolsParent = new GameObject("RuntimeToolsParent");
                    runtimeToolsParent.transform.SetParent(transform, false);
                    toolsParent = runtimeToolsParent.transform;
                }
            }
        }

        private Transform FindChildByName(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }

                Transform result = FindChildByName(child, childName);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private void BuildSlots()
        {
            if (slotParent == null)
            {
                Debug.LogError("HotbarUI: slotParent is missing.");
                return;
            }

            if (slotPrefab == null)
            {
                Debug.LogError("HotbarUI: slotPrefab is missing.");
                return;
            }

            if (hotbar == null)
            {
                Debug.LogError("HotbarUI: hotbar is missing.");
                return;
            }

            for (int i = slotParent.childCount - 1; i >= 0; i--)
            {
                Destroy(slotParent.GetChild(i).gameObject);
            }

            slotUIs.Clear();

            for (int i = 0; i < hotbar.size; i++)
            {
                GameObject go = Instantiate(slotPrefab, slotParent);
                InventorySlotUI ui = go.GetComponent<InventorySlotUI>();

                if (ui == null)
                {
                    Debug.LogError("HotbarUI: slotPrefab does not have InventorySlotUI.");
                    continue;
                }

                ui.tooltip = tooltip;
                ui.SetupHotbar(hotbar, inventory, i, this);
                slotUIs.Add(ui);
            }
        }

        public void RefreshUI()
        {
            ResolveReferences();

            if (hotbar == null)
            {
                Debug.LogError("HotbarUI: hotbar is missing.");
                return;
            }

            if (toolsParent == null)
            {
                Debug.LogError("HotbarUI: toolsParent is missing.");
                return;
            }

            if (slotUIs.Count != hotbar.size)
            {
                BuildSlots();
            }

            if (slotUIs.Count == 0)
            {
                return;
            }

            selectedIndex = Mathf.Clamp(selectedIndex, 0, hotbar.size - 1);

            for (int i = 0; i < hotbar.size; i++)
            {
                if (i >= slotUIs.Count || slotUIs[i] == null)
                {
                    continue;
                }

                slotUIs[i].SetSlot(hotbar.slots[i]);

                Transform backgroundTransform = slotUIs[i].transform.GetChild(0);
                Image bg = backgroundTransform.GetComponent<Image>();

                if (bg != null)
                {
                    bg.color = (i == selectedIndex) ? Color.yellow : Color.white;
                }
            }

            InventorySlot slot = slotUIs[selectedIndex].GetSlot();

            for (int x = toolsParent.childCount - 1; x >= 0; x--)
            {
                Destroy(toolsParent.GetChild(x).gameObject);
            }

            if (slot == null)
            {
                return;
            }

            if (slot.IsEmpty)
            {
                return;
            }

            if (slot.item == null)
            {
                return;
            }

            if (slot.item.model == null)
            {
                return;
            }

            Instantiate(slot.item.model, toolsParent);
        }
    }
}