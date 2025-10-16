using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
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
        Debug.Log($"UpdateUI llamado para: {itemSO?.itemName ?? "NULL"} x{quantity}");

        if (itemSO == null || quantity <= 0)
        {
            Debug.LogWarning("ItemSO es null o quantity es 0");
            return;
        }
        if (itemImage != null)
        {
            if (itemSO.icon != null)
            {
                itemImage.sprite = itemSO.icon;
                itemImage.enabled = true;
                itemImage.gameObject.SetActive(true);
                itemImage.color = Color.white;

                Debug.Log($"✅ Imagen actualizada: {itemSO.icon.name}");
            }
            else
            {
                Debug.LogError($"❌ El ItemSO '{itemSO.itemName}' NO tiene icono asignado!");
            }
        }
        else
        {
            Debug.LogError("❌ itemImage es NULL en el prefab!");
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
        else
        {
            Debug.LogWarning("quantityText no asignado");
        }
        if (backgroundImage != null)
        {
            backgroundImage.color = GetColorByItemType(itemSO.itemType);
            Debug.Log($"Color de fondo cambiado a: {itemSO.itemType}");
        }
    }

    private Color GetColorByItemType(ApiItemType type)
    {
        switch (type)
        {
            case ApiItemType.Method:
                return new Color(0.4f, 0.8f, 0.4f, 1f); // Verde
            case ApiItemType.Header:
                return new Color(0.4f, 0.6f, 1f, 1f); // Azul
            case ApiItemType.Token:
                return new Color(1f, 0.8f, 0.4f, 1f); // Dorado
            case ApiItemType.Fragment:
                return new Color(0.8f, 0.4f, 1f, 1f); // Púrpura
            case ApiItemType.Consumable:
                return new Color(1f, 0.4f, 0.4f, 1f); // Rojo
            default:
                return Color.white;
        }
    }
}
