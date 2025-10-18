// ShopKeeper.cs
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ShopItem
{
    public ItemSO item;
    public int price;
}

public class ShopKeeper : MonoBehaviour
{
    public GameObject shopPanel;
    public List<ShopItem> itemsForSale;
    private bool playerInRange;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleShop();
        }
    }

    public void ToggleShop()
    {
        bool isActive = !shopPanel.activeSelf;
        shopPanel.SetActive(isActive);
        Time.timeScale = isActive ? 0 : 1;

        if (isActive)
        {
            ShopManager.instance.PopulateShop(itemsForSale);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            shopPanel.SetActive(false);
            Time.timeScale = 1;
        }
    }
}