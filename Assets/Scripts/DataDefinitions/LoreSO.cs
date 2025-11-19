using UnityEngine;

[CreateAssetMenu(fileName = "New Lore", menuName = "BindingOfApi/Lore")]
public class LoreSO : ScriptableObject
{
    public int loreID;
    public string title;
    [TextArea(10, 20)]
    public string content;
}