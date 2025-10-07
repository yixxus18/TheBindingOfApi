using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Door", menuName = "Scriptable Objects/Door")]
public class DoorScriptable : ScriptableObject
{
    public RoomType roomType;
    public Sprite horizontalDoor;
}
