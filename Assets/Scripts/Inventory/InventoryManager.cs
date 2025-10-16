using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("Dynamic Inventory Settings")]
    public Transform inventoryContainer;
    public GameObject slotPrefab;
    public int maxSlots = 24;

    private List<InventorySlot> activeSlots = new List<InventorySlot>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        Loot.OnItemLooted += HandleItemLooted;
    }

    void OnDestroy()
    {
        Loot.OnItemLooted -= HandleItemLooted;
    }

    private void HandleItemLooted(ItemSO item, int quantity)
    {
        AddItem(item, quantity);
    }

    public bool AddItem(ItemSO item, int quantity)
    {
        if (item == null || quantity <= 0)
            return false;

        foreach (var slot in activeSlots)
        {
            if (slot.itemSO == item && slot.quantity < item.stackSize)
            {
                int spaceLeft = item.stackSize - slot.quantity;
                int amountToAdd = Mathf.Min(quantity, spaceLeft);

                slot.quantity += amountToAdd;
                slot.UpdateUI();

                quantity -= amountToAdd;

                if (quantity <= 0)
                    return true;
            }
        }

        while (quantity > 0)
        {
            if (activeSlots.Count >= maxSlots)
            {
                Debug.Log("Inventario lleno!");
                return false;
            }

            InventorySlot newSlot = CreateNewSlot();

            int amountToAdd = Mathf.Min(quantity, item.stackSize);
            newSlot.itemSO = item;
            newSlot.quantity = amountToAdd;
            newSlot.UpdateUI();

            quantity -= amountToAdd;
        }

        return true;
    }

    private InventorySlot CreateNewSlot()
    {
        GameObject slotObj = Instantiate(slotPrefab, inventoryContainer);
        InventorySlot slot = slotObj.GetComponent<InventorySlot>();
        activeSlots.Add(slot);
        return slot;
    }

    public void RemoveSlot(InventorySlot slot)
    {
        if (activeSlots.Contains(slot))
        {
            activeSlots.Remove(slot);
            Destroy(slot.gameObject);
        }
    }

    public void ConsumeItem(InventorySlot slot)
    {
        if (slot.itemSO == null || slot.quantity <= 0)
            return;

        if (slot.itemSO.itemType == ApiItemType.Consumable)
        {
            ItemEffects.Apply(slot.itemSO);
        }

        slot.quantity--;

        if (slot.quantity <= 0)
        {
            RemoveSlot(slot);
        }
        else
        {
            slot.UpdateUI();
        }
    }

    public bool HasItem(ItemSO item)
    {
        return activeSlots.Exists(slot => slot.itemSO == item && slot.quantity > 0);
    }

    public int GetItemCount(ItemSO item)
    {
        int total = 0;
        foreach (var slot in activeSlots)
        {
            if (slot.itemSO == item)
                total += slot.quantity;
        }
        return total;
    }

    public void ClearInventory()
    {
        foreach (var slot in activeSlots.ToArray())
        {
            RemoveSlot(slot);
        }
        activeSlots.Clear();
    }
}
