using UnityEngine;
using System.Collections.Generic;

public class TerminalActivator : MonoBehaviour
{
    public GameObject centralTerminalPrefab;
    public List<RoomPuzzleSO> puzzleContexts;

    private bool playerInRange;
    public static GameObject terminalInstance;

    private void OnDestroy()
    {
        if (terminalInstance != null)
        {
            Destroy(terminalInstance);
            terminalInstance = null;
        }
    }

    private void Update()
    {
        if (playerInRange && GameInput.Instance.GetInteractPressed())
        {
            if (terminalInstance == null)
            {
                terminalInstance = Instantiate(centralTerminalPrefab);

                Transform panel = terminalInstance.transform.Find("Panel");
                if (panel != null)
                {
                    panel.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogError("El prefab de la terminal no tiene un hijo llamado 'Panel'.");
                }

                ApiTerminalManager terminalManager = terminalInstance.GetComponentInChildren<ApiTerminalManager>();
                if (terminalManager != null)
                {
                    terminalManager.OpenTerminal(puzzleContexts);
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