using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;

[CreateAssetMenu(fileName = "New Room Puzzle", menuName = "BindingOfApi/Puzzle")]
public class RoomPuzzleSO : ScriptableObject
{
    public string puzzleID;
    public string puzzleName;
    [TextArea] public string descriptionHint;

    [Header("Solución Requerida")]
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
        Debug.Log($"<color=yellow>--- INICIANDO VALIDACIÓN DEL PUZZLE: {puzzleName} ---</color>");

        // 1. Validar Método
        Debug.Log($"[1] Validando Método: Esperado <b>'{requiredMethod}'</b> - Recibido <b>'{method}'</b>");
        if (method != requiredMethod)
        {
            Debug.LogError("❌ Falló el Mtodo.");
            return false;
        }

        // 2. Validar URL
        Debug.Log($"[2] Validando URL: Recibida <b>'{url}'</b>");
        foreach (var fragment in requiredUrlFragments)
        {
            if (!url.Contains(fragment))
            {
                Debug.LogError($"❌ Falló URL. No se encontró el fragmento requerido: <b>'{fragment}'</b> en la URL enviada.");
                return false;
            }
        }

        // 3. Validar Headers
        Debug.Log($"[3] Validando Headers. Cantidad recibida: {headers?.Count ?? 0}");
        foreach (var reqHeader in requiredHeaders)
        {
            string trimmedReq = reqHeader.Trim();
            bool headerFound = false;

            if (headers != null)
            {
                foreach (var h in headers)
                {
                    // Logueamos cada comparación para ver si hay espacios invisibles
                    // Debug.Log($"   Comparando: '{h.Trim()}' contra '{trimmedReq}'"); 
                    if (h.Trim() == trimmedReq)
                    {
                        headerFound = true;
                        break;
                    }
                }
            }

            if (!headerFound)
            {
                Debug.LogError($"❌ Falló Header. Falta el header requerido: <b>'{trimmedReq}'</b>");
                return false;
            }
        }

        // 4. Validar Body
        string cleanBody = CleanString(body);
        Debug.Log($"[4] Validando Body.");
        Debug.Log($"   Body Original Recibido: '{body}'");
        Debug.Log($"   Body Limpio (sin espacios/enters): '{cleanBody}'");

        foreach (var snippet in requiredBodySnippets)
        {
            if (string.IsNullOrEmpty(snippet)) continue;

            string cleanSnippet = CleanString(snippet);
            Debug.Log($"   Buscando Snippet Limpio: '{cleanSnippet}'");

            if (!cleanBody.Contains(cleanSnippet))
            {
                Debug.LogError($"❌ Falló Body. El body limpio no contiene: <b>'{cleanSnippet}'</b>");
                return false;
            }
        }

        Debug.Log("<color=green>✅ ¡VALIDACIÓN EXITOSA! Puzzle Resuelto.</color>");
        return true;
    }

    private string CleanString(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        // Eliminamos espacios, saltos de línea, retornos de carro y tabulaciones
        return input.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "");
    }
}