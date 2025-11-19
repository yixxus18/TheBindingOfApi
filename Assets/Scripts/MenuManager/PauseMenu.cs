using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool gameIsPaused = false;

    [Header("UI References")]
    public GameObject pauseMenuPanel;
    public GameObject darkOverlay;

    [Header("Settings Menu Elements")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Toggle fullscreenToggle;

    private void Awake()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (darkOverlay != null) darkOverlay.SetActive(false);
    }

    private void Start()
    {
        ConnectToSettingsManager();
        RegisterAllButtonSounds();
    }

    private void RegisterAllButtonSounds()
    {
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        foreach (Button btn in allButtons)
        {
            btn.onClick.AddListener(() =>
            {
                if (SettingsManager.Instance != null)
                {
                    SettingsManager.Instance.PlayButtonClickSound();
                }
            });
        }
    }

    void Update()
    {
        if (GameInput.Instance.GetPausePressed())
        {
            TogglePause();
        }
    }

    public void TogglePause()
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

    public void Pause()
    {
        pauseMenuPanel.SetActive(true);
        darkOverlay.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.LoadSettings();
        }
    }

    public void Resume()
    {
        pauseMenuPanel.SetActive(false);
        darkOverlay.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        gameIsPaused = false;
        Loader.Load("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void ConnectToSettingsManager()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.masterVolumeSlider = masterVolumeSlider;
            SettingsManager.Instance.musicVolumeSlider = musicVolumeSlider;
            SettingsManager.Instance.fullscreenToggle = fullscreenToggle;
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.RemoveAllListeners();
                masterVolumeSlider.onValueChanged.AddListener(SettingsManager.Instance.SetMasterVolume);
            }
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.RemoveAllListeners();
                musicVolumeSlider.onValueChanged.AddListener(SettingsManager.Instance.SetMusicVolume);
            }
            if (fullscreenToggle != null)
            {
                fullscreenToggle.onValueChanged.RemoveAllListeners();
                fullscreenToggle.onValueChanged.AddListener(SettingsManager.Instance.SetFullscreen);
            }
        }
        else
        {
            Debug.LogWarning("SettingsManager.Instance no encontrado.");
        }
    }
}