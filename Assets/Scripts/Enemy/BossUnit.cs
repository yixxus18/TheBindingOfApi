using UnityEngine;

public class BossUnit : MonoBehaviour
{
    public int bossID;

    private bool isDead = false;
    private EnemyController enemyController;
    private SpikedSlimeController slimeController;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        slimeController = GetComponent<SpikedSlimeController>();
    }

    private void Update()
    {
        if (isDead) return;

        if (enemyController != null && enemyController.currentHealth <= 0)
        {
            HandleDeath();
        }
        else if (slimeController != null && slimeController.currentHealth <= 0)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        isDead = true;
        Debug.Log($"Boss with ID {bossID} has been defeated. isDead: {isDead}");
        if (DungeonObjectiveManager.instance != null)
        {
            DungeonObjectiveManager.instance.NotifyProgress(ObjectiveType.KillBoss, bossID.ToString());
        }
    }
}