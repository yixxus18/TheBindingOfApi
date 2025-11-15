using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

[CreateAssetMenu(fileName = "New Room Puzzle", menuName = "BindingOfApi/Puzzle")]
public class RoomPuzzleSO : ScriptableObject
{
    public string puzzleID;
    public string puzzleName;
    [TextArea] public string descriptionHint;

    [Header("Soluci�n Requerida")]
    public string requiredMethod;
    public List<string> requiredUrlFragments;
    public List<string> requiredHeaders;
    public List<string> requiredBodySnippets;

    [Header("Resultados")]
    [TextArea] public string successResponse;
    public string failureErrorCode;
    public UnityEvent OnPuzzleSolved;

    [Header("Recompensas Adicionales")]
    public ItemSO itemReward;
    public LoreSO loreReward;

    public bool ValidateRequest(string method, string url, List<string> headers, string body)
    {
        if (method != requiredMethod) return false;

        foreach (var fragment in requiredUrlFragments)
        {
            if (!url.Contains(fragment)) return false;
        }

        foreach (var reqHeader in requiredHeaders)
        {
            if (headers == null || !headers.Any(h => h.Trim() == reqHeader.Trim())) return false;
        }

        foreach (var snippet in requiredBodySnippets)
        {
            if (string.IsNullOrEmpty(snippet)) continue;
            if (!body.Contains(snippet)) return false;
        }

        return true;
    }
}