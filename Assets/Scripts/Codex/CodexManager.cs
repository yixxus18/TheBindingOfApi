using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

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

    [Header("Lore Detail View")]
    public GameObject loreDetailPanel;
    public TextMeshProUGUI loreTitleText;
    public TextMeshProUGUI loreDescriptionText;

    private LoreSO currentlyDisplayedLore;

    private void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
    }

    public void OpenCodexTab()
    {
        UpdateCodexUI();
        if (loreDetailPanel != null)
        {
            loreDetailPanel.SetActive(false);
            currentlyDisplayedLore = null;
        }
    }

    public void AddRequestEntry(string puzzleName, string method, string url)
    {
        if (learnedRequests.Any(r => r.puzzleName == puzzleName)) return;
        learnedRequests.Add(new RequestEntry { puzzleName = puzzleName, fullRequest = $"{method} {url}" });

        if (GameManager.Instance != null)
        {
            SaveSystem.SaveGame(this, GameManager.Instance.statsManager, GameManager.Instance.inventoryManager);
            Debug.Log("Nueva petición aprendida y progreso guardado.");
        }
    }

    public void AddLoreEntry(LoreSO lore)
    {
        if (lore != null && !discoveredLore.Contains(lore))
        {
            discoveredLore.Add(lore);
            if (DungeonObjectiveManager.instance != null)
            {
                DungeonObjectiveManager.instance.NotifyProgress(ObjectiveType.CollectItem, lore.loreID);
            }
        }
    }

    public void UpdateCodexUI()
    {
        foreach (Transform child in requestLogContainer) Destroy(child.gameObject);
        foreach (Transform child in loreListContainer) Destroy(child.gameObject);

        foreach (var entry in learnedRequests)
        {
            GameObject go = Instantiate(codexEntryPrefab, requestLogContainer);
            go.GetComponent<CodexEntryUI>().SetupRequestEntry(entry.puzzleName, entry.fullRequest);
        }

        foreach (var lore in discoveredLore)
        {
            GameObject go = Instantiate(codexEntryPrefab, loreListContainer);
            CodexEntryUI ui = go.GetComponent<CodexEntryUI>();
            if (ui != null)
            {
                ui.SetupLoreEntry(lore);
            }
        }
    }

    public void ShowLoreDetails(LoreSO lore)
    {
        if (lore == null || loreDetailPanel == null) return;

        if (loreDetailPanel.activeSelf && currentlyDisplayedLore == lore)
        {
            loreDetailPanel.SetActive(false);
            currentlyDisplayedLore = null;
        }
        else
        {
            loreTitleText.text = lore.title;
            loreDescriptionText.text = lore.content;
            currentlyDisplayedLore = lore;
            loreDetailPanel.SetActive(true);
        }
    }
}