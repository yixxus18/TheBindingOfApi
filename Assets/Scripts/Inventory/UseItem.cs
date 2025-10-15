using UnityEngine;

public static class UseItem
{
    public static void Apply(ItemSO item)
    {
        if (item.itemType != ApiItemType.Consumable) return;

        if (item.healAmount > 0)
        {
            StatsManager.instance.Heal(item.healAmount);
        }
    }
}