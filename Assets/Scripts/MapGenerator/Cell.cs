using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    Regular,
    Item,
    Shop,
    Boss,
    Secret,
    Puzzle
}

public enum RoomShape
{
    OneByOne,
    OneByTwo,
    TwoByOne,
    TwoByTwo,
    LShape
}

public enum RoomState
{
    Hidden,
    Visited,
    Current
}

public class Cell : MonoBehaviour
{
    public RoomType roomType;
    public RoomShape roomShape;

    public int index;
    public int value;

    public SpriteRenderer spriteRenderer;
    public SpriteRenderer mainRenderer;
    public SpriteRenderer roomSprite;

    public List<int> cellList = new List<int>();

    private void Awake()
    {
        if (mainRenderer == null)
            mainRenderer = transform.Find("Sprite")?.GetComponent<SpriteRenderer>();
    }

    public void SetSpecialRoomSprite(Sprite icon)
    {
        if (mainRenderer != null)
            mainRenderer.sprite = icon;
    }

    public void SetRoomSprite(Sprite roomIcon)
    {
        if (mainRenderer != null)
            mainRenderer.sprite = roomIcon;
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
            roomSprite.transform.localRotation = Quaternion.identity;
        else if (cellA + 1 == cellB && cellB + 9 == cellC)
            roomSprite.transform.localRotation = Quaternion.Euler(0, 0, 270f);
        else if (cellA + 10 == cellB && cellB + 1 == cellC)
            roomSprite.transform.localRotation = Quaternion.Euler(0, 0, 90f);
        else if (cellA + 9 == cellB && cellA + 10 == cellC)
            roomSprite.transform.localRotation = Quaternion.Euler(0, 0, 180f);
    }

    public void ApplyRotation(float angle)
    {
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    public void SetRoomState(RoomState state)
    {
        switch (state)
        {
            case RoomState.Hidden:
                if (mainRenderer != null) mainRenderer.enabled = false;
                break;
            case RoomState.Visited:
            case RoomState.Current:
                if (mainRenderer != null) mainRenderer.enabled = true;
                break;
        }
    }
}
