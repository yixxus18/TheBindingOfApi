using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public List<InventoryItemData> inventoryItems = new List<InventoryItemData>();
    public List<RequestEntry> learnedRequests;
    public List<int> discoveredLoreIDs;
    public int playerEngineeringLevel;
    public List<string> unlockedDungeons;
    public List<string> completedObjectiveIDs;
    public int highestLevelUnlocked;

    public int maxHealth;
    public int currentHealth;
    public int power;
    public int speed;
    public int gold;

    public int level;
    public int currentExp;
    public int expToLevel;

    public List<NPCStateData> npcStates;

    public SaveData()
    {
        learnedRequests = new List<RequestEntry>();
        discoveredLoreIDs = new List<int>();
        unlockedDungeons = new List<string>();
        playerEngineeringLevel = 1;
        completedObjectiveIDs = new List<string>();
        highestLevelUnlocked = 0;
        npcStates = new List<NPCStateData>();
    }
}

[System.Serializable]
public class InventoryItemData
{
    public int itemID;
    public int quantity;
}

[System.Serializable]
public class NPCStateData
{
    public string npcID;
    public int conversationIndex;
}