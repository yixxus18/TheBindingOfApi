using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject miniMenuContainer;
    public GameObject menuButtonsPanel; 
    public Button toggleMenuButton; 
    private bool isMenuOpen = false;

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

        miniMenuContainer.SetActive(false);
        menuButtonsPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleMiniMenu();
        }
    }

    public void ToggleMiniMenu()
    {
        isMenuOpen = !isMenuOpen;
        miniMenuContainer.SetActive(isMenuOpen);
        menuButtonsPanel.SetActive(isMenuOpen);

        Time.timeScale = isMenuOpen ? 0 : 1;
        InventorySlot.isTerminalActive = isMenuOpen;

        if (isMenuOpen)
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
