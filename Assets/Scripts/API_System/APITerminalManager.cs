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

    [Header("Input Fields (Menu Terminal)")]
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

    private void OnSendRequest()
    {
        string method, fullUrl;
        List<string> headers = new List<string>();
        string body = "";

        if (isCentralTerminal)
        {
            method = methodSlot.GetCurrentItem()?.apiValue ?? "GET";
            fullUrl = baseUrl + (urlSlot.GetCurrentItem()?.apiValue ?? "");
            if (headerSlot.GetCurrentItem() != null) headers.Add(headerSlot.GetCurrentItem().apiValue);
            body = bodySlot.GetCurrentItem()?.apiValue ?? "";
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
            ShowResponse($"<color=green>200 OK</color>\n{currentRoomPuzzle.successResponse}");
            currentRoomPuzzle.OnPuzzleSolved?.Invoke();
            if (DungeonObjectiveManager.instance != null)
                DungeonObjectiveManager.instance.NotifyProgress(ObjectiveType.SolvePuzzle, currentRoomPuzzle.puzzleName);

            if (isCentralTerminal && CodexManager.instance != null)
            {
                CodexManager.instance.AddRequestEntry(currentRoomPuzzle.puzzleName, method, url.Replace(baseUrl, ""));
            }
        }
        else
        {
            ShowResponse($"<color=red>{currentRoomPuzzle.failureErrorCode} Error</color>");
        }
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
            methodInputField.text = "";
            urlInputField.text = "";
            headersInputField.text = "";
            bodyInputField.text = "";
        }
        responseText.text = "Esperando Petición...";
    }

    private void ShowResponse(string response)
    {
        responseText.text = response;
    }
}