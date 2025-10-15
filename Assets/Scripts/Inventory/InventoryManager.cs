using UnityEngine;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    public InventorySlot[] itemSlots;

    private void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public bool AddItem(ItemSO item, int quantity)
    {
        foreach (var slot in itemSlots)
        {
            if (slot.itemSO == item && slot.quantity < item.stackSize)
            {
                int spaceLeft = item.stackSize - slot.quantity;
                int amountToAdd = Mathf.Min(quantity, spaceLeft);
                slot.quantity += amountToAdd;
                quantity -= amountToAdd;
                slot.UpdateUI();
                if (quantity <= 0) return true;
            }
        }
        foreach (var slot in itemSlots)
        {
            if (slot.itemSO == null)
            {
                int amountToAdd = Mathf.Min(quantity, item.stackSize);
                slot.itemSO = item;
                slot.quantity = amountToAdd;
                quantity -= amountToAdd;
                slot.UpdateUI();
                if (quantity <= 0) return true;
            }
        }
        Debug.Log("Inventario lleno.");
        return false;
    }

    public void ProcessItemUse(InventorySlot slot)
    {
        if (slot.itemSO == null) return;
        UseItem.Apply(slot.itemSO);
        slot.quantity--;
        if (slot.quantity <= 0)
        {
            slot.itemSO = null;
        }
        slot.UpdateUI();
    }
}