using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class TerminalDropSlot : MonoBehaviour, IDropHandler
{
    public TerminalSlotType slotType;
    public TMP_Text displayText;

    private List<ItemSO> currentItems = new List<ItemSO>();

    private void Start()
    {
        ClearSlot();
    }

    private void OnDestroy()
    {
        ReturnAllItemsToInventory();
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlot inventorySlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (inventorySlot == null || inventorySlot.itemSO == null) return;

        ItemSO itemToDrop = inventorySlot.itemSO;

        if (IsValidItemType(itemToDrop))
        {
            if (InventoryManager.instance.RemoveItem(itemToDrop, 1))
            {
                currentItems.Add(itemToDrop);
                UpdateSlotUI();
            }
        }
    }

    public void ReturnAllItemsToInventory()
    {
        if (currentItems == null) return;
        foreach (var item in currentItems)
        {
            if (item != null) InventoryManager.instance.AddItem(item, 1);
        }
        currentItems.Clear();
    }

    public void ClearSlot()
    {
        ReturnAllItemsToInventory();
        UpdateSlotUI();
    }

    public void ConsumeItem()
    {
        currentItems.Clear();
    }

    private bool IsValidItemType(ItemSO item)
    {
        if (item == null) return false;
        switch (slotType)
        {
            case TerminalSlotType.Method: return item.itemType == ApiItemType.Method && currentItems.Count == 0;
            case TerminalSlotType.Url: return item.itemType == ApiItemType.Fragment;
            case TerminalSlotType.Header: return item.itemType == ApiItemType.Header || item.itemType == ApiItemType.Token;
            case TerminalSlotType.Body: return item.itemType == ApiItemType.Body;
            default: return false;
        }
    }

    private void UpdateSlotUI()
    {
        if (currentItems != null && currentItems.Count > 0)
        {
            displayText.text = string.Join("", currentItems.Where(item => item != null).Select(item => item.apiValue ?? ""));
        }
        else
        {
            displayText.text = GetDefaultText();
        }
    }

    private string GetDefaultText()
    {
        switch (slotType)
        {
            case TerminalSlotType.Method: return "METHOD";
            case TerminalSlotType.Url: return "URL";
            case TerminalSlotType.Header: return "HEADER";
            case TerminalSlotType.Body: return "BODY";
            default: return "";
        }
    }

    public List<ItemSO> GetCurrentItems() => currentItems;
    public ItemSO GetCurrentItem() => currentItems.FirstOrDefault();
}

public enum TerminalSlotType { Method, Url, Header, Body }