using UnityEngine;

public class LoreUnlocker : MonoBehaviour
{
    public enum UnlockMethod
    {
        OnTriggerEnter,
        OnInteractKeyPress
    }

    [Header("Configuración del Lore")]
    public LoreSO loreToDiscover;
    public UnlockMethod unlockMethod = UnlockMethod.OnTriggerEnter;

    [Header("Opciones")]
    public bool destroyAfterUnlock = true;
    public GameObject interactPrompt;

    private bool playerInRange = false;

    private void Awake()
    {
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (unlockMethod == UnlockMethod.OnInteractKeyPress && playerInRange)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                UnlockLore();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (unlockMethod == UnlockMethod.OnTriggerEnter)
            {
                UnlockLore();
            }
            else
            {
                playerInRange = true;
                if (interactPrompt != null)
                {
                    interactPrompt.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (unlockMethod == UnlockMethod.OnInteractKeyPress)
            {
                playerInRange = false;
                if (interactPrompt != null)
                {
                    interactPrompt.SetActive(false);
                }
            }
        }
    }

    private void UnlockLore()
    {
        if (loreToDiscover == null || CodexManager.instance == null)
        {
            Debug.LogWarning("LoreSO o CodexManager no está asignado.", this.gameObject);
            return;
        }
        CodexManager.instance.AddLoreEntry(loreToDiscover);
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
        if (destroyAfterUnlock)
        {
            Destroy(gameObject);
        }
        else
        {
            this.enabled = false;
        }
    }
}