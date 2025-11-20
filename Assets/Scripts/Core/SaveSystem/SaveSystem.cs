using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public static class SaveSystem
{
    public static void SaveGame(CodexManager codex, StatsManager stats, InventoryManager inventory, ExpManager expManager)
    {
        if (DatabaseManager.Instance == null) return;

        SaveData data = new SaveData();

        data.completedObjectiveIDs = ProgressionManager.instance.completedObjectiveIDs.ToList();
        data.highestLevelUnlocked = ProgressionManager.instance.highestLevelUnlocked;

        data.npcStates.Clear();
        foreach (var kvp in ProgressionManager.instance.npcConversationStates)
        {
            data.npcStates.Add(new NPCStateData { npcID = kvp.Key, conversationIndex = kvp.Value });
        }

        data.learnedRequests = codex.learnedRequests;
        data.discoveredLoreIDs = codex.discoveredLore.Select(l => l.loreID).ToList();

        data.playerEngineeringLevel = stats.engineering;
        data.maxHealth = stats.maxHealth;
        data.currentHealth = stats.currentHealth;
        data.power = stats.power;
        data.speed = stats.speed;
        data.gold = stats.gold;

        data.level = expManager.level;
        data.currentExp = expManager.currentExp;
        data.expToLevel = expManager.expToLevel;

        data.inventoryItems.Clear();
        var groupedInventory = inventory.GetActiveSlots()
            .Where(slot => slot.itemSO != null && slot.quantity > 0)
            .GroupBy(slot => slot.itemSO.itemID)
            .Select(group => new InventoryItemData { itemID = group.Key, quantity = group.Sum(slot => slot.quantity) });
        data.inventoryItems.AddRange(groupedInventory);

        DatabaseManager.Instance.SaveGameData(data);
    }

    public static void LoadGame(CodexManager codex, StatsManager stats, InventoryManager inventory, ExpManager expManager, LoreDatabaseSO loreDatabase, ItemDatabaseSO itemDatabase)
    {
        if (DatabaseManager.Instance == null) return;
        SaveData data = DatabaseManager.Instance.LoadGameData();
        if (data == null) return;

        ProgressionManager.instance.completedObjectiveIDs = new HashSet<string>(data.completedObjectiveIDs ?? new List<string>());
        ProgressionManager.instance.highestLevelUnlocked = data.highestLevelUnlocked;

        ProgressionManager.instance.npcConversationStates.Clear();
        if (data.npcStates != null)
        {
            foreach (var npcData in data.npcStates)
            {
                ProgressionManager.instance.npcConversationStates[npcData.npcID] = npcData.conversationIndex;
            }
        }

        codex.learnedRequests.Clear();
        codex.learnedRequests.AddRange(data.learnedRequests ?? new List<RequestEntry>());

        codex.discoveredLore.Clear();
        foreach (int id in data.discoveredLoreIDs ?? new List<int>())
        {
            LoreSO lore = loreDatabase.GetLoreByID(id);
            if (lore != null) codex.discoveredLore.Add(lore);
        }

        stats.engineering = data.playerEngineeringLevel;
        stats.maxHealth = data.maxHealth > 0 ? data.maxHealth : 200;
        stats.currentHealth = data.currentHealth;
        stats.power = data.power > 0 ? data.power : 10;
        stats.speed = data.speed > 0 ? data.speed : 5;
        stats.gold = data.gold;
        stats.TriggerStatsChanged();

        expManager.level = data.level > 0 ? data.level : 1;
        expManager.currentExp = data.currentExp;
        expManager.expToLevel = data.expToLevel > 0 ? data.expToLevel : 100;

        if (PlayerUIManager.instance != null)
        {
            PlayerUIManager.instance.UpdateExpBar();
        }

        inventory.ClearInventory();
        foreach (var itemData in data.inventoryItems)
        {
            ItemSO itemSO = itemDatabase.GetItemByID(itemData.itemID);
            if (itemSO != null) inventory.AddItem(itemSO, itemData.quantity);
        }
    }
}