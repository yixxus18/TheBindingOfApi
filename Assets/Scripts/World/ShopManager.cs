using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    [Header("UI Prefabs")]
    public GameObject shopCanvasPrefab;
    public GameObject shopSlotPrefab;

    private GameObject shopCanvasInstance;
    private Transform slotContainer;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void OpenShop(List<ShopItem> items)
    {
        if (shopCanvasInstance != null) return;
        shopCanvasInstance = Instantiate(shopCanvasPrefab);
        slotContainer = shopCanvasInstance.transform.Find("ShopPanel/Scroll View/Viewport/Content");
        Button closeButton = shopCanvasInstance.transform.Find("ShopPanel/CloseButton").GetComponent<Button>();
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
        }
        UIManager.isGamePaused = true;
        PopulateShop(items);
    }

    public void CloseShop()
    {
        if (shopCanvasInstance == null) return;
        Destroy(shopCanvasInstance);
        shopCanvasInstance = null;
        UIManager.isGamePaused = false;
        if (TooltipManager.instance != null)
        {
            TooltipManager.instance.HideTooltip();
        }
    }

    private void PopulateShop(List<ShopItem> items)
    {
        if (slotContainer == null) return;
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (var shopItem in items)
        {
            GameObject slotGO = Instantiate(shopSlotPrefab, slotContainer);
            ShopSlot slot = slotGO.GetComponent<ShopSlot>();
            slot.Initialize(shopItem.item, shopItem.price, this);
        }
    }

    public void TryBuyItem(ItemSO item, int price)
    {
        if (StatsManager.instance.TrySpendGold(price))
        {
            bool success = InventoryManager.instance.AddItem(item, 1);
            if (success)
            {
                Debug.Log($"Comprado: {item.itemName}");
            }
            else
            {
                StatsManager.instance.AddGold(price);
                Debug.Log("Inventario lleno. No se pudo comprar.");
            }
        }
        else
        {
            Debug.Log("Oro insuficiente.");
        }
    }
}