using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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

    public List<InventorySlot> GetActiveSlots()
    {
        return activeSlots;
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
                InventorySlot emptySlot = activeSlots.FirstOrDefault(s => s.itemSO == null);
                if (emptySlot == null)
                {
                    Debug.Log("Inventario lleno!");
                    return false;
                }
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

    public bool RemoveItem(ItemSO item, int quantity)
    {
        if (item == null || quantity <= 0) return false;

        InventorySlot slotToRemoveFrom = activeSlots.Find(slot => slot.itemSO == item);

        if (slotToRemoveFrom != null && slotToRemoveFrom.quantity >= quantity)
        {
            slotToRemoveFrom.quantity -= quantity;

            if (slotToRemoveFrom.quantity <= 0)
            {
                slotToRemoveFrom.itemSO = null;
                RemoveSlot(slotToRemoveFrom);
            }
            else
            {
                slotToRemoveFrom.UpdateUI();
            }
            return true;
        }
        return false;
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

    public void ClearInventory()
    {
        foreach (var slot in activeSlots.ToArray())
        {
            RemoveSlot(slot);
        }
        activeSlots.Clear();
    }
}