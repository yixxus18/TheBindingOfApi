using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool gameIsPaused = false;

    [Header("UI References")]
    public GameObject pauseMenuPanel;
    public GameObject darkOverlay;
    public Button pauseButton;

    [Header("Settings Menu Elements")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Toggle fullscreenToggle;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            gameObject.SetActive(false);
            return;
        }

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (darkOverlay != null)
            darkOverlay.SetActive(false);
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            ConnectToSettingsManager();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void TogglePause()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.PlayButtonClickSound();
        }

        if (gameIsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        pauseMenuPanel.SetActive(true);
        darkOverlay.SetActive(true);
        Time.timeScale = 0f; 
        gameIsPaused = true;
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(false);

        LoadCurrentSettings();
    }

    public void Resume()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.PlayButtonClickSound();
        }

        pauseMenuPanel.SetActive(false);
        darkOverlay.SetActive(false);
        Time.timeScale = 1f; 
        gameIsPaused = false;
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(true);
    }

    public void LoadMainMenu()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.PlayButtonClickSound();
        }

        Time.timeScale = 1f;
        gameIsPaused = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.PlayButtonClickSound();
        }

        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("Quitting Game...");
    }

    private void ConnectToSettingsManager()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.masterVolumeSlider = masterVolumeSlider;
            SettingsManager.Instance.musicVolumeSlider = musicVolumeSlider;
            SettingsManager.Instance.fullscreenToggle = fullscreenToggle;

            LoadCurrentSettings();
        }
        else
        {
            Debug.LogWarning("SettingsManager.Instance no encontrado. Asegúrate de que existe en la escena.");
        }
    }

    private void LoadCurrentSettings()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.LoadSettings();
        }
    }
}
