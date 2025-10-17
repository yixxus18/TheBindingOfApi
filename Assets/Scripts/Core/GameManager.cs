using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Managers")]
    public UIManager uiManager;
    public CodexManager codexManager;
    public StatsManager statsManager;
    public DungeonObjectiveManager objectiveManager;
    public InventoryManager inventoryManager;
    public ExpManager expManager;

    [Header("Databases")]
    public LoreDatabaseSO loreDatabase;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SaveSystem.LoadGame(codexManager, statsManager, loreDatabase);
    }

    private void OnApplicationQuit()
    {
        SaveSystem.SaveGame(codexManager, statsManager);
    }
}