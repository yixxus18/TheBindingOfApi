using UnityEngine;
using System;

public class Loot : MonoBehaviour
{
    [Header("Item Configuration")]
    public ItemSO itemSO;
    public int quantity = 1;

    [Header("Visual Components")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    [Header("Pickup Settings")]
    public bool canBePickedUp = true;
    public float pickupDelay = 0.5f;

    public static event Action<ItemSO, int> OnItemLooted;

    private void Start()
    {
        if (itemSO != null)
        {
            UpdateAppearance();
        }

        canBePickedUp = false;
        Invoke(nameof(EnablePickup), pickupDelay);
    }

    private void OnValidate()
    {
        if (itemSO == null)
            return;

        UpdateAppearance();
    }

    public void Initialize(ItemSO item, int amount)
    {
        itemSO = item;
        quantity = amount;
        canBePickedUp = false;
        UpdateAppearance();

        Invoke(nameof(EnablePickup), pickupDelay);
    }

    private void UpdateAppearance()
    {
        if (spriteRenderer != null && itemSO != null)
        {
            spriteRenderer.sprite = itemSO.icon;
            gameObject.name = $"Loot_{itemSO.itemName}";
        }
    }

    private void EnablePickup()
    {
        canBePickedUp = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && canBePickedUp && itemSO != null)
        {
            PickupItem();
        }
    }

    private void PickupItem()
    {
        canBePickedUp = false;
        OnItemLooted?.Invoke(itemSO, quantity);

        if (animator != null)
        {
            animator.Play("LootPickup");
        }

        if (AudioManager.Instance != null)
        {
            if (itemSO.isGold && AudioManager.Instance.coinPickupSound != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.coinPickupSound);
            }
            else if (AudioManager.Instance.itemDropSound != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.itemDropSound);
            }
        }

        Destroy(gameObject, 0.5f);
    }
}