using System.Collections.Generic;
using UnityEngine;

namespace InventoryFramework
{
    public class Hotbar : MonoBehaviour
    {
        public int size = 9;
        public List<InventorySlot> slots;

        void Awake()
        {
            slots = new List<InventorySlot>(new InventorySlot[size]);

            for (int i = 0; i < size; i++)
            {
                slots[i] = new InventorySlot();
            }
        }

        public InventorySlot GetSlot(int index)
        {
            if (index < 0 || index >= size)
            {
                return null;
            }

            return slots[index];
        }

        public bool AddItem(Item newItem, int amount = 1)
        {
            if (newItem == null || amount <= 0)
            {
                return false;
            }

            foreach (var slot in slots)
            {
                if (slot == null)
                {
                    continue;
                }

                if (!slot.IsEmpty && slot.item == newItem && slot.count < newItem.maxStack)
                {
                    int space = newItem.maxStack - slot.count;
                    int add = Mathf.Min(space, amount);

                    slot.count += add;
                    amount -= add;

                    if (amount <= 0)
                    {
                        return true;
                    }
                }
            }

            foreach (var slot in slots)
            {
                if (slot == null)
                {
                    continue;
                }

                if (slot.IsEmpty)
                {
                    slot.item = newItem;
                    slot.count = amount;

                    return true;
                }
            }

            return false;
        }

        public int GetItemCount(Item targetItem)
        {
            if (targetItem == null)
            {
                return 0;
            }

            int totalCount = 0;

            foreach (var slot in slots)
            {
                if (slot == null || slot.IsEmpty)
                {
                    continue;
                }

                if (slot.item == targetItem)
                {
                    totalCount += slot.count;
                }
            }

            return totalCount;
        }

        public bool RemoveItem(Item targetItem, int amount)
        {
            if (targetItem == null || amount <= 0)
            {
                return false;
            }

            foreach (var slot in slots)
            {
                if (slot == null || slot.IsEmpty)
                {
                    continue;
                }

                if (slot.item != targetItem)
                {
                    continue;
                }

                if (slot.count > amount)
                {
                    slot.count -= amount;
                    return true;
                }

                amount -= slot.count;
                ClearSlot(slot);

                if (amount <= 0)
                {
                    return true;
                }
            }

            return amount <= 0;
        }

        private void ClearSlot(InventorySlot slot)
        {
            slot.item = null;
            slot.count = 0;
        }
    }
}