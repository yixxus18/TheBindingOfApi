using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class LevelSelector : MonoBehaviour
{
    public GameObject levelButtonPrefab;
    public Transform buttonContainer;
    public int levelsCount = 10;
    void Start()
    {
        CreateLevelButtons();
    }

    void CreateLevelButtons()
    {
        for (int i = 1; i <= levelsCount; i++)
        {
            GameObject button = Instantiate(levelButtonPrefab, buttonContainer);
            button.GetComponentInChildren<TMP_Text>().text = "Nivel " + i;
            int levelIndex = i;
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Level_" + levelIndex);
            });
        }
    }
}
