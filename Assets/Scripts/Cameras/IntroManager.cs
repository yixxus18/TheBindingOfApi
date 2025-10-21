using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class IntroManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public CanvasGroup textCanvasGroup;
    public TextMeshProUGUI introText;
    public Image screenFadeOverlay;

    public string hubSceneName = "Hub";

    [TextArea(3, 5)]
    public string fullTextToShow;
    public float typingSpeed = 0.05f;
    public float delayBeforeText = 2.0f;
    public float fadeInDuration = 1.5f;
    public float fadeOutDuration = 1.5f;

    public AudioSource sfxSource;
    public AudioClip typingSound;

    private Coroutine typingCoroutine;
    private bool isLoadingNextScene = false;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
        }

        if (introText != null)
        {
            introText.text = "";
        }

        StartCoroutine(IntroSequence());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                introText.text = fullTextToShow;
                typingCoroutine = null;
            }
            else
            {
                StartCoroutine(LoadNextSceneWithFade());
            }
        }
    }

    private IEnumerator IntroSequence()
    {
        yield return StartCoroutine(FadeScreen(1, 0, 1f));
        yield return StartCoroutine(AnimateText());
    }

    private IEnumerator AnimateText()
    {
        yield return new WaitForSeconds(delayBeforeText);
        yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 0, 1, fadeInDuration));

        typingCoroutine = StartCoroutine(TypeSentence(fullTextToShow));
        yield return typingCoroutine;

        yield return new WaitForSeconds(2.0f);
        yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 1, 0, fadeOutDuration));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        introText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            introText.text += letter;
            if (sfxSource != null && typingSound != null)
            {
                sfxSource.PlayOneShot(typingSound);
            }
            yield return new WaitForSeconds(typingSpeed);
        }
        typingCoroutine = null;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        StartCoroutine(LoadNextSceneWithFade());
    }

    private IEnumerator LoadNextSceneWithFade()
    {
        if (isLoadingNextScene)
        {
            yield break;
        }
        isLoadingNextScene = true;

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }

        yield return StartCoroutine(FadeScreen(0, 1, 1f));
        SceneManager.LoadScene(hubSceneName);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, timer / duration);
            yield return null;
        }
        cg.alpha = end;
    }

    private IEnumerator FadeScreen(float startAlpha, float endAlpha, float duration)
    {
        if (screenFadeOverlay == null) yield break;
        Color color = screenFadeOverlay.color;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            screenFadeOverlay.color = color;
            yield return null;
        }
        color.a = endAlpha;
        screenFadeOverlay.color = color;
    }
}