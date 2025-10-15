using UnityEngine;
using System.IO;
using System.Linq;

public static class SaveSystem
{
    private static readonly string SAVE_FILE = "/player_progress.dat";

    public static void SaveGame(CodexManager codex, StatsManager stats)
    {
        string path = Application.persistentDataPath + SAVE_FILE;
        SaveData data = new SaveData();

        data.learnedRequests = codex.learnedRequests;
        data.discoveredLoreIDs = codex.discoveredLore.Select(l => l.loreID).ToList();
        // data.playerEngineeringLevel = stats.engineeringLevel; 

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log("Juego guardado en: " + path);
    }

    public static void LoadGame(CodexManager codex, StatsManager stats,LoreDatabaseSO loreDatabase)
    {
        string path = Application.persistentDataPath + SAVE_FILE;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            codex.learnedRequests = data.learnedRequests;
            codex.discoveredLore.Clear();
            foreach (string id in data.discoveredLoreIDs)
            {
                LoreSO lore = loreDatabase.GetLoreByID(id);
                if (lore != null) codex.discoveredLore.Add(lore);
            }
            // stats.engineeringLevel = data.playerEngineeringLevel; 
            Debug.Log("Juego cargado.");
        }
    }
}