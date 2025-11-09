using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public static class SaveSystem
{
    public static void SaveGame(CodexManager codex, StatsManager stats, InventoryManager inventory)
    {
        if (DatabaseManager.Instance == null)
        {
            Debug.LogError("DatabaseManager no fue encontrado. No se puede guardar el juego.");
            return;
        }

        SaveData data = new SaveData();

        data.completedObjectiveIDs = ProgressionManager.instance.completedObjectiveIDs.ToList();
        data.highestLevelUnlocked = ProgressionManager.instance.highestLevelUnlocked;

        data.learnedRequests = codex.learnedRequests;
        data.discoveredLoreIDs = codex.discoveredLore.Select(l => l.loreID).ToList();

        data.playerEngineeringLevel = stats.engineering;

        foreach (var slot in inventory.GetActiveSlots())
        {
            if (slot.itemSO != null && slot.quantity > 0)
            {
                data.inventoryItems.Add(new InventoryItemData
                {
                    itemID = slot.itemSO.itemID,
                    quantity = slot.quantity
                });
            }
        }

        DatabaseManager.Instance.SaveGameData(data);
        Debug.Log("Juego guardado en la base de datos SQLite.");
    }

    public static void LoadGame(CodexManager codex, StatsManager stats, InventoryManager inventory, LoreDatabaseSO loreDatabase, ItemDatabaseSO itemDatabase)
    {
        if (DatabaseManager.Instance == null)
        {
            Debug.LogError("DatabaseManager no fue encontrado. No se puede cargar el juego.");
            return;
        }

        SaveData data = DatabaseManager.Instance.LoadGameData();
        if (data == null)
        {
            Debug.LogWarning("No se encontraron datos de guardado para cargar.");
            return;
        }

        ProgressionManager.instance.completedObjectiveIDs = new HashSet<string>(data.completedObjectiveIDs ?? new List<string>());
        ProgressionManager.instance.highestLevelUnlocked = data.highestLevelUnlocked;

        codex.learnedRequests = data.learnedRequests ?? new List<RequestEntry>();
        codex.discoveredLore.Clear();
        foreach (string id in data.discoveredLoreIDs ?? new List<string>())
        {
            LoreSO lore = loreDatabase.GetLoreByID(id);
            if (lore != null)
            {
                codex.discoveredLore.Add(lore);
            }
        }

        stats.engineering = data.playerEngineeringLevel;

        inventory.ClearInventory();
        foreach (var itemData in data.inventoryItems)
        {
            ItemSO itemSO = itemDatabase.GetItemByID(itemData.itemID);
            if (itemSO != null)
            {
                inventory.AddItem(itemSO, itemData.quantity);
            }
        }

        Debug.Log("Juego cargado desde la base de datos SQLite.");
    }
}