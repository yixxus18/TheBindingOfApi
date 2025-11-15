using UnityEngine;
using System;
using System.Collections.Generic;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager instance;
    public static event Action OnLevelUnlocked;

    public HashSet<string> completedObjectiveIDs = new HashSet<string>();
    public int highestLevelUnlocked = 0;

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
            Debug.Log($"Progreso permanente guardado: Objetivo '{objectiveId}' completado.");
        }
    }

    public void UnlockNextLevel()
    {
        highestLevelUnlocked++;
        Debug.Log($"¡Nivel {highestLevelUnlocked} desbloqueado!");
        OnLevelUnlocked?.Invoke();
    }
}