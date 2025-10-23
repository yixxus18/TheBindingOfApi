using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Item Data")]
    public ItemSO itemSO;
    public int quantity;

    [Header("UI References")]
    public Image itemImage;
    public TMP_Text quantityText;
    public Image backgroundImage;

    [Header("Terminal Settings")]
    public static bool isTerminalActive = false;
    public static event Action<ItemSO> OnItemSentToTerminal;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemSO == null || quantity <= 0)
            return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (isTerminalActive && itemSO.itemType != ApiItemType.Consumable)
            {
                OnItemSentToTerminal?.Invoke(itemSO);
            }
            else if (itemSO.itemType == ApiItemType.Consumable)
            {
                InventoryManager.instance.ConsumeItem(this);
            }
        }
    }

    public void UpdateUI()
    {
        if (itemSO == null || quantity <= 0)
        {
            itemImage.enabled = false;
            if (quantityText != null) quantityText.gameObject.SetActive(false);
            if (backgroundImage != null) backgroundImage.color = new Color(1, 1, 1, 0.5f); // Un color por defecto para slots vacíos
            return;
        }

        if (itemImage != null)
        {
            if (itemSO.icon != null)
            {
                itemImage.sprite = itemSO.icon;
                itemImage.enabled = true;
                itemImage.color = Color.white;
            }
            else
            {
                itemImage.enabled = false;
            }
        }

        if (quantityText != null)
        {
            if (quantity > 1)
            {
                quantityText.text = quantity.ToString();
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                quantityText.text = "";
                quantityText.gameObject.SetActive(false);
            }
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = GetColorByItemType(itemSO.itemType);
        }
    }

    private Color GetColorByItemType(ApiItemType type)
    {
        switch (type)
        {
            case ApiItemType.Method:
                return new Color(0.4f, 0.8f, 0.4f, 1f);
            case ApiItemType.Header:
                return new Color(0.4f, 0.6f, 1f, 1f);
            case ApiItemType.Token:
                return new Color(1f, 0.8f, 0.4f, 1f);
            case ApiItemType.Fragment:
                return new Color(0.8f, 0.4f, 1f, 1f);
            case ApiItemType.Consumable:
                return new Color(1f, 0.4f, 0.4f, 1f);
            default:
                return Color.white;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemSO != null && TooltipManager.instance != null)
        {
            TooltipManager.instance.ShowTooltip(itemSO.itemName, itemSO.itemDescription);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.instance != null)
        {
            TooltipManager.instance.HideTooltip();
        }
    }
}