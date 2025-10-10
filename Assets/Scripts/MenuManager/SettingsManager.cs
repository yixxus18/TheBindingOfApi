using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
            return;
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
        LoadSettings();
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindMusicAudioSource();
        FindUIReferences(scene.name);
        LoadSettings();
    }

    private void FindUIReferences(string sceneName)
    {
        if (sceneName == "MainMenu")
        {
            StartCoroutine(FindMainMenuReferences());
        }
    }

    private System.Collections.IEnumerator FindMainMenuReferences()
    {
        yield return null;
        MainMenu mainMenu = FindFirstObjectByType<MainMenu>();

        if (mainMenu != null)
        {
            Slider[] sliders = mainMenu.optionsMenu.GetComponentsInChildren<Slider>(true);
            Toggle[] toggles = mainMenu.optionsMenu.GetComponentsInChildren<Toggle>(true);

            foreach (Slider slider in sliders)
            {
                if (slider.name.ToLower().Contains("master"))
                {
                    masterVolumeSlider = slider;
                    slider.onValueChanged.RemoveAllListeners();
                    slider.onValueChanged.AddListener(SetMasterVolume);
                }
                else if (slider.name.ToLower().Contains("music"))
                {
                    musicVolumeSlider = slider;
                    slider.onValueChanged.RemoveAllListeners();
                    slider.onValueChanged.AddListener(SetMusicVolume);
                }
            }

            foreach (Toggle toggle in toggles)
            {
                if (toggle.name.ToLower().Contains("fullscreen"))
                {
                    fullscreenToggle = toggle;
                    toggle.onValueChanged.RemoveAllListeners();
                    toggle.onValueChanged.AddListener(SetFullscreen);
                }
            }

            LoadSettings();
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
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float volume)
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = volume;
        }

        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
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
        AudioListener.volume = masterVolume;
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = masterVolume;

        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = musicVolume;
        }
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = musicVolume;

        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        Screen.fullScreen = isFullscreen;
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = isFullscreen;
    }

    public void ResetSettings()
    {
        SetMasterVolume(1.0f);
        SetMusicVolume(1.0f);
        SetFullscreen(true);
        LoadSettings();
    }
}
