using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EndGameManager : MonoBehaviour
{
    [Header("Pantallas")]
    public CanvasGroup blackScreenGroup;
    public RawImage videoDisplayImage;

    [Header("Cinemática")]
    public VideoPlayer cinematicPlayer;

    [Header("Créditos")]
    public RectTransform creditsPanel;
    public float creditsScrollSpeed = 50f;
    public float creditsDuration = 30f;
    public AudioClip creditsMusic;

    [Header("Historia Final (Texto)")]
    public TMP_Text finalStoryText;
    [TextArea(3, 10)]
    public string fullStoryText;
    public float typingSpeed = 0.05f;

    private bool isSkipping = false;
    private Coroutine typingCoroutine;

    private void Start()
    {
        if (videoDisplayImage != null) videoDisplayImage.gameObject.SetActive(false);
        if (creditsPanel != null) creditsPanel.gameObject.SetActive(false);

        StartCoroutine(EndGameSequence());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
        {
            isSkipping = true;
        }
    }

    private IEnumerator EndGameSequence()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.StopMusic();

        blackScreenGroup.alpha = 1;
        blackScreenGroup.gameObject.SetActive(true);

        if (finalStoryText != null)
        {
            finalStoryText.text = "";
            typingCoroutine = StartCoroutine(TypeSentence(fullStoryText));

            while (typingCoroutine != null)
            {
                if (isSkipping)
                {
                    StopCoroutine(typingCoroutine);
                    finalStoryText.text = fullStoryText;
                    typingCoroutine = null;
                    isSkipping = false;
                    yield return new WaitForSeconds(0.5f);
                }
                yield return null;
            }

            float waitTimer = 0f;
            while (waitTimer < 4.0f)
            {
                if (isSkipping) break;
                waitTimer += Time.deltaTime;
                yield return null;
            }
        }

        isSkipping = false;

        if (cinematicPlayer != null && videoDisplayImage != null)
        {
            finalStoryText.text = "";

            cinematicPlayer.Prepare();
            while (!cinematicPlayer.isPrepared) yield return null;

            videoDisplayImage.gameObject.SetActive(true);
            blackScreenGroup.alpha = 0;

            cinematicPlayer.Play();

            while (cinematicPlayer.isPlaying)
            {
                if (isSkipping) break;
                yield return null;
            }

            cinematicPlayer.Stop();
            videoDisplayImage.gameObject.SetActive(false);
        }

        isSkipping = false;
        blackScreenGroup.alpha = 1;

        if (creditsPanel != null)
        {
            creditsPanel.gameObject.SetActive(true);

            if (AudioManager.Instance != null && creditsMusic != null)
            {
                AudioManager.Instance.PlayMusic(creditsMusic);
            }

            float timer = 0;
            while (timer < creditsDuration)
            {
                if (isSkipping) break;

                float speed = isSkipping ? creditsScrollSpeed * 5 : creditsScrollSpeed;
                creditsPanel.anchoredPosition += new Vector2(0, speed * Time.deltaTime);

                timer += Time.deltaTime;
                yield return null;
            }
        }

        yield return new WaitForSeconds(2f);
        Loader.Load("Hub");
    }

    private IEnumerator TypeSentence(string sentence)
    {
        finalStoryText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            finalStoryText.text += letter;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayTypingSound();
            }
            yield return new WaitForSeconds(typingSpeed);
        }
        typingCoroutine = null;
    }
}