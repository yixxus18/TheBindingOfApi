using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodexEntryUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI entryText;
    public Button button;

    private LoreSO associatedLore;

    private void Start()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnEntryClicked);
        }
        else
        {
            Debug.LogError("¡Button es NULL!");
        }
    }

    public void SetupLoreEntry(LoreSO lore)
    {
        this.associatedLore = lore;
        entryText.text = lore.title;
    }

    public void SetupRequestEntry(string puzzleName, string fullRequest)
    {
        this.associatedLore = null;
        entryText.text = $"<b>{puzzleName}:</b> {fullRequest}";
    }

    public void OnEntryClicked()
    {
        if (associatedLore != null)
        {
            if (CodexManager.instance != null)
            {
                Debug.Log("Llamando a ShowLoreDetails...");
                CodexManager.instance.ShowLoreDetails(associatedLore);
            }
            else
            {
                Debug.LogError("CodexManager.instance es NULL");
            }
        }
        else
        {
            Debug.LogWarning("No hay lore asociado (probablemente es una request)");
        }
    }
}
