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

    [Header("Level Managers")]
    public MinimapManager minimapManager;
    public MapGenerator mapGenerator;

    [Header("Databases")]
    public LoreDatabaseSO loreDatabase;
    public ItemDatabaseSO itemDatabase;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SaveSystem.LoadGame(codexManager, statsManager, inventoryManager, expManager, loreDatabase, itemDatabase);
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
        HandleSceneChange(scene.name);
    }
    
    /// <summary>
    /// Método público que puede ser llamado por LoadingManager para asegurar
    /// que la UI se actualice cuando cambia la escena.
    /// </summary>
    public void OnSceneChanged(string sceneName)
    {
        HandleSceneChange(sceneName);
    }
    
    private void HandleSceneChange(string sceneName)
    {
        Debug.Log($"[GameManager] Escena cargada: '{sceneName}'");
        
        // Escenas donde NO queremos mostrar la UI persistente
        bool hideUI = sceneName == "IntroEscene" || 
                      sceneName == "IntroScene" || 
                      sceneName == "MainMenu" ||
                      sceneName == "EndScene";
        
        if (hideUI)
        {
            Cursor.visible = (sceneName == "MainMenu" || sceneName == "EndScene");
            Cursor.lockState = (sceneName == "MainMenu" || sceneName == "EndScene") ? CursorLockMode.None : CursorLockMode.Locked;
            
            // Ocultar los canvas persistentes
            SetPersistentUIVisible(false);
        }
        else
        {
            // Hub, Level_1, Level_2, etc. - Mostrar todo
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            
            // Mostrar los canvas
            SetPersistentUIVisible(true);
        }

        minimapManager = FindFirstObjectByType<MinimapManager>();
        mapGenerator = FindFirstObjectByType<MapGenerator>();
    }
    
    /// <summary>
    /// Oculta o muestra todos los canvas UI persistentes que son hijos del _GAME_MANAGERS_
    /// </summary>
    private void SetPersistentUIVisible(bool visible)
    {
        int count = 0;
        
        // Recorrer todos los hijos directos del GameManager (_GAME_MANAGERS_)
        foreach (Transform child in transform)
        {
            // Si el hijo tiene un Canvas, activar/desactivar el GameObject completo
            Canvas canvas = child.GetComponent<Canvas>();
            if (canvas != null)
            {
                child.gameObject.SetActive(visible);
                count++;
            }
        }
        
        // Ocultar tooltips si existen (puede estar fuera del GameManager)
        if (TooltipManager.instance != null)
        {
            TooltipManager.instance.gameObject.SetActive(visible);
        }
        
        Debug.Log($"[GameManager] {count} UI(s) {(visible ? "ACTIVADAS" : "DESACTIVADAS")} - Escena: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
    }

    private void OnApplicationQuit()
    {
        SaveSystem.SaveGame(codexManager, statsManager, inventoryManager, expManager);
    }
}