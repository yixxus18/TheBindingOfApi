using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;

[CreateAssetMenu(fileName = "New Room Puzzle", menuName = "BindingOfApi/Puzzle")]
public class RoomPuzzleSO : ScriptableObject
{
    public int puzzleID; // Cambiado a int para coincidir con tu sistema nuevo
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

        Debug.Log($"[Puzzle Check] Validando Puzzle: {puzzleName}");
        Debug.Log($"[Puzzle Check] Método Recibido: '{method}' | Esperado: '{requiredMethod}'");
        Debug.Log($"[Puzzle Check] URL Recibida: '{url}'");
        // 1. Validar Método
        if (method != requiredMethod)
        {
            Debug.LogWarning("[Puzzle Check] Falló el método.");
            return false;
        }

        // 2. Validar URL (Limpiando espacios para evitar errores tontos)
        string cleanUrl = CleanString(url);
        foreach (var fragment in requiredUrlFragments)
        {
            if (!url.Contains(fragment))
            {
                Debug.LogWarning($"[Puzzle Check] Falló URL. Falta el fragmento: '{fragment}'");
                return false;
            }
            string cleanFragment = CleanString(fragment);
            if (!cleanUrl.Contains(cleanFragment)) return false;
        }

        // 3. Validar Headers (Limpiando espacios y saltos de línea)
        foreach (var reqHeader in requiredHeaders)
        {
            string cleanReqHeader = CleanString(reqHeader);
            bool headerFound = false;

            if (headers != null)
            {
                foreach (var h in headers)
                {
                    // Comparamos versiones "limpias" de ambos lados
                    if (CleanString(h) == cleanReqHeader)
                    {
                        headerFound = true;
                        break;
                    }
                }
            }

            if (!headerFound) return false;
        }

        // 4. Validar Body (Limpiando espacios y saltos de línea)
        string cleanBody = CleanString(body);
        foreach (var snippet in requiredBodySnippets)
        {
            if (string.IsNullOrEmpty(snippet)) continue;

            string cleanSnippet = CleanString(snippet);
            if (!cleanBody.Contains(cleanSnippet)) return false;
        }

        return true;
    }

    // Función mágica que elimina espacios, enters y tabulaciones
    private string CleanString(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        return input.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "");
    }
}