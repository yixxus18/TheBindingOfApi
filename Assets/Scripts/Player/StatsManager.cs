using UnityEngine;
using System;

public class StatsManager : MonoBehaviour
{
    public static StatsManager instance;
    public static event Action OnStatsChanged;

    [Header("Player Stats")]
    public int maxHealth = 200;
    public int currentHealth;
    public int power = 10;
    public int speed = 5;
    public int engineering = 1;

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
        OnStatsChanged?.Invoke();
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnStatsChanged?.Invoke();
    }

    public void LevelUp()
    {
        maxHealth += 20;
        currentHealth = maxHealth;
        power += 2;
        engineering += 1;

        Debug.Log($"¡Level Up! Power: {power}, MaxHP: {maxHealth}");
        OnStatsChanged?.Invoke();
    }
}