using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ApiTerminalManager : MonoBehaviour
{
    [Header("Terminal Type")]
    public bool isCentralTerminal;

    [Header("UI References")]
    public GameObject terminalPanel;
    public TMP_Text responseText;
    public Button sendButton;
    public Button clearButton;
    public Button closeButton;

    [Header("Drop Slots (Central Terminal)")]
    public TerminalDropSlot methodSlot;
    public TerminalDropSlot urlSlot;
    public TerminalDropSlot headerSlot;
    public TerminalDropSlot bodySlot;

    [Header("Menu Terminal Elements")]
    public TMP_Dropdown learnedRequestsDropdown;
    public TMP_InputField methodInputField;
    public TMP_InputField urlInputField;
    public TMP_InputField headersInputField;
    public TMP_InputField bodyInputField;

    [HideInInspector]
    public RoomPuzzleSO currentRoomPuzzle;

    private readonly string baseUrl = "https://www.conectop.com";

    private void Start()
    {
        sendButton?.onClick.AddListener(OnSendRequest);
        clearButton?.onClick.AddListener(ClearTerminal);
        closeButton?.onClick.AddListener(CloseTerminal);

        if (learnedRequestsDropdown != null)
        {
            learnedRequestsDropdown.onValueChanged.AddListener(OnLearnedRequestSelected);
        }

        RegisterAllButtonSounds();
    }

    private void RegisterAllButtonSounds()
    {
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        foreach (Button btn in allButtons)
        {
            btn.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.menuNavigationSound);
                }
            });
        }
    }

    private void OnDestroy()
    {
        if (isCentralTerminal)
        {
            if (TerminalActivator.terminalInstance == gameObject.transform.root.gameObject)
            {
                TerminalActivator.terminalInstance = null;
            }
        }
    }

    public void OpenTerminal(RoomPuzzleSO puzzleContext = null)
    {
        currentRoomPuzzle = puzzleContext;
        terminalPanel.SetActive(true);
        ClearTerminal();

        if (!isCentralTerminal)
        {
            PopulateLearnedRequests();
        }
    }

    public void CloseTerminal()
    {
        if (isCentralTerminal)
        {
            ClearTerminal();
            Destroy(gameObject.transform.root.gameObject);
        }
        else
        {
            terminalPanel.SetActive(false);
        }
        currentRoomPuzzle = null;
    }

    private void PopulateLearnedRequests()
    {
        if (learnedRequestsDropdown == null) return;

        learnedRequestsDropdown.ClearOptions();
        List<string> options = new List<string> { "Selecciona una petición aprendida..." };

        if (CodexManager.instance != null)
        {
            options.AddRange(CodexManager.instance.learnedRequests.Select(r => r.puzzleName));
        }

        learnedRequestsDropdown.AddOptions(options);
        learnedRequestsDropdown.value = 0;
    }

    private void OnLearnedRequestSelected(int index)
    {
        if (index == 0 || CodexManager.instance == null)
        {
            return;
        }

        RequestEntry request = CodexManager.instance.learnedRequests[index - 1];

        string[] parts = request.fullRequest.Split(' ');
        if (parts.Length >= 2)
        {
            if (methodInputField != null) methodInputField.text = parts[0];
            if (urlInputField != null) urlInputField.text = parts[1];

            if (headersInputField != null) headersInputField.text = "";
            if (bodyInputField != null) bodyInputField.text = "";
        }
    }

    private void OnSendRequest()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.processingRequestSound);

        string method, fullUrl;
        List<string> headers = new List<string>();
        string body = "";

        if (isCentralTerminal)
        {
            method = methodSlot.GetCurrentItem()?.apiValue ?? "GET";
            string urlFragments = string.Join("", urlSlot.GetCurrentItems().Select(i => i.apiValue));
            fullUrl = baseUrl + urlFragments;
            headers = headerSlot.GetCurrentItems().Select(i => i.apiValue).ToList();
            body = string.Join("", bodySlot.GetCurrentItems().Select(i => i.apiValue));
        }
        else
        {
            method = methodInputField.text;
            fullUrl = baseUrl + urlInputField.text;
            headers = headersInputField.text.Split('\n').ToList();
            body = bodyInputField.text;
        }

        if (currentRoomPuzzle != null)
        {
            ValidatePuzzleRequest(method, fullUrl, headers, body);
        }
        else
        {
            ShowResponse($"<color=cyan>Simulación:</color> {method} {fullUrl}\n<color=green>200 OK</color>");
        }
    }

    private void ValidatePuzzleRequest(string method, string url, List<string> headers, string body)
    {
        bool isSuccess = currentRoomPuzzle.ValidateRequest(method, url, headers, body);
        if (isSuccess)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.terminalSuccessSound);

            ShowResponse($"<color=green>200 OK</color>\n{currentRoomPuzzle.successResponse}");
            currentRoomPuzzle.OnPuzzleSolved?.Invoke();

            ConfirmItemsUsed();

            if (DungeonObjectiveManager.instance != null)
                DungeonObjectiveManager.instance.NotifyProgress(ObjectiveType.SolvePuzzle, currentRoomPuzzle.puzzleID);

            if (isCentralTerminal && CodexManager.instance != null)
            {
                CodexManager.instance.AddRequestEntry(currentRoomPuzzle.puzzleName, method, url.Replace(baseUrl, ""));
            }
        }
        else
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.terminalErrorSound);

            ShowResponse($"<color=red>{currentRoomPuzzle.failureErrorCode} Error</color>");
        }
    }

    private void ConfirmItemsUsed()
    {
        if (!isCentralTerminal) return;

        methodSlot?.ConsumeItem();
        urlSlot?.ConsumeItem();
        headerSlot?.ConsumeItem();
        bodySlot?.ConsumeItem();
    }

    public void ClearTerminal()
    {
        if (isCentralTerminal)
        {
            methodSlot?.ClearSlot();
            urlSlot?.ClearSlot();
            headerSlot?.ClearSlot();
            bodySlot?.ClearSlot();
        }
        else
        {
            if (learnedRequestsDropdown != null) learnedRequestsDropdown.value = 0;
            if (methodInputField != null) methodInputField.text = "";
            if (urlInputField != null) urlInputField.text = "";
            if (headersInputField != null) headersInputField.text = "";
            if (bodyInputField != null) bodyInputField.text = "";
        }
        responseText.text = "Esperando Petición...";
    }

    private void ShowResponse(string response)
    {
        responseText.text = response;
    }
}