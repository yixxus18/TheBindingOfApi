using UnityEngine;

[System.Serializable]
public class LootTableEntry
{
    public ItemSO itemToDrop;
    [Range(0f, 1f)]
    public float dropChance;
}