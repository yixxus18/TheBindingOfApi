using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomManager : MonoBehaviour
{
    [System.Serializable]
    public struct RoomColorMapping
    {
        public RoomType roomType;
        public Color color;
    }

    private List<Room> createdRooms;

    [Header("Offset Variables")]
    public float offsetX = 18f;
    public float offsetY = 11f;

    [Header("Prefab References")]
    public Room roomPrefab;
    public Door doorPrefab;

    [Header("Scriptable Object References")]
    public DoorScriptable[] doors;

    [Header("Tilemap References")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public TileBase[] floorTiles;
    public TileBase wallTile;
    public RoomColorMapping[] roomColorMappings;

    [Header("Room Size in Tiles")]
    public int roomWidthInTiles = 18;
    public int roomHeightInTiles = 11;

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
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        int startIndex = 45;
        int startGridX = startIndex % 10;
        int startGridY = startIndex / 10;

        foreach (var currentCell in spawnedCells)
        {
            int gridX = currentCell.index % 10;
            int gridY = currentCell.index / 10;
            int deltaX = gridX - startGridX;
            int deltaY = gridY - startGridY;
            Vector2 roomWorldPosition = new Vector2(deltaX * offsetX, -deltaY * offsetY);
            DrawRoom(roomWorldPosition, currentCell.roomType);
            var spawnedRoomContainer = Instantiate(roomPrefab, roomWorldPosition, Quaternion.identity);
            spawnedRoomContainer.SetupRoom(currentCell);
            createdRooms.Add(spawnedRoomContainer);
        }
    }
    private void DrawRoom(Vector2 roomCenterWorldPos, RoomType roomType)
    {
        Color floorColor = Color.white;
        var mapping = roomColorMappings.FirstOrDefault(m => m.roomType == roomType);
        if (mapping.roomType == roomType)
        {
            floorColor = mapping.color;
        }

        Vector3Int roomCenterCell = floorTilemap.WorldToCell(roomCenterWorldPos);
        int halfWidth = roomWidthInTiles / 2;
        int halfHeight = roomHeightInTiles / 2;
        for (int x = -halfWidth - 1; x <= halfWidth; x++)
        {
            for (int y = -halfHeight - 1; y <= halfHeight; y++)
            {
                if (x == -halfWidth - 1 || x == halfWidth || y == -halfHeight - 1 || y == halfHeight)
                {
                    wallTilemap.SetTile(new Vector3Int(roomCenterCell.x + x, roomCenterCell.y + y, 0), wallTile);
                }
            }
        }

        for (int x = -halfWidth; x < halfWidth; x++)
        {
            for (int y = -halfHeight; y < halfHeight; y++)
            {
                TileBase randomTile = floorTiles[Random.Range(0, floorTiles.Length)];
                Vector3Int tilePosition = new Vector3Int(roomCenterCell.x + x, roomCenterCell.y + y, 0);

                floorTilemap.SetTile(tilePosition, randomTile);
                floorTilemap.SetTileFlags(tilePosition, TileFlags.None);
                floorTilemap.SetColor(tilePosition, floorColor);
            }
        }
    }
}