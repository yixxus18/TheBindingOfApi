using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] public GameObject optionsMenuPanel;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip buttonClickSound;

    private void Start()
    {
        ShowMainMenuPanel();
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

    public void ShowOptionsMenu()
    {
        mainMenuPanel.SetActive(false);
        optionsMenuPanel.SetActive(true);
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.FindUIReferences("MainMenu");
            SettingsManager.Instance.LoadSettings();
        }
    }

    public void ShowMainMenuPanel()
    {
        if (optionsMenuPanel != null) optionsMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        PlayerPrefs.Save();
        Application.Quit();
    }

    public void StartGame()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.gameStartSound != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.gameStartSound);
        }
        Loader.Load("IntroEscene");
    }
}