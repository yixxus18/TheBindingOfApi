using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance { get; private set; }

    [Header("Loading Screen Prefab")]
    [Tooltip("El Prefab del Canvas que contiene la pantalla de carga.")]
    public GameObject loadingScreenPrefab;

    private GameObject loadingScreenInstance;

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
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Notificar al GameManager para que actualice la UI
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSceneChanged(scene.name);
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        loadingScreenInstance = Instantiate(loadingScreenPrefab);
        DontDestroyOnLoad(loadingScreenInstance);
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone)
        {
            yield return null;
        }
        Destroy(loadingScreenInstance);
    }
}