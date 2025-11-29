using UnityEngine;
using System;
using System.Collections.Generic;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager instance;
    public static event Action OnLevelUnlocked;

    public HashSet<string> completedObjectiveIDs = new HashSet<string>();
    public int highestLevelUnlocked = 1;

    public Dictionary<int, int> npcConversationStates = new Dictionary<int, int>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool IsObjectiveCompleted(string objectiveId)
    {
        return completedObjectiveIDs.Contains(objectiveId);
    }

    public bool IsLevelUnlocked(int levelIndex)
    {
        return levelIndex <= highestLevelUnlocked;
    }

    public void CompleteObjective(string objectiveId)
    {
        if (!completedObjectiveIDs.Contains(objectiveId))
        {
            completedObjectiveIDs.Add(objectiveId);
        }
    }

    public void UnlockNextLevel()
    {
        if (highestLevelUnlocked < 100)
        {
            highestLevelUnlocked++;
            OnLevelUnlocked?.Invoke();
        }
    }

    public int GetNPCConversationIndex(int npcID)
    {
        if (npcConversationStates.ContainsKey(npcID))
        {
            return npcConversationStates[npcID];
        }
        return 0;
    }

    public void SetNPCConversationIndex(int npcID, int index)
    {
        if (npcConversationStates.ContainsKey(npcID))
        {
            npcConversationStates[npcID] = index;
        }
        else
        {
            npcConversationStates.Add(npcID, index);
        }
    }
}