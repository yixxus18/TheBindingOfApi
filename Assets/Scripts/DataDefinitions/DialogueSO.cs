using UnityEngine;

[System.Serializable]
public struct DialogueLine
{
    public ActorSO speaker;
    [TextArea(3, 5)]
    public string text;
}

[System.Serializable]
public struct DialogueOption
{
    public string optiontext;
    public DialogueSO nextDialogue;
}

[CreateAssetMenu(fileName = "New Dialogue", menuName = "BindingOfApi/Dialogue")]
public class DialogueSO : ScriptableObject
{
    [Header("Contenido")]
    public DialogueLine[] lines;
    public DialogueOption[] options;

    [Header("Recompensa de Lore")]
    public LoreSO loreToUnlock;
}