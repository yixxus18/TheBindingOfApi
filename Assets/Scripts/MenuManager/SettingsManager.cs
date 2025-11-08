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

    [Header("Audio Sources")]
    public AudioSource musicAudioSource;
    public AudioSource buttonClickAudioSource;
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
        if (buttonClickAudioSource == null)
        {
            buttonClickAudioSource = gameObject.AddComponent<AudioSource>();
            buttonClickAudioSource.playOnAwake = false;
            buttonClickAudioSource.loop = false;
        }
    }

    public void PlayButtonClickSound()
    {
        if (buttonClickAudioSource != null && buttonClickSound != null)
        {
            buttonClickAudioSource.PlayOneShot(buttonClickSound);
        }
    }

    private void FindMusicAudioSource()
    {
        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCamera != null)
        {
            musicAudioSource = mainCamera.GetComponent<AudioSource>();
        }
    }

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = volume;
        }
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    public void LoadSettings()
    {
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        AudioListener.volume = masterVolume;
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVolume;
        }

        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = musicVolume;
        }
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume;
        }

        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        Screen.fullScreen = isFullscreen;
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = isFullscreen;
        }
    }
}