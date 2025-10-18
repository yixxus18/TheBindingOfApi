using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
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

        itemNameText.text = itemSO.itemName;
        itemImage.sprite = itemSO.icon;
        priceText.text = price.ToString() + " $";
    }

    public void OnBuyButtonClicked()
    {
        shopManager.TryBuyItem(itemSO, price);
    }
}