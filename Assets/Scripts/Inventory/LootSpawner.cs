// Nuevo script: LootSpawner.cs
using UnityEngine;

public class LootSpawner : MonoBehaviour
{
    public static LootSpawner Instance { get; private set; }

    [Header("Loot Prefab")]
    [Tooltip("Arrastra aquí tu prefab de Loot desde la carpeta Assets/Prefabs")]
    public GameObject lootPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnLoot(ItemSO item, int amount, Vector3 position)
    {
        if (lootPrefab == null)
        {
            Debug.LogError("El prefab de Loot no está asignado en el LootSpawner!");
            return;
        }

        GameObject lootObj = Instantiate(lootPrefab, position, Quaternion.identity);
        Loot lootComponent = lootObj.GetComponent<Loot>();
        if (lootComponent != null)
        {
            lootComponent.Initialize(item, amount);
        }
    }
}