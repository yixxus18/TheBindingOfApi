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
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !DialogueManager.instance.isDialogueActive)
        {
            ToggleShop();
        }
    }

    public void ToggleShop()
    {
        bool isActive = !shopPanel.activeSelf;
        shopPanel.SetActive(isActive);
        UIManager.isGamePaused = isActive;

        if (isActive)
        {
            ShopManager.instance.PopulateShop(itemsForSale);
        }
        else
        {
            if (TooltipManager.instance != null)
            {
                TooltipManager.instance.HideTooltip();
            }
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
            UIManager.isGamePaused = false;
            if (TooltipManager.instance != null)
            {
                TooltipManager.instance.HideTooltip();
            }
        }
    }
}