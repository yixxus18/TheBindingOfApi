using UnityEngine;

[CreateAssetMenu(fileName = "New Lore", menuName = "BindingOfApi/Lore")]
public class LoreSO : ScriptableObject
{
    public string loreID; // ID único, ej: "CONCEPT_API_BASICS" 
    public string title;
    [TextArea(10, 20)]
    public string content;
}