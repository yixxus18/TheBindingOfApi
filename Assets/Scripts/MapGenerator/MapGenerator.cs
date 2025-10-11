using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    private int[] floorPlan;
    public int[] getFloorPlan => floorPlan;

    private int floorPlanCount;
    private int minRooms = 7;
    private int maxRooms = 15;
    private List<int> endRooms;

    private int bossRoomIndex;
    private int secretRoomIndex;
    private int shopRoomIndex;
    private int itemRoomIndex;
    private int puzzleRoomIndex;

    public Cell cellPrefab;
    private float cellSize = 0.5f;
    private Queue<int> cellQueue;
    private List<Cell> spawnedCells;

    public List<Cell> getSpawnedCells => spawnedCells;

    [Header("Sprite References")]
    [SerializeField] private Sprite item;
    [SerializeField] private Sprite shop;
    [SerializeField] private Sprite boss;
    [SerializeField] private Sprite secret;
    [SerializeField] private Sprite puzzle;

    public static MapGenerator instance;

    void Start()
    {
        instance = this;
        spawnedCells = new();
        SetupDungeon();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetupDungeon();
        }
    }

    void SetupDungeon()
    {
        foreach (var cell in spawnedCells)
        {
            if (cell != null) Destroy(cell.gameObject);
        }
        spawnedCells.Clear();

        floorPlan = new int[100];
        floorPlanCount = 0;
        cellQueue = new Queue<int>();
        endRooms = new List<int>();

        VisitCell(45);
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        while (cellQueue.Count > 0)
        {
            int index = cellQueue.Dequeue();
            int x = index % 10;
            bool created = false;

            if (x > 1) created |= VisitCell(index - 1);
            if (x < 9) created |= VisitCell(index + 1);
            if (index > 20) created |= VisitCell(index - 10);
            if (index < 70) created |= VisitCell(index + 10);

            if (!created)
            {
                endRooms.Add(index);
            }
        }

        if (floorPlanCount < minRooms)
        {
            SetupDungeon();
            return;
        }
        endRooms.RemoveAll(item => GetNeighbourCount(item) > 1);

        SetupSpecialRooms();
    }

    private bool VisitCell(int index)
    {
        if (floorPlan[index] != 0 || GetNeighbourCount(index) > 1 || floorPlanCount >= maxRooms || Random.value < 0.5f)
            return false;
        cellQueue.Enqueue(index);
        floorPlan[index] = 1;
        floorPlanCount++;

        SpawnRoom(index); 

        return true;
    }

    private void SpawnRoom(int index)
    {
        int x = index % 10;
        int y = index / 10;
        Vector2 position = new Vector2((x - 4.5f) * cellSize, -(y - 4.5f) * cellSize);

        Cell newCell = Instantiate(cellPrefab, position, Quaternion.identity);
        newCell.index = index;
        newCell.SetRoomShape(RoomShape.OneByOne);
        newCell.SetRoomType(RoomType.Regular);
        newCell.cellList.Add(index);

        spawnedCells.Add(newCell);
    }

    void SetupSpecialRooms()
    {
        bossRoomIndex = endRooms.Count > 0 ? endRooms[endRooms.Count - 1] : -1;
        if (bossRoomIndex != -1)
        {
            endRooms.RemoveAt(endRooms.Count - 1);
        }

        itemRoomIndex = RandomEndRoom();
        shopRoomIndex = RandomEndRoom();
        puzzleRoomIndex = RandomEndRoom();
        secretRoomIndex = PickSecretRoom();

        if (itemRoomIndex == -1 || shopRoomIndex == -1 || bossRoomIndex == -1 ||
            secretRoomIndex == -1 || puzzleRoomIndex == -1)
        {
            SetupDungeon();
            return;
        }
        SpawnRoom(secretRoomIndex);

        UpdateSpecialRoomVisuals();
        RoomManager.instance.SetupRooms(spawnedCells);
    }

    void UpdateSpecialRoomVisuals()
    {
        foreach (var cell in spawnedCells)
        {
            if (cell.index == itemRoomIndex) { cell.SetSpecialRoomSprite(item); cell.SetRoomType(RoomType.Item); }
            if (cell.index == shopRoomIndex) { cell.SetSpecialRoomSprite(shop); cell.SetRoomType(RoomType.Shop); }
            if (cell.index == puzzleRoomIndex) { cell.SetSpecialRoomSprite(puzzle); cell.SetRoomType(RoomType.Puzzle); }
            if (cell.index == bossRoomIndex) { cell.SetSpecialRoomSprite(boss); cell.SetRoomType(RoomType.Boss); }
            if (cell.index == secretRoomIndex) { cell.SetSpecialRoomSprite(secret); cell.SetRoomType(RoomType.Secret); }
        }
    }

    int RandomEndRoom()
    {
        if (endRooms.Count == 0) return -1;
        int randomIndex = Random.Range(0, endRooms.Count);
        int roomIndex = endRooms[randomIndex];
        endRooms.RemoveAt(randomIndex);
        return roomIndex;
    }

    int PickSecretRoom()
    {
        List<int> possibleSecretRooms = new List<int>();
        for (int i = 0; i < floorPlan.Length; i++)
        {
            if (floorPlan[i] == 0) 
            {
                if (GetNeighbourCount(i) >= 3)
                {
                    possibleSecretRooms.Add(i);
                }
            }
        }

        if (possibleSecretRooms.Count > 0)
        {
            return possibleSecretRooms[Random.Range(0, possibleSecretRooms.Count)];
        }
        return -1; 
    }

    private int GetNeighbourCount(int index)
    {
        if (index <= 10 || index >= 89 || index % 10 == 0 || index % 10 == 9) return 0;
        return (floorPlan[index - 1] > 0 ? 1 : 0) +
               (floorPlan[index + 1] > 0 ? 1 : 0) +
               (floorPlan[index - 10] > 0 ? 1 : 0) +
               (floorPlan[index + 10] > 0 ? 1 : 0);
    }
}