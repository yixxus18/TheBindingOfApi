using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Audio Settings")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;

    [Header("SFX Generales")]
    public AudioClip buttonClickSound;

    [Header("Video Settings")]
    public Toggle fullscreenToggle;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupButtonAudioSource();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        FindMusicAudioSource();
        FindUIReferences(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindMusicAudioSource();
        FindUIReferences(scene.name);
    }

    public void FindUIReferences(string sceneName)
    {
        if (sceneName == "MainMenu")
        {
            StartCoroutine(FindMainMenuReferences());
        }
    }

    private IEnumerator FindMainMenuReferences()
    {
        yield return null;

        MainMenu mainMenu = FindFirstObjectByType<MainMenu>();
        if (mainMenu != null && mainMenu.optionsMenuPanel != null)
        {
            Slider[] sliders = mainMenu.optionsMenuPanel.GetComponentsInChildren<Slider>(true);
            Toggle toggle = mainMenu.optionsMenuPanel.GetComponentInChildren<Toggle>(true);

            foreach (var s in sliders)
            {
                if (s.name.ToLower().Contains("master"))
                {
                    masterVolumeSlider = s;
                }
                else if (s.name.ToLower().Contains("music"))
                {
                    musicVolumeSlider = s;
                }
            }
            fullscreenToggle = toggle;

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.RemoveAllListeners();
                masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            }
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.RemoveAllListeners();
                musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            }
            if (fullscreenToggle != null)
            {
                fullscreenToggle.onValueChanged.RemoveAllListeners();
                fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            }

            LoadSettings();
        }
    }

    private void SetupButtonAudioSource()
    {

    }

    public void PlayButtonClickSound()
    {
        if (AudioManager.Instance != null && buttonClickSound != null)
        {
            AudioManager.Instance.PlaySFX(buttonClickSound);
        }
    }

    private void FindMusicAudioSource()
    {

    }

    public void SetMasterVolume(float volume)
    {
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null) AudioManager.Instance.UpdateVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null) AudioManager.Instance.UpdateVolumes();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        if (masterVolumeSlider != null) masterVolumeSlider.value = masterVolume;

        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;

        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        Screen.fullScreen = isFullscreen;
        if (fullscreenToggle != null) fullscreenToggle.isOn = isFullscreen;

        if (AudioManager.Instance != null) AudioManager.Instance.UpdateVolumes();
    }
}