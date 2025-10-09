using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    private List<Room> createdRooms;

    [Header("Offset Variables")]
    public float offsetX;
    public float offsetY;

    [Header("Prefab References")]
    public Room roomPrefab;
    public Door doorPrefab;

    [Header("Scriptable Object References")]
    public DoorScriptable[] doors;
    public RoomScriptable[] rooms;

    public static RoomManager instance;

    private void Awake()
    {
        instance = this;
        createdRooms = new List<Room>();
    }

    public void SetupRooms(List<Cell> spawnedCells)
    {
        for (int i = createdRooms.Count - 1; i >= 0; i--)
        {
            if (createdRooms[i] != null)
            {
                Destroy(createdRooms[i].gameObject);
            }
        }
        createdRooms.Clear();
        int startIndex = 45;
        int startGridX = startIndex % 10;
        int startGridY = startIndex / 10;
        foreach (var currentCell in spawnedCells)
        {
            var foundRoom = rooms.FirstOrDefault(x => x.roomShape == currentCell.roomShape && x.roomType == currentCell.roomType);
            if (foundRoom == null)
            {
                foundRoom = rooms.FirstOrDefault(x => x.roomShape == currentCell.roomShape && x.roomType == RoomType.Regular);
                if (foundRoom == null)
                {
                    Debug.LogError($"No se encontró RoomScriptable para RoomShape: {currentCell.roomShape}");
                    continue;
                }
            }

            int gridX = currentCell.index % 10;
            int gridY = currentCell.index / 10;
            int deltaX = gridX - startGridX;
            int deltaY = gridY - startGridY;
            Vector2 roomPosition = new Vector2(deltaX * offsetX, -deltaY * offsetY);
            var spawnedRoom = Instantiate(roomPrefab, roomPosition, Quaternion.identity);
            spawnedRoom.transform.localScale = new Vector3(2f, 2f, 1f);
            spawnedRoom.SetupRoom(currentCell, foundRoom);
            createdRooms.Add(spawnedRoom);
        }
    }
}