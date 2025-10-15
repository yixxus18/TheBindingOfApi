using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public List<RequestEntry> learnedRequests;
    public List<string> discoveredLoreIDs;
    public int playerEngineeringLevel;
    public List<string> unlockedDungeons;

    public SaveData()
    {
        learnedRequests = new List<RequestEntry>();
        discoveredLoreIDs = new List<string>();
        unlockedDungeons = new List<string>();
        playerEngineeringLevel = 1;
    }
}