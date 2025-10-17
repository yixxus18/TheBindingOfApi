using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager instance;

    [Header("Player Stats")]
    public int maxHealth = 200;
    public int currentHealth;
    public int power = 10;       // Daño base
    public int speed = 5;        // Velocidad de movimiento
    public int engineering = 1;  // Bonos a puzzles

    [Header("Combat Stats (Opcionales)")]
    public float weaponRange = 1.5f;
    public float knockbackForce = 5f;
    public float knockbackTime = 0.2f;
    public float stunTime = 0.3f;



    private void Awake()
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

        currentHealth = maxHealth;
    }
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Jugador derrotado");
        }
    }
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }
    public void LevelUp()
    {
        maxHealth += 20;
        currentHealth = maxHealth;
        power += 2;
        engineering += 1;

        Debug.Log($"¡Level Up! Power: {power}, MaxHP: {maxHealth}");
    }
}
