using System.Collections.Generic; 
using UnityEngine; 
using TMPro; 
 
public enum ObjectiveType { KillBoss, SolvePuzzle, CollectItem }

[System.Serializable]
public class DungeonObjective
{
    public string description;
    public ObjectiveType type;
    public string targetId;
    public int requiredAmount;
    [HideInInspector] public int currentAmount;
    [HideInInspector] public bool isCompleted;
}

public class DungeonObjectiveManager : MonoBehaviour
{
    public static DungeonObjectiveManager instance;

    public List<DungeonObjective> currentObjectives = new List<DungeonObjective>();

    [Header("UI References")]
    public Transform objectivesContainer;
    public GameObject objectiveUIPrefab;

    private void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
    }

    public void SetDungeonObjectives(List<DungeonObjective> newObjectives)
    {
        ClearObjectives();
        foreach (var obj in newObjectives)
        {
            obj.currentAmount = 0;
            obj.isCompleted = false;
            currentObjectives.Add(obj);
        }
        UpdateUI();
    }

    public void NotifyProgress(ObjectiveType type, string id, int amount = 1)
    {
        foreach (var obj in currentObjectives)
        {
            if (!obj.isCompleted && obj.type == type && obj.targetId == id)
            {
                obj.currentAmount += amount;
                if (obj.currentAmount >= obj.requiredAmount)
                {
                    obj.isCompleted = true;
                }
                UpdateUI();
                return;
            }
        }
    }

    public void UpdateUI()
    {
        foreach (Transform child in objectivesContainer) Destroy(child.gameObject);
        if (objectiveUIPrefab == null) return;
        foreach (var obj in currentObjectives)
        {
            GameObject go = Instantiate(objectiveUIPrefab, objectivesContainer);
            TMP_Text text = go.GetComponent<TMP_Text>();
            string status = obj.isCompleted ? "<color=green>[HECHO]</color>" : $"[{obj.currentAmount}/{obj.requiredAmount}]";
            text.text = $"{obj.description} {status}";
            if (obj.isCompleted) text.fontStyle = FontStyles.Strikethrough;
        }
    }

    public void ClearObjectives()
    {
        currentObjectives.Clear();
        UpdateUI();
    }
}