using UnityEngine;

public class TerminalActivator : MonoBehaviour
{
    public GameObject centralTerminalPrefab;
    public RoomPuzzleSO puzzleContext;

    private bool playerInRange;
    public static GameObject terminalInstance;

    private void Update()
    {
        if (playerInRange && GameInput.Instance.GetInteractPressed())
        {
            if (terminalInstance == null)
            {
                terminalInstance = Instantiate(centralTerminalPrefab);

                ApiTerminalManager terminalManager = terminalInstance.GetComponentInChildren<ApiTerminalManager>();
                if (terminalManager != null)
                {
                    terminalManager.OpenTerminal(puzzleContext);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}