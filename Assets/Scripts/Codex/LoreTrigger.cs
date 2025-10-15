using UnityEngine;

public class LoreTrigger : MonoBehaviour
{
    public LoreSO loreToDiscover;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CodexManager.instance.AddLoreEntry(loreToDiscover);
            Destroy(gameObject);
        }
    }
}