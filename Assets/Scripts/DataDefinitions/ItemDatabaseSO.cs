using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "BindingOfApi/Item Database")]
public class ItemDatabaseSO : ScriptableObject
{
    public List<ItemSO> allItems;

    public ItemSO GetItemByID(int id)
    {
        return allItems.FirstOrDefault(item => item.itemID == id);
    }
}