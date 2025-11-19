using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public enum ObjectiveType { KillBoss, SolvePuzzle, CollectItem }

[System.Serializable]
public class DungeonObjective
{
    public string description;
    public ObjectiveType type;
    public string targetId; // Almacena el ID (ya sea "10" para items o "PUZZLE_01" para puzzles)
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
            // Verificar si ya se completó anteriormente usando el ProgressionManager
            if (ProgressionManager.instance.IsObjectiveCompleted(obj.targetId))
            {
                obj.isCompleted = true;
                obj.currentAmount = obj.requiredAmount;
            }
            else
            {
                // Caso especial para items: Verificar si ya los tiene en el inventario
                if (obj.type == ObjectiveType.CollectItem && int.TryParse(obj.targetId, out int itemId))
                {
                    // Buscar el item en la base de datos para comprobar cantidad actual
                    ItemSO item = GameManager.Instance.itemDatabase.GetItemByID(itemId);
                    if (item != null)
                    {
                        int count = InventoryManager.instance.GetItemCount(item);
                        obj.currentAmount = count;
                        if (count >= obj.requiredAmount)
                        {
                            obj.isCompleted = true;
                            // Marcar como completado en progresión inmediatamente si ya tiene los items
                            ProgressionManager.instance.CompleteObjective(obj.targetId);
                        }
                    }
                }
                else
                {
                    obj.currentAmount = 0;
                    obj.isCompleted = false;
                }
            }
            currentObjectives.Add(obj);
        }
        UpdateUI();
        CheckForAllObjectivesCompleted();
    }

    public void NotifyProgress(ObjectiveType type, string id, int amount = 1)
    {
        foreach (var obj in currentObjectives)
        {
            // Comparamos ID y Tipo. El ID viene como string ("10", "PUZZLE_ID")
            if (!obj.isCompleted && obj.type == type && obj.targetId == id)
            {
                obj.currentAmount += amount;

                // Si es de tipo recolección, asegurarse de no superar el máximo visualmente
                if (obj.currentAmount >= obj.requiredAmount)
                {
                    obj.currentAmount = obj.requiredAmount;
                    obj.isCompleted = true;
                    ProgressionManager.instance.CompleteObjective(obj.targetId);
                }

                UpdateUI();
                CheckForAllObjectivesCompleted();
                return;
            }
        }
    }

    private void CheckForAllObjectivesCompleted()
    {
        if (currentObjectives.Count > 0 && currentObjectives.All(obj => obj.isCompleted))
        {
            // Lógica específica del Hub
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Hub" &&
                ProgressionManager.instance.highestLevelUnlocked == 0)
            {
                ProgressionManager.instance.UnlockNextLevel();

                if (GameManager.Instance != null)
                {
                    SaveSystem.SaveGame(
                        GameManager.Instance.codexManager,
                        GameManager.Instance.statsManager,
                        GameManager.Instance.inventoryManager,
                        GameManager.Instance.expManager
                    );
                }
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

            string colorTag = obj.isCompleted ? "<color=green>" : "<color=white>";
            string status = obj.isCompleted ? "[HECHO]" : $"[{obj.currentAmount}/{obj.requiredAmount}]";

            text.text = $"{colorTag}{obj.description} {status}</color>";

            if (obj.isCompleted) text.fontStyle = FontStyles.Strikethrough;
        }
    }

    public void ClearObjectives()
    {
        currentObjectives.Clear();
        UpdateUI();
    }
}