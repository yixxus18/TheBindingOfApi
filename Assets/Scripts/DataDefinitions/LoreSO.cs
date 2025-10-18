// LoreSO.cs (Sin cambios)
using UnityEngine;

[CreateAssetMenu(fileName = "New Lore", menuName = "BindingOfApi/Lore")]
public class LoreSO : ScriptableObject
{
    public string loreID;
    public string title;
    [TextArea(10, 20)]
    public string content;
}