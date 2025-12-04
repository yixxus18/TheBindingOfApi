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
    private bool isWaitingForChoice = false;

    private Coroutine typingCoroutine;
    private bool isFading = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        dialogueCanvasGroup.alpha = 0;
        dialogueCanvasGroup.interactable = false;
        dialogueCanvasGroup.blocksRaycasts = false;
    }

    private void Update()
    {
        if (isDialogueActive && !isWaitingForChoice && !isFading && Input.GetKeyDown(KeyCode.E))
        {
            AdvanceDialogue();
        }
    }

    public void StartDialogue(DialogueSO dialogue)
    {
        if (isDialogueActive) return;

        isDialogueActive = true;
        isWaitingForChoice = false;
        isFading = true;
        currentDialogue = dialogue;
        dialogueIndex = 0;

        StopAllCoroutines();
        StartCoroutine(ShowDialogueProcess());
    }

    private IEnumerator ShowDialogueProcess()
    {
        yield return StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 0, 1, fadeDuration));
        isFading = false;
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
        isWaitingForChoice = false;
        ClearChoices();

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        if (dialogueIndex >= currentDialogue.lines.Length)
        {
            StartCoroutine(EndDialogueProcess());
            return;
        }

        DialogueLine line = currentDialogue.lines[dialogueIndex];
        if (portriat != null) portriat.sprite = line.speaker.portrait;
        if (actorName != null) actorName.text = line.speaker.actorName;

        typingCoroutine = StartCoroutine(TypeSentence(line.text));
        dialogueIndex++;
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        for (int i = 0; i < sentence.Length; i++)
        {
            if (sentence[i] == '<')
            {
                int closingTagIndex = sentence.IndexOf('>', i);
                if (closingTagIndex != -1)
                {
                    dialogueText.text += sentence.Substring(i, closingTagIndex - i + 1);
                    i = closingTagIndex;
                }
                else
                {
                    dialogueText.text += sentence[i];
                }
            }
            else
            {
                dialogueText.text += sentence[i];
                if (AudioManager.Instance != null && i % 2 == 0)
                {
                    AudioManager.Instance.PlayTypingSound();
                }
                yield return new WaitForSecondsRealtime(typingSpeed);
            }
        }
        typingCoroutine = null;
    }

    private void ShowChoices()
    {
        isWaitingForChoice = true;
        dialogueText.text = "";

        for (int i = 0; i < currentDialogue.options.Length; i++)
        {
            if (i < choiceButtons.Length)
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
        isWaitingForChoice = false;
        if (dialogueSO == null)
            StartCoroutine(EndDialogueProcess());
        else
            StartDialogue(dialogueSO);
    }

    private IEnumerator EndDialogueProcess()
    {
        if (!isDialogueActive) yield break;

        if (currentDialogue != null && currentDialogue.loreToUnlock != null && currentDialogue.loreToUnlock.Length > 0)
        {
            foreach (LoreSO lore in currentDialogue.loreToUnlock)
            {
                if (lore != null)
                {
                    CodexManager.instance.AddLoreEntry(lore);
                }
            }
        }

        if (currentDialogue != null && currentDialogue.itemReward != null)
        {
            InventoryManager.instance.AddItem(currentDialogue.itemReward, currentDialogue.itemRewardQuantity);
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

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(start, end, timer / duration);
            yield return new WaitForEndOfFrame();
        }
        cg.alpha = end;
        cg.interactable = (end > 0);
        cg.blocksRaycasts = (end > 0);
    }
}