using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelDoor : MonoBehaviour
{
    [Header("Configuración del Nivel")]
    public int levelIndex;
    public string sceneNameToLoad;

    [Header("Visuales")]
    public GameObject lockedIndicator;
    public GameObject unlockedIndicator;

    private bool isUnlocked = false;
    private bool playerInRange = false;

    void Start()
    {
        RefreshLockState();
    }

    void OnEnable()
    {
        RefreshLockState();
    }

    void Update()
    {
        if (playerInRange && isUnlocked && GameInput.Instance.GetInteractPressed())
        {
            if (GameManager.Instance != null)
            {
                SaveSystem.SaveGame(GameManager.Instance.codexManager, GameManager.Instance.statsManager, GameManager.Instance.inventoryManager);
                Debug.Log("Progreso guardado antes de entrar al nivel " + levelIndex);
            }
            Loader.Load(sceneNameToLoad);
        }
    }

    private void RefreshLockState()
    {
        isUnlocked = ProgressionManager.instance.IsLevelUnlocked(levelIndex);
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (lockedIndicator != null) lockedIndicator.SetActive(!isUnlocked);
        if (unlockedIndicator != null) unlockedIndicator.SetActive(isUnlocked);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}