using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class LootChest : MonoBehaviour
{
    [Header("Configuración")]
    public List<LootTableEntry> lootTable;
    public bool guaranteedDrop = false;

    [Header("Componentes")]
    public Animator animator;
    private bool isOpen = false;
    private bool playerInRange = false;

    private void Update()
    {
        if (playerInRange && !isOpen && GameInput.Instance.GetInteractPressed())
        {
            OpenChest();
        }
    }

    public void OpenChest()
    {
        isOpen = true;

        if (animator != null)
        {
            animator.Play("chest_open");
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.chestOpenSound);
        }

        DropItem();

        GetComponent<BoxCollider2D>().enabled = false;
    }

    private void DropItem()
    {
        if (lootTable == null || lootTable.Count == 0) return;

        if (guaranteedDrop)
        {
            SpawnItem(lootTable[0].itemToDrop);
        }
        else
        {
            foreach (var entry in lootTable)
            {
                if (Random.value <= entry.dropChance)
                {
                    SpawnItem(entry.itemToDrop);
                    return;
                }
            }

            SpawnItem(lootTable[0].itemToDrop);
        }
    }

    private void SpawnItem(ItemSO item)
    {
        if (item.isGold)
        {
            StatsManager.instance.AddGold(item.goldAmount);
        }
        else
        {
            LootSpawner.Instance.SpawnLoot(item, 1, transform.position + Vector3.down);
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