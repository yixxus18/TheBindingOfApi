using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject optionsMenu;

    public AudioSource audioSource;
    public AudioClip buttonClickSound;

    void Start()
    {

    }
    public void ShowOptionsMenu()
    {
        PlayButtonSound();
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);

        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.LoadSettings();
        }
    }

    public void ShowMainMenu()
    {
        PlayButtonSound();
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void QuitGame()
    {
        PlayButtonSound();
        PlayerPrefs.Save();
        Application.Quit();
        Debug.Log("Game is exiting...");
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
