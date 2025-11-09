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