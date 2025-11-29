using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;

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
            if (ProgressionManager.instance.IsObjectiveCompleted(obj.targetId))
            {
                obj.isCompleted = true;
                obj.currentAmount = obj.requiredAmount;
            }
            else
            {
                if (obj.type == ObjectiveType.CollectItem && int.TryParse(obj.targetId, out int itemId))
                {
                    ItemSO item = GameManager.Instance.itemDatabase.GetItemByID(itemId);
                    if (item != null)
                    {
                        int count = InventoryManager.instance.GetItemCount(item);
                        obj.currentAmount = count;
                        if (count >= obj.requiredAmount)
                        {
                            obj.isCompleted = true;
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
        CheckForAllObjectivesCompleted(true);
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
                    obj.currentAmount = obj.requiredAmount;
                    obj.isCompleted = true;
                    ProgressionManager.instance.CompleteObjective(obj.targetId);
                }
                UpdateUI();
                CheckForAllObjectivesCompleted(false);
                return;
            }
        }
    }

    private void CheckForAllObjectivesCompleted(bool isInitialCheck)
    {
        if (currentObjectives.Count > 0 && currentObjectives.All(obj => obj.isCompleted))
        {
            if (SceneManager.GetActiveScene().name == "Hub")
            {
                return;
            }

            if (isInitialCheck)
            {
                if (MinimapManager.instance != null)
                {
                    MinimapManager.instance.RevealAllMap();
                }
            }
            else
            {
                StartCoroutine(LevelCompleteRoutine());
            }
        }
    }

    private IEnumerator LevelCompleteRoutine()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.levelCompleteSound);
        }

        if (GameManager.Instance != null)
        {
            SaveSystem.SaveGame(
                GameManager.Instance.codexManager,
                GameManager.Instance.statsManager,
                GameManager.Instance.inventoryManager,
                GameManager.Instance.expManager
            );
        }

        ProgressionManager.instance.UnlockNextLevel();

        if (MinimapManager.instance != null) MinimapManager.instance.RevealAllMap();

        yield return new WaitForSeconds(3f);

        Loader.Load("Hub");
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