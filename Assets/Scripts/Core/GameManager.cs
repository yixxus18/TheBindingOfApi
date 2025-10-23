using UnityEngine;
using UnityEngine.SceneManagement;

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
    public ProgressionManager progressionManager;

    [Header("Databases")]
    public LoreDatabaseSO loreDatabase;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SaveSystem.LoadGame(codexManager, statsManager, loreDatabase);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "IntroScene")
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void OnApplicationQuit()
    {
        SaveSystem.SaveGame(codexManager, statsManager);
    }
}