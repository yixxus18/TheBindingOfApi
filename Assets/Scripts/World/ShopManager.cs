using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    [Header("UI Prefabs")]
    [Tooltip("Arrastra aquí el PREFAB del Canvas de la tienda (ShopCanvas.prefab)")]
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
        else
        {
            Debug.LogError("No se encontró 'CloseButton' en el prefab de la tienda. Revisa el nombre y la jerarquía.");
        }

        UIManager.isGamePaused = true;
        PopulateShop(items);
    }

    public void CloseShop()
    {
        if (shopCanvasInstance == null) return;

        Destroy(shopCanvasInstance);
        shopCanvasInstance = null; // Liberar la referencia
        UIManager.isGamePaused = false;

        if (TooltipManager.instance != null)
        {
            TooltipManager.instance.HideTooltip();
        }
    }

    private void PopulateShop(List<ShopItem> items)
    {
        if (slotContainer == null)
        {
            Debug.LogError("No se encontró el 'slotContainer'. Revisa la ruta en ShopManager.cs: 'ShopPanel/Scroll View/Viewport/Content'");
            return;
        }

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
        bool success = InventoryManager.instance.AddItem(item, 1);
        if (success)
        {
            Debug.Log($"Comprado: {item.itemName}");
        }
        else
        {
            Debug.Log("Inventario lleno. No se pudo comprar.");
        }
    }
}