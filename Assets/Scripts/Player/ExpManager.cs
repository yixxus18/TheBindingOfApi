using UnityEngine;

public class ExpManager : MonoBehaviour
{
    public static ExpManager instance;

    [Header("Experience")]
    public int level = 1;
    public int currentExp = 0;
    public int expToLevel = 100;
    public float expGrowthMultiplier = 1.5f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        EnemyController.OnMonsterDefeated += GainExperience;
    }

    void OnDisable()
    {
        EnemyController.OnMonsterDefeated -= GainExperience;
    }

    public void GainExperience(int amount)
    {
        currentExp += amount;
        while (currentExp >= expToLevel)
        {
            LevelUp();
        }

        if (PlayerUIManager.instance != null)
        {
            PlayerUIManager.instance.UpdateExpBar();
        }
    }

    void LevelUp()
    {
        currentExp -= expToLevel;
        level++;
        expToLevel = Mathf.RoundToInt(expToLevel * expGrowthMultiplier);
        if (StatsManager.instance != null)
        {
            StatsManager.instance.maxHealth += 20;
            StatsManager.instance.currentHealth = StatsManager.instance.maxHealth;
            StatsManager.instance.power += 2;
            StatsManager.instance.speed += 1;
        }
        if (PlayerUIManager.instance != null)
        {
            PlayerUIManager.instance.UpdateExpBar();
            PlayerUIManager.instance.UpdateHealthBar();
            PlayerUIManager.instance.UpdateStatsDisplay();
        }
    }
}
