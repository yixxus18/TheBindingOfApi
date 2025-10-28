using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomManager : MonoBehaviour
{
    [System.Serializable]
    public struct RoomColorMapping { public RoomType roomType; public Color color; }

    private List<Room> createdRooms;

    [Header("Offset Variables")]
    public float offsetX = 18f;
    public float offsetY = 12f;

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
    public int roomWidthInTiles = 16;
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
            if (createdRooms[i] != null) Destroy(createdRooms[i].gameObject);
        }
        createdRooms.Clear();
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        int startGridX = 45 % 10;
        int startGridY = 45 / 10;

        foreach (var roomCell in spawnedCells)
        {
            DrawCompleteRoomShape(roomCell, startGridX, startGridY);

            Vector2 roomWorldPosition = CalculateRoomWorldPosition(roomCell, startGridX, startGridY);
            var spawnedRoomContainer = Instantiate(roomPrefab, roomWorldPosition, Quaternion.identity);
            spawnedRoomContainer.SetupRoom(roomCell);
            createdRooms.Add(spawnedRoomContainer);
        }
    }

    private void DrawCompleteRoomShape(Cell room, int startGridX, int startGridY)
    {
        HashSet<Vector3Int> floorPositions = new HashSet<Vector3Int>();
        Color roomColor = GetRoomColor(room.roomType);

        int halfWidth = roomWidthInTiles / 2;
        int halfHeight = roomHeightInTiles / 2;
        int gapX = (int)offsetX - roomWidthInTiles;
        int gapY = (int)offsetY - roomHeightInTiles;

        foreach (int index in room.cellList)
        {
            Vector3Int roomCenter = GetTilemapCenterForIndex(index, startGridX, startGridY);

            // 1. Dibujar el bloque principal de suelo
            for (int x = -halfWidth; x < halfWidth; x++)
            {
                // Usamos <= halfHeight para asegurar 11 baldosas de alto si halfHeight es 5 (-5 a 5 son 11)
                for (int y = -halfHeight; y <= halfHeight; y++)
                {
                    floorPositions.Add(new Vector3Int(roomCenter.x + x, roomCenter.y + y, 0));
                }
            }

            // 2. Rellenar huecos horizontales (puente a la derecha)
            if (room.cellList.Contains(index + 1))
            {
                for (int x = halfWidth; x < halfWidth + gapX; x++)
                {
                    for (int y = -halfHeight; y <= halfHeight; y++)
                    {
                        floorPositions.Add(new Vector3Int(roomCenter.x + x, roomCenter.y + y, 0));
                    }
                }
            }

            // 3. Rellenar huecos verticales (puente hacia abajo)
            if (room.cellList.Contains(index + 10))
            {
                for (int x = -halfWidth; x < halfWidth; x++)
                {
                    for (int y = 1; y <= gapY; y++)
                    {
                        // Dibujamos debajo del límite inferior actual
                        floorPositions.Add(new Vector3Int(roomCenter.x + x, roomCenter.y - halfHeight - y, 0));
                    }
                }
            }
            // 4. Rellenar la esquina si es necesario (para salas 2x2)
            if (room.cellList.Contains(index + 1) && room.cellList.Contains(index + 10) && room.cellList.Contains(index + 11))
            {
                for (int x = halfWidth; x < halfWidth + gapX; x++)
                {
                    for (int y = 1; y <= gapY; y++)
                    {
                        floorPositions.Add(new Vector3Int(roomCenter.x + x, roomCenter.y - halfHeight - y, 0));
                    }
                }
            }
        }

        // Dibujar todo el suelo recopilado
        foreach (var pos in floorPositions)
        {
            TileBase randomTile = floorTiles[Random.Range(0, floorTiles.Length)];
            floorTilemap.SetTile(pos, randomTile);
            floorTilemap.SetTileFlags(pos, TileFlags.None);
            floorTilemap.SetColor(pos, roomColor);
        }

        // Dibujar paredes alrededor del perímetro del suelo recopilado
        foreach (var pos in floorPositions)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    Vector3Int neighborPos = new Vector3Int(pos.x + x, pos.y + y, pos.z);
                    if (!floorPositions.Contains(neighborPos))
                    {
                        wallTilemap.SetTile(neighborPos, wallTile);
                    }
                }
            }
        }
    }

    private Vector3Int GetTilemapCenterForIndex(int index, int startGridX, int startGridY)
    {
        int gridX = index % 10;
        int gridY = index / 10;
        int deltaX = gridX - startGridX;
        int deltaY = gridY - startGridY;
        Vector2 worldPos = new Vector2(deltaX * offsetX, -deltaY * offsetY);
        return floorTilemap.WorldToCell(worldPos);
    }

    private Color GetRoomColor(RoomType roomType)
    {
        var mapping = roomColorMappings.FirstOrDefault(m => m.roomType == roomType);
        return mapping.roomType == roomType ? mapping.color : Color.white;
    }

    private Vector2 CalculateRoomWorldPosition(Cell cell, int startGridX, int startGridY)
    {
        float combinedX = 0f, combinedY = 0f;
        foreach (int index in cell.cellList)
        {
            combinedX += index % 10;
            combinedY += index / 10;
        }
        float avgX = combinedX / cell.cellList.Count;
        float avgY = combinedY / cell.cellList.Count;
        float deltaX = avgX - startGridX;
        float deltaY = avgY - startGridY;
        return new Vector2(deltaX * offsetX, -deltaY * offsetY);
    }
}