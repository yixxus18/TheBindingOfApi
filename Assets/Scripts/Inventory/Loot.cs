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

    [Header("Audio (Opcional)")]
    public AudioClip pickupSound;

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
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
        Destroy(gameObject, 0.5f);
        Debug.Log($"Recogido: {itemSO.itemName} x{quantity}");
    }

    public static void SpawnLoot(ItemSO item, int amount, Vector3 position)
    {
        GameObject lootPrefab = Resources.Load<GameObject>("Prefabs/Loot");

        if (lootPrefab == null)
        {
            Debug.LogError("No se encontró el prefab de Loot en Resources/Prefabs/Loot");
            return;
        }

        GameObject lootObj = Instantiate(lootPrefab, position, Quaternion.identity);
        Loot loot = lootObj.GetComponent<Loot>();

        if (loot != null)
        {
            loot.Initialize(item, amount);
        }
    }
}
