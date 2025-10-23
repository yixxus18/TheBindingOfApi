using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    public TMP_Text itemNameText;
    public TMP_Text priceText;
    public Image itemImage;

    private ItemSO itemSO;
    private int price;
    private ShopManager shopManager;

    public void Initialize(ItemSO newItem, int newPrice, ShopManager manager)
    {
        itemSO = newItem;
        price = newPrice;
        shopManager = manager;

        if (itemSO != null)
        {
            itemNameText.text = itemSO.itemName;
            if (itemSO.icon != null)
            {
                itemImage.sprite = itemSO.icon;
                itemImage.enabled = true;
            }
            else
            {
                itemImage.enabled = false;
            }
        }
        priceText.text = price.ToString() + " $";
    }

    public void OnBuyButtonClicked()
    {
        if (itemSO != null)
        {
            shopManager.TryBuyItem(itemSO, price);
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