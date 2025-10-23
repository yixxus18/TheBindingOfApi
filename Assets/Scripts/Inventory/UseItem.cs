using UnityEngine;
public static class ItemEffects
{
    public static void Apply(ItemSO item)
    {
        if (item == null || item.itemType != ApiItemType.Consumable)
            return;
        if (item.healAmount > 0)
        {
            StatsManager.instance.Heal(item.healAmount);
            Debug.Log($"Usado {item.itemName}: +{item.healAmount} HP");
        }

    }
}