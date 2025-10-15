using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "LoreDatabase", menuName = "BindingOfApi/Lore Database")] 
public class LoreDatabaseSO : ScriptableObject
{
    public List<LoreSO> allLore;
    public LoreSO GetLoreByID(string id)
    {
        return allLore.FirstOrDefault(l => l.loreID == id);
    }
}
