using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager instance;

    [Header("Health Bar")]
    public Slider healthSlider;
    public TMP_Text healthText;
    public Image healthFillImage;
    public Gradient healthGradient;

    [Header("Experience Bar")]
    public Slider expSlider;
    public TMP_Text levelText;
    public TMP_Text expText;

    [Header("Stats Display")]
    public TMP_Text powerText;
    public TMP_Text speedText;
    public TMP_Text engineeringText;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        StatsManager.OnStatsChanged += UpdateAllUI;
    }

    private void OnDisable()
    {
        StatsManager.OnStatsChanged -= UpdateAllUI;
    }

    void Start()
    {
        UpdateAllUI();
    }
    private void UpdateAllUI()
    {
        UpdateHealthBar();
        UpdateStatsDisplay();
    }

    public void UpdateHealthBar()
    {
        if (StatsManager.instance == null) return;
        int current = StatsManager.instance.currentHealth;
        int max = StatsManager.instance.maxHealth;
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
        if (healthText != null)
        {
            healthText.text = $"{current}/{max}";
        }
        if (healthFillImage != null && healthGradient != null)
        {
            float normalizedHealth = (float)current / max;
            healthFillImage.color = healthGradient.Evaluate(normalizedHealth);
        }
    }

    public void UpdateExpBar()
    {
        if (ExpManager.instance == null) return;
        int current = ExpManager.instance.currentExp;
        int needed = ExpManager.instance.expToLevel;
        int level = ExpManager.instance.level;
        if (expSlider != null)
        {
            expSlider.maxValue = needed;
            expSlider.value = current;
        }
        if (levelText != null)
        {
            levelText.text = $"Nivel{level}";
        }
        if (expText != null)
        {
            expText.text = $"{current}/{needed}";
        }
    }

    public void UpdateStatsDisplay()
    {
        if (StatsManager.instance == null) return;
        if (powerText != null)
            powerText.text = $"Power: {StatsManager.instance.power}";
        if (speedText != null)
            speedText.text = $"Speed: {StatsManager.instance.speed}";
        if (engineeringText != null)
            engineeringText.text = $"Engineering: {StatsManager.instance.engineering}";
    }
}