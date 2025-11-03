using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;

public enum RoomType
{
    Regular,        // Salas normales con enemigos
    Item,          // Sala con loot/cofres
    Shop,          // Sala de tienda
    Boss,          // Sala del jefe final
    Secret,        // Sala secreta
    Puzzle         // Sala con rompecabezas de petici�n HTTP
}

public enum RoomShape
{
    OneByOne,
    OneByTwo,
    TwoByOne,
    TwoByTwo,
    LShape
}

public class Cell : MonoBehaviour
{
    public RoomType roomType;
    public RoomShape roomShape;

    public int index;
    public int value;

    public SpriteRenderer spriteRenderer;
    public SpriteRenderer roomSprite;

    public List<int> cellList = new List<int>();

    public void SetSpecialRoomSprite(Sprite icon)
    {
        spriteRenderer.sprite = icon;
    }

    public void SetRoomSprite(Sprite roomIcon)
    {
        roomSprite.sprite = roomIcon;
    }

    public void SetRoomType(RoomType newRoomType)
    {
        roomType = newRoomType;
    }

    public void SetRoomShape(RoomShape newRoomShape)
    {
        roomShape = newRoomShape;
    }

    public void RotateCell(List<int> roomIndexes)
    {
        if (roomIndexes.Count != 3) return;

        var cellA = roomIndexes[0];
        var cellB = roomIndexes[1];
        var cellC = roomIndexes[2];

        if (cellA + 1 == cellB && cellA + 10 == cellC)
        {
            roomSprite.transform.localRotation = Quaternion.identity;
        }
        else if (cellA + 1 == cellB && cellB + 9 == cellC)
        {
            roomSprite.transform.localRotation = Quaternion.Euler(0, 0, 270f);
        }
        else if (cellA + 10 == cellB && cellB + 1 == cellC)
        {
            roomSprite.transform.localRotation = Quaternion.Euler(0, 0, 90f);
        }
        else if (cellA + 9 == cellB && cellA + 10 == cellC)
        {
            roomSprite.transform.localRotation = Quaternion.Euler(0, 0, 180f);
        }
    }

    public void ApplyRotation(float angle)
    {
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
