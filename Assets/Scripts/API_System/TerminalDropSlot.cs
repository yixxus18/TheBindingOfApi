using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TerminalDropSlot : MonoBehaviour, IDropHandler
{
    public TerminalSlotType slotType;
    public TMP_Text displayText;

    private ItemSO currentItem;
    private InventorySlot sourceSlot;

    private void Start()
    {
        ClearSlot();
    }

    private void OnDestroy()
    {
        ReturnItemToInventory();
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlot inventorySlot = eventData.pointerDrag.GetComponent<InventorySlot>();

        if (inventorySlot != null && IsValidItemType(inventorySlot.itemSO))
        {
            ReturnItemToInventory();

            if (InventoryManager.instance.RemoveItem(inventorySlot.itemSO, 1))
            {
                currentItem = inventorySlot.itemSO;
                sourceSlot = inventorySlot;
                UpdateSlotUI();
            }
        }
    }

    public void ReturnItemToInventory()
    {
        if (currentItem != null)
        {
            InventoryManager.instance.AddItem(currentItem, 1);
            currentItem = null;
            sourceSlot = null;
        }
    }

    public void ClearSlot()
    {
        ReturnItemToInventory();
        UpdateSlotUI();
    }

    private bool IsValidItemType(ItemSO item)
    {
        if (item == null) return false;
        switch (slotType)
        {
            case TerminalSlotType.Method: return item.itemType == ApiItemType.Method;
            case TerminalSlotType.Url: return item.itemType == ApiItemType.Fragment;
            case TerminalSlotType.Header: return item.itemType == ApiItemType.Header || item.itemType == ApiItemType.Token;
            case TerminalSlotType.Body: return item.itemType == ApiItemType.Body;
            default: return false;
        }
    }

    private void UpdateSlotUI()
    {
        displayText.text = currentItem != null ? currentItem.apiValue : GetDefaultText();
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

    public ItemSO GetCurrentItem() => currentItem;
}

public enum TerminalSlotType { Method, Url, Header, Body }