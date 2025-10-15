using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class RequestEntry
{
    public string puzzleName;
    public string fullRequest;
}

public class CodexManager : MonoBehaviour
{
    public static CodexManager instance;

    public List<RequestEntry> learnedRequests = new List<RequestEntry>();
    public List<LoreSO> discoveredLore = new List<LoreSO>();

    [Header("UI References")]
    public Transform requestLogContainer;
    public Transform loreListContainer;
    public GameObject codexEntryPrefab; 

    private void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
    }

    public void AddRequestEntry(string puzzleName, string method, string url)
    {
        if (learnedRequests.Any(r => r.puzzleName == puzzleName)) return;
        learnedRequests.Add(new RequestEntry
        {
            puzzleName = puzzleName,
            fullRequest =$"{method} {url}"
        });
    }

    public void AddLoreEntry(LoreSO lore)
    {
        if (discoveredLore.Any(l => l.loreID == lore.loreID)) return;
        discoveredLore.Add(lore);
    }

    public void UpdateCodexUI()
    {

        foreach (Transform child in requestLogContainer) Destroy(child.gameObject);
        foreach (Transform child in loreListContainer) Destroy(child.gameObject);

        foreach (var entry in learnedRequests)
        {
            GameObject go = Instantiate(codexEntryPrefab, requestLogContainer);
            go.GetComponent<TMPro.TMP_Text>().text = $"<b>{entry.puzzleName}:</b> { entry.fullRequest}"; 
        }
        foreach (var lore in discoveredLore)
        {
            GameObject go = Instantiate(codexEntryPrefab, loreListContainer);
            go.GetComponent<TMPro.TMP_Text>().text = lore.title;
        }
    }
}