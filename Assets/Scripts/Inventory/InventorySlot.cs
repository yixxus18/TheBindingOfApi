using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
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

    private Canvas rootCanvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (GetComponent<CanvasGroup>() == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        else
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        rootCanvas = GetComponentInParent<Canvas>();
    }

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
                InventoryManager.instance.ConsumeItem(this);
            }
        }
    }

    private bool IsDragAllowed()
    {
        return TerminalActivator.terminalInstance != null && TerminalActivator.terminalInstance.activeInHierarchy;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemSO == null || !IsDragAllowed()) return;

        originalParent = transform.parent;
        transform.SetParent(rootCanvas.transform);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (itemSO == null || !IsDragAllowed()) return;
        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (itemSO == null || !IsDragAllowed()) return;

        if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponent<TerminalDropSlot>() == null)
        {
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = Vector2.zero;
        }

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
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

    public void UpdateUI()
    {
        bool hasItem = itemSO != null && quantity > 0;

        if (itemImage != null)
        {
            itemImage.enabled = hasItem && itemSO.icon != null;
            if (itemImage.enabled)
            {
                itemImage.sprite = itemSO.icon;
            }
        }

        if (quantityText != null)
        {
            quantityText.enabled = hasItem && quantity > 1;
            if (quantityText.enabled)
            {
                quantityText.text = quantity.ToString();
            }
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = hasItem ? GetColorByItemType(itemSO.itemType) : Color.white;
        }
    }

    private Color GetColorByItemType(ApiItemType type)
    {
        switch (type)
        {
            case ApiItemType.Method: return new Color(0.4f, 0.8f, 0.4f, 1f);
            case ApiItemType.Header: return new Color(0.4f, 0.6f, 1f, 1f);
            case ApiItemType.Token: return new Color(1f, 0.8f, 0.4f, 1f);
            case ApiItemType.Fragment: return new Color(0.8f, 0.4f, 1f, 1f);
            case ApiItemType.Consumable: return new Color(1f, 0.4f, 0.4f, 1f);
            default: return Color.white;
        }
    }
}