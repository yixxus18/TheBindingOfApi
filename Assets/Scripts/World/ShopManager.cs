// ShopManager.cs
using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    [Header("UI References")]
    public GameObject shopSlotPrefab;
    public Transform slotContainer;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void PopulateShop(List<ShopItem> items)
    {
        // Limpiar slots antiguos
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }

        // Crear nuevos slots
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