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

    [Header("Manager References")]
    [Tooltip("Arrastra aquí el GameObject que tiene el ApiTerminalManager del menú de pausa.")]
    public ApiTerminalManager menuTerminalManager;

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
        if (GameInput.Instance.GetToggleMenuPressed())
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
        else
        {
            if (menuTerminalManager != null && menuTerminalManager.terminalPanel.activeSelf)
            {
                menuTerminalManager.CloseTerminal();
            }
            if (TooltipManager.instance != null)
            {
                TooltipManager.instance.HideTooltip();
            }
        }
    }

    public void OpenInventoryTab() => OpenTab(inventoryTab);

    public void OpenTerminalTab()
    {
        OpenTab(terminalTab);
        if (menuTerminalManager != null)
        {
            menuTerminalManager.OpenTerminal();
        }
    }

    public void OpenObjectivesTab() => OpenTab(objectivesTab);

    public void OpenCodexTab()
    {
        OpenTab(codexTab);
        if (CodexManager.instance != null)
        {
            CodexManager.instance.UpdateCodexUI();
        }
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