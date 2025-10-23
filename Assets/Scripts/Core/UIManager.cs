using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static bool isGamePaused = false;

    [Header("Main Menu")]
    public GameObject miniMenuContainer;
    public GameObject menuButtonsPanel;
    public Button toggleMenuButton;

    [Header("Tabs")]
    public GameObject inventoryTab;
    public GameObject terminalTab;
    public GameObject codexTab;
    public GameObject objectivesTab;

    void Start()
    {
        if (toggleMenuButton != null)
        {
            toggleMenuButton.onClick.AddListener(ToggleMiniMenu);
        }
        isGamePaused = false;
        miniMenuContainer.SetActive(false);
        menuButtonsPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.C))
        {
            ToggleMiniMenu();
        }
    }

    public void ToggleMiniMenu()
    {
        isGamePaused = !isGamePaused;
        miniMenuContainer.SetActive(isGamePaused);
        menuButtonsPanel.SetActive(isGamePaused);
        InventorySlot.isTerminalActive = isGamePaused;

        if (isGamePaused)
        {
            OpenTab(inventoryTab);
        }
        else if (ApiTerminalManager.instance != null)
        {
            ApiTerminalManager.instance.ClearTerminal();
        }
    }

    public void OpenInventoryTab() => OpenTab(inventoryTab);
    public void OpenTerminalTab() => OpenTab(terminalTab);
    public void OpenObjectivesTab() => OpenTab(objectivesTab);
    public void OpenCodexTab()
    {
        OpenTab(codexTab);
        CodexManager.instance.UpdateCodexUI();
    }

    private void OpenTab(GameObject tabToOpen)
    {
        inventoryTab.SetActive(false);
        terminalTab.SetActive(false);
        codexTab.SetActive(false);
        objectivesTab.SetActive(false);
        tabToOpen.SetActive(true);
    }
}