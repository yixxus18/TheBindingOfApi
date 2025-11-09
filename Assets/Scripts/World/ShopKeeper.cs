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
    public List<ShopItem> itemsForSale;
    private bool playerInRange;

    void Update()
    {
        if (playerInRange && GameInput.Instance.GetInteractPressed() && !DialogueManager.instance.isDialogueActive)
        {
            ShopManager.instance.OpenShop(itemsForSale);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) playerInRange = false;
    }
}