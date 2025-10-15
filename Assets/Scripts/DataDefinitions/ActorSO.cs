using UnityEngine;

[CreateAssetMenu(fileName = "ActorSO", menuName = "BindingOfApi/Actor")]
public class ActorSO : ScriptableObject
{
    public string actorName;
    public Sprite portrait;
}