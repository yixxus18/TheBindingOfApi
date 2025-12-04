using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Fuentes de Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource typingSource;

    [Header("UI & Sistema")]
    public AudioClip menuNavigationSound;
    public AudioClip openTerminalSound;
    public AudioClip gameStartSound;
    public AudioClip terminalErrorSound;
    public AudioClip terminalSuccessSound;
    public AudioClip processingRequestSound;
    public AudioClip levelCompleteSound;
    public AudioClip openBookSound;
    public AudioClip powerUpSound;
    public AudioClip typingSound;

    [Header("Jugador")]
    public AudioClip playerStepSound;
    public AudioClip playerAttackSound;
    public AudioClip playerHurtSound;
    public AudioClip playerDeathSound;

    [Header("Mundo")]
    public AudioClip doorLockedSound;
    public AudioClip chestOpenSound;
    public AudioClip itemDropSound;
    public AudioClip coinPickupSound;

    [Header("Configuración de Música por Escena")]
    [SerializeField] private List<SceneMusic> sceneMusicList;

    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip musicClip;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
        UpdateVolumes();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    public void PlayMusicForScene(string sceneName)
    {
        AudioClip clipToPlay = null;

        foreach (var item in sceneMusicList)
        {
            if (item.sceneName == sceneName)
            {
                clipToPlay = item.musicClip;
                break;
            }
        }

        if (clipToPlay != null)
        {
            if (musicSource.clip != clipToPlay)
            {
                musicSource.clip = clipToPlay;
                musicSource.Play();
            }
        }
        else
        {
            if (musicSource.isPlaying && sceneName != "EndScene")
            {
                musicSource.Stop();
                musicSource.clip = null;
            }
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip) return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null)
        {
            sfxSource.pitch = Random.Range(0.95f, 1.05f);
            sfxSource.PlayOneShot(clip, volumeScale);
            sfxSource.pitch = 1f;
        }
    }

    public void PlayTypingSound()
    {
        if (typingSound != null && typingSource != null)
        {
            typingSource.PlayOneShot(typingSound, 0.5f);
        }
    }

    public void UpdateVolumes()
    {
        if (SettingsManager.Instance != null)
        {
            musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
            float sfxVol = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
            sfxSource.volume = sfxVol;
            if (typingSource != null) typingSource.volume = sfxVol;
        }
    }
}