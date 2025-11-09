using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public List<InventoryItemData> inventoryItems = new List<InventoryItemData>();
    public List<RequestEntry> learnedRequests;
    public List<string> discoveredLoreIDs;
    public int playerEngineeringLevel;
    public List<string> unlockedDungeons;

    public List<string> completedObjectiveIDs;
    public int highestLevelUnlocked;

    public SaveData()
    {
        learnedRequests = new List<RequestEntry>();
        discoveredLoreIDs = new List<string>();
        unlockedDungeons = new List<string>();
        playerEngineeringLevel = 1;
        completedObjectiveIDs = new List<string>();
        highestLevelUnlocked = 0;
    }
}

[System.Serializable]
public class InventoryItemData
{
    public string itemID;
    public int quantity;
}