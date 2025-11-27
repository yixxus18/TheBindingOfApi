using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomManager : MonoBehaviour
{
    [System.Serializable] public struct RoomColorMapping { public RoomType roomType; public Color color; }
    [System.Serializable] public struct RoomContentConfig { public RoomType roomType; public List<GameObject> possiblePrefabs; public int minCount; public int maxCount; public bool spawnInCenter; }

    private List<Room> createdRooms;

    [Header("Offset Variables")] public float offsetX = 18f; public float offsetY = 12f;
    [Header("Prefab References")] public Room roomPrefab; public Door doorPrefab;
    [Header("Scriptable Object References")] public DoorScriptable[] doors;
    [Header("Tilemap References")] public Tilemap floorTilemap; public Tilemap wallTilemap; public TileBase[] floorTiles; public TileBase wallTile; public RoomColorMapping[] roomColorMappings;
    [Header("Room Size in Tiles")] public int roomWidthInTiles = 16; public int roomHeightInTiles = 11;
    [Header("Room Content Configuration")] public List<RoomContentConfig> roomContents;

    public static RoomManager instance;

    private void Awake()
    {
        instance = this;
        createdRooms = new List<Room>();
    }

    public void SetupRooms(List<Cell> spawnedCells)
    {
        for (int i = createdRooms.Count - 1; i >= 0; i--) { if (createdRooms[i] != null) Destroy(createdRooms[i].gameObject); }
        createdRooms.Clear(); floorTilemap.ClearAllTiles(); wallTilemap.ClearAllTiles();
        int startGridX = 45 % 10; int startGridY = 45 / 10;

        foreach (var roomCell in spawnedCells)
        {
            DrawCompleteRoomShape(roomCell, startGridX, startGridY);
            Vector2 roomWorldPosition = CalculateRoomWorldPosition(roomCell, startGridX, startGridY);
            var spawnedRoomContainer = Instantiate(roomPrefab, roomWorldPosition, Quaternion.identity);
            spawnedRoomContainer.SetupRoom(roomCell);
            SpawnRoomContent(spawnedRoomContainer, roomCell);
            createdRooms.Add(spawnedRoomContainer);
        }
    }

    private void SpawnRoomContent(Room roomContainer, Cell cell)
    {
        if (cell.index == 45) return;

        RoomContentConfig config = roomContents.FirstOrDefault(c => c.roomType == cell.roomType);
        if (config.possiblePrefabs == null || config.possiblePrefabs.Count == 0) return;
        if (cell.roomType == RoomType.Puzzle)
        {
            if (config.possiblePrefabs.Count > 0)
            {
                GameObject pcPrefab = config.possiblePrefabs[0];
                Vector3 spawnPos = roomContainer.transform.position;
                spawnPos.z = -1f;
                GameObject pcInstance = Instantiate(pcPrefab, spawnPos, Quaternion.identity, roomContainer.transform);
                RoomSetup roomSetup = FindFirstObjectByType<RoomSetup>();
                if (roomSetup != null)
                {
                    TerminalActivator activator = pcInstance.GetComponent<TerminalActivator>();
                    if (activator != null)
                    {
                        activator.puzzleContexts = roomSetup.GetPuzzles();
                    }
                }
            }
            return;
        }
        if (cell.roomType == RoomType.Shop)
        {
            float spacing = 4.0f;
            float startX = -(config.possiblePrefabs.Count - 1) * spacing / 2f;
            for (int i = 0; i < config.possiblePrefabs.Count; i++)
            {
                if (config.possiblePrefabs[i] != null)
                {
                    Vector3 spawnPos = roomContainer.transform.position + new Vector3(startX + (i * spacing), 0, -1f);
                    Instantiate(config.possiblePrefabs[i], spawnPos, Quaternion.identity, roomContainer.transform);
                }
            }
        }
        else if (config.spawnInCenter)
        {
            GameObject prefabToSpawn = config.possiblePrefabs[Random.Range(0, config.possiblePrefabs.Count)];
            Vector3 spawnPos = roomContainer.transform.position;
            spawnPos.z = -1f;
            Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, roomContainer.transform);
        }
        else
        {
            int count = Random.Range(config.minCount, config.maxCount + 1);
            float rangeX = (roomWidthInTiles / 2f) - 2f;
            float rangeY = (roomHeightInTiles / 2f) - 2f;

            for (int i = 0; i < count; i++)
            {
                GameObject prefabToSpawn = config.possiblePrefabs[Random.Range(0, config.possiblePrefabs.Count)];
                Vector2 randomPos = new Vector2(Random.Range(-rangeX, rangeX), Random.Range(-rangeY, rangeY));
                Vector3 spawnPos = roomContainer.transform.position + (Vector3)randomPos;
                spawnPos.z = -1f;
                Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, roomContainer.transform);
            }
        }
    }

    private void DrawCompleteRoomShape(Cell room, int startGridX, int startGridY) { HashSet<Vector3Int> floorPositions = new HashSet<Vector3Int>(); Color roomColor = GetRoomColor(room.roomType); int halfWidth = roomWidthInTiles / 2; int halfHeight = roomHeightInTiles / 2; int gapX = (int)offsetX - roomWidthInTiles; int gapY = (int)offsetY - roomHeightInTiles; foreach (int index in room.cellList) { Vector3Int roomCenter = GetTilemapCenterForIndex(index, startGridX, startGridY); for (int x = -halfWidth; x < halfWidth; x++) { for (int y = -halfHeight; y <= halfHeight; y++) { floorPositions.Add(new Vector3Int(roomCenter.x + x, roomCenter.y + y, 0)); } } if (room.cellList.Contains(index + 1)) { for (int x = halfWidth; x < halfWidth + gapX; x++) { for (int y = -halfHeight; y <= halfHeight; y++) { floorPositions.Add(new Vector3Int(roomCenter.x + x, roomCenter.y + y, 0)); } } } if (room.cellList.Contains(index + 10)) { for (int x = -halfWidth; x < halfWidth; x++) { for (int y = 1; y <= gapY; y++) { floorPositions.Add(new Vector3Int(roomCenter.x + x, roomCenter.y - halfHeight - y, 0)); } } } if (room.cellList.Contains(index + 1) && room.cellList.Contains(index + 10) && room.cellList.Contains(index + 11)) { for (int x = halfWidth; x < halfWidth + gapX; x++) { for (int y = 1; y <= gapY; y++) { floorPositions.Add(new Vector3Int(roomCenter.x + x, roomCenter.y - halfHeight - y, 0)); } } } } foreach (var pos in floorPositions) { TileBase randomTile = floorTiles[Random.Range(0, floorTiles.Length)]; floorTilemap.SetTile(pos, randomTile); floorTilemap.SetTileFlags(pos, TileFlags.None); floorTilemap.SetColor(pos, roomColor); } foreach (var pos in floorPositions) { for (int x = -1; x <= 1; x++) { for (int y = -1; y <= 1; y++) { if (x == 0 && y == 0) continue; Vector3Int neighborPos = new Vector3Int(pos.x + x, pos.y + y, pos.z); if (!floorPositions.Contains(neighborPos)) { wallTilemap.SetTile(neighborPos, wallTile); } } } } }
    private Vector3Int GetTilemapCenterForIndex(int index, int startGridX, int startGridY) { int gridX = index % 10; int gridY = index / 10; int deltaX = gridX - startGridX; int deltaY = gridY - startGridY; Vector2 worldPos = new Vector2(deltaX * offsetX, -deltaY * offsetY); return floorTilemap.WorldToCell(worldPos); }
    private Color GetRoomColor(RoomType roomType) { var mapping = roomColorMappings.FirstOrDefault(m => m.roomType == roomType); return mapping.roomType == roomType ? mapping.color : Color.white; }
    private Vector2 CalculateRoomWorldPosition(Cell cell, int startGridX, int startGridY) { float combinedX = 0f, combinedY = 0f; foreach (int index in cell.cellList) { combinedX += index % 10; combinedY += index / 10; } float avgX = combinedX / cell.cellList.Count; float avgY = combinedY / cell.cellList.Count; float deltaX = avgX - startGridX; float deltaY = avgY - startGridY; return new Vector2(deltaX * offsetX, -deltaY * offsetY); }
}