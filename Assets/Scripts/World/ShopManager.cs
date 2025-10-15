using UnityEngine;

public class ShopManager : MonoBehaviour
{

    public ItemSO itemToSell;
    public int price;

    public void BuyItem()
    {
        bool success = InventoryManager.instance.AddItem(itemToSell, 1);
        if (success)
        {
            Debug.Log($"Comprado: {itemToSell.itemName}");
        }
    }
}