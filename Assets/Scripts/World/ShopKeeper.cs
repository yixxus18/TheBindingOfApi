using UnityEngine;

public class ShopKeeper : MonoBehaviour
{
    public GameObject shopPanel; 
    private bool playerInRange;

    private void Update()
    {
        if (playerInRange && Input.GetButtonDown("Interact"))
        {
            shopPanel.SetActive(!shopPanel.activeSelf);
            Time.timeScale = shopPanel.activeSelf ? 0 : 1;
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