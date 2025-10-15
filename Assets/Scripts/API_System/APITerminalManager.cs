using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ApiTerminalManager : MonoBehaviour
{
    public static ApiTerminalManager instance;

    [Header("UI Construction Slots")]
    public TMP_Text methodText;
    public TMP_InputField urlInputField;
    public TMP_InputField headersInputField;
    public TMP_InputField bodyInputField;
    public TMP_Text responseOutputText;

    private string currentMethod;
    private List<string> currentHeaders = new List<string>();

    public RoomPuzzleSO currentRoomPuzzle;

    private void Awake() { instance = this; }
    private void OnEnable() { InventorySlot.OnItemSentToTerminal += AddItemToTerminal; }
    private void OnDisable() { InventorySlot.OnItemSentToTerminal -= AddItemToTerminal; }

    private void AddItemToTerminal(ItemSO item)
    {
        switch (item.itemType)
        {
            case ApiItemType.Method:
                currentMethod = item.apiValue;
                methodText.text = currentMethod;
                break;
            case ApiItemType.Fragment:
                urlInputField.text += item.apiValue;
                break;
            case ApiItemType.Header:
            case ApiItemType.Token:
                if (!currentHeaders.Contains(item.apiValue))
                {
                    currentHeaders.Add(item.apiValue);
                    headersInputField.text += item.apiValue + "\n";
                }
                break;
            case ApiItemType.Body:
                bodyInputField.text += item.apiValue;
                break;
        }
    }

    public void OnSendRequest()
    {
        if (currentRoomPuzzle == null) return;
        string fullUrl = "https://www.conectop.com" + urlInputField.text;
        string body = bodyInputField.text;

        bool isSuccess = currentRoomPuzzle.ValidateRequest(currentMethod, fullUrl,
currentHeaders, body);

        if (isSuccess)
        {
            responseOutputText.text = $"<color=green>200 OK </ color >\n{ currentRoomPuzzle.successResponse}"; 
            currentRoomPuzzle.OnPuzzleSolved?.Invoke();
            DungeonObjectiveManager.instance.NotifyProgress(ObjectiveType.SolvePuzzle,currentRoomPuzzle.puzzleName);
            CodexManager.instance.AddRequestEntry(currentRoomPuzzle.puzzleName,currentMethod, urlInputField.text);
        }
        else
        {
            responseOutputText.text = $"<color=red>{currentRoomPuzzle.failureErrorCode} Error </ color > "; 
        }
    }

    public void ClearTerminal()
    {
        currentMethod = "";
        methodText.text = "METHOD";
        urlInputField.text = "";
        headersInputField.text = "";
        bodyInputField.text = "";
        currentHeaders.Clear();
        responseOutputText.text = "Esperando Petición...";
    }
}