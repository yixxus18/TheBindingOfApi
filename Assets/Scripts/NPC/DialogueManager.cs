using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("UI References")]
    public Image portriat;
    public TMP_Text actorName;
    public TMP_Text dialogueText;
    public CanvasGroup dialogueCanvasGroup;
    public Button[] choiceButtons;

    [Header("Configuración de Fade")]
    public float fadeDuration = 0.2f;

    [Header("Configuración de Texto")]
    public float typingSpeed = 0.04f;

    private DialogueSO currentDialogue;
    private int dialogueIndex;
    public bool isDialogueActive { get; private set; }

    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        dialogueCanvasGroup.alpha = 0;
        dialogueCanvasGroup.interactable = false;
        dialogueCanvasGroup.blocksRaycasts = false;
    }

    public void StartDialogue(DialogueSO dialogue)
    {
        if (isDialogueActive) return;

        isDialogueActive = true;
        currentDialogue = dialogue;
        dialogueIndex = 0;

        StopAllCoroutines();
        StartCoroutine(ShowDialogueProcess());
    }

    private IEnumerator ShowDialogueProcess()
    {
        yield return StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 0, 1, fadeDuration));
        ShowNextDialogueLine();
    }

    public void AdvanceDialogue()
    {
        if (currentDialogue == null || !isDialogueActive) return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            if (dialogueIndex > 0 && dialogueIndex <= currentDialogue.lines.Length)
            {
                dialogueText.text = currentDialogue.lines[dialogueIndex - 1].text;
            }
            typingCoroutine = null;
        }
        else if (dialogueIndex < currentDialogue.lines.Length)
        {
            ShowNextDialogueLine();
        }
        else
        {
            if (currentDialogue.options.Length > 0)
            {
                ShowChoices();
            }
            else
            {
                StartCoroutine(EndDialogueProcess());
            }
        }
    }

    private void ShowNextDialogueLine()
    {
        ClearChoices();
        DialogueLine line = currentDialogue.lines[dialogueIndex];
        portriat.sprite = line.speaker.portrait;
        actorName.text = line.speaker.actorName;

        typingCoroutine = StartCoroutine(TypeSentence(line.text));
        dialogueIndex++;
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed); // Usar Realtime para que funcione con Time.timeScale = 0
        }
        typingCoroutine = null;
    }

    private void ShowChoices()
    {
        dialogueText.text = "";

        for (int i = 0; i < currentDialogue.options.Length; i++)
        {
            if (i < choiceButtons.Length) // Chequeo de seguridad
            {
                var option = currentDialogue.options[i];
                choiceButtons[i].GetComponentInChildren<TMP_Text>().text = option.optiontext;
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].onClick.AddListener(() => ChooseOption(option.nextDialogue));
            }
        }
    }

    private void ChooseOption(DialogueSO dialogueSO)
    {
        if (dialogueSO == null)
            StartCoroutine(EndDialogueProcess());
        else
            StartDialogue(dialogueSO);
    }

    private IEnumerator EndDialogueProcess()
    {
        if (!isDialogueActive) yield break;

        if (currentDialogue != null && currentDialogue.loreToUnlock != null)
        {
            CodexManager.instance.AddLoreEntry(currentDialogue.loreToUnlock);
        }

        yield return StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 1, 0, fadeDuration));

        isDialogueActive = false;
        ClearChoices();
    }

    private void ClearChoices()
    {
        foreach (var button in choiceButtons)
        {
            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false);
        }
    }

    // --- CÓDIGO CORREGIDO Y FINAL ---
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            // La corrección clave está aquí. Usamos Time.unscaledDeltaTime para que el fade
            // funcione incluso si el juego está pausado (Time.timeScale = 0).
            timer += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(start, end, timer / duration);
            yield return new WaitForEndOfFrame(); // Cambiado para mayor suavidad en la UI
        }
        cg.alpha = end;
        cg.interactable = (end > 0);
        cg.blocksRaycasts = (end > 0);
    }
}