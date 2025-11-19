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
    public AudioClip menuNavigationSound; // beep-6-96243
    public AudioClip openTerminalSound;   // computer-processing...
    public AudioClip gameStartSound;      // game-start-6104
    public AudioClip terminalErrorSound;  // error-126627
    public AudioClip terminalSuccessSound;// SE_EXTND10_HORN_01
    public AudioClip processingRequestSound; // sdn_egg
    public AudioClip levelCompleteSound;  // snd_won
    public AudioClip openBookSound;       // text-notification-96707
    public AudioClip powerUpSound;        // video-game-powerup-38065
    public AudioClip typingSound;         // El mismo de la intro

    [Header("Jugador")]
    public AudioClip playerStepSound;     // snd_step2
    public AudioClip playerAttackSound;   // sword-sound-2
    public AudioClip playerHurtSound;     // hurt_c_08
    public AudioClip playerDeathSound;    // snd_closet_fall

    [Header("Mundo")]
    public AudioClip doorLockedSound;     // door-closed
    public AudioClip chestOpenSound;      // wooden-trunk-latch
    public AudioClip itemDropSound;       // treasure-chest-open-sfx
    public AudioClip coinPickupSound;     // retro-coin-4

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