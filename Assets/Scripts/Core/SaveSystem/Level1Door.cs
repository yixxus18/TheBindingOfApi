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
        ProgressionManager.OnLevelUnlocked += RefreshLockState;
        RefreshLockState();
    }

    void OnDisable()
    {
        ProgressionManager.OnLevelUnlocked -= RefreshLockState;
    }

    void Update()
    {
        if (playerInRange && GameInput.Instance.GetInteractPressed())
        {
            if (!isUnlocked)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.doorLockedSound);
                }
            }
            else
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
                    Debug.Log("Progreso guardado antes de entrar al nivel " + levelIndex);
                }
                Loader.Load(sceneNameToLoad);
            }
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