using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public ItemSO itemSO;
    public int quantity;

    public Image itemImage;
    public TMP_Text quantityText;

    public static bool isTerminalActive = false;
    public static event Action<ItemSO> OnItemSentToTerminal;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemSO == null || quantity <= 0) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (isTerminalActive && itemSO.itemType != ApiItemType.Consumable)
            {
                OnItemSentToTerminal?.Invoke(itemSO);
            }
            else if (itemSO.itemType == ApiItemType.Consumable)
            {
                InventoryManager.instance.ProcessItemUse(this);
            }
        }
    }

    public void UpdateUI()
    {
        if (quantity <= 0) itemSO = null;
        bool hasItem = itemSO != null;
        itemImage.gameObject.SetActive(hasItem);
        quantityText.gameObject.SetActive(hasItem);

        if (hasItem)
        {
            itemImage.sprite = itemSO.icon;
            quantityText.text = quantity.ToString();
        }
    }
}