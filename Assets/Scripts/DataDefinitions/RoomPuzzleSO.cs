using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Room Puzzle", menuName = "BindingOfApi/Puzzle")]
public class RoomPuzzleSO : ScriptableObject
{
    public string puzzleName;
    [TextArea] public string descriptionHint;

    [Header("Solución Requerida")]
    public string requiredMethod;
    public string requiredUrlFragment;
    public List<string> requiredHeaders;
    public string requiredBodySnippet;

    [Header("Resultados")]
    [TextArea] public string successResponse;
    public string failureErrorCode;
    public UnityEvent OnPuzzleSolved;

    public bool ValidateRequest(string method, string url, List<string> headers, string body)
    {
        if (method != requiredMethod) return false;
        if (!url.Contains(requiredUrlFragment)) return false;
        if (!string.IsNullOrEmpty(requiredBodySnippet) && !body.Contains(requiredBodySnippet)) return false;
        foreach (var reqHeader in requiredHeaders)
        {
            if (headers == null || !headers.Contains(reqHeader)) return false;
        }
        return true;
    }
}