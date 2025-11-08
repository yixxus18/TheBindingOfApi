using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] public GameObject optionsMenuPanel; // ¡Hazla pública para que SettingsManager pueda encontrarla!

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip buttonClickSound;

    private void Start()
    {
        // Asegúrate de que al inicio se muestre el menú principal
        ShowMainMenuPanel();
    }

    public void ShowOptionsMenu()
    {
        PlayButtonSound();
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
        PlayButtonSound();
        if (optionsMenuPanel != null) optionsMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        PlayButtonSound();
        PlayerPrefs.Save();
        Application.Quit();
    }

    public void StartGame()
    {
        PlayButtonSound();
        UnityEngine.SceneManagement.SceneManager.LoadScene("IntroEscene");
    }

    private void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
}