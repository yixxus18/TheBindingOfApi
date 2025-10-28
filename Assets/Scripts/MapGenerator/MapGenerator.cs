using System.Collections.Generic;
using System.Linq;
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
    private List<int> bigRoomIndexes;

    [Header("Sprite References")]
    [SerializeField] private Sprite item;
    [SerializeField] private Sprite shop;
    [SerializeField] private Sprite boss;
    [SerializeField] private Sprite secret;
    [SerializeField] private Sprite puzzle;

    [Header("Room Shape Sprites")]
    [SerializeField] private Sprite largeRoom;
    [SerializeField] private Sprite verticalRoom;
    [SerializeField] private Sprite horizontalRoom;
    [SerializeField] private Sprite lShapeRoom;

    [Header("Minimap")]
    public MinimapIcon minimapIconPrefab;
    public Transform minimapParent;
    public float minimapCellSize = 20f;

    public static MapGenerator instance;

    private static readonly List<int[]> roomShapes = new()
    {
        new int[]{ 1 }, new int[]{ -1 }, new int[]{ 10 }, new int[]{ -10 },
        new int[] { 1, 10 }, new int[] { 1, 11 }, new int[] { 10, 11 }, new int[] { 9, 10 },
        new int[] { -1, 9 }, new int[] { -1, 10 }, new int[] { 1, -10 }, new int[] { 1, -9 },
        new int[] { -9, -10 }, new int[] { -1, -10 }, new int[] { -1, -11 }, new int[]{ -10, -11 },
        new int[] { 1, 10, 11 }, new int[] { 1, -9, -10 }, new int[] { -1, 9, 10 }, new int[] { -1, -10, -11 }
    };

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
        if (MinimapManager.instance != null)
        {
            MinimapManager.instance.ClearMap();
        }

        foreach (var cell in spawnedCells)
        {
            if (cell != null) Destroy(cell.gameObject);
        }
        spawnedCells.Clear();

        floorPlan = new int[100];
        floorPlanCount = 0;
        cellQueue = new Queue<int>();
        endRooms = new List<int>();
        bigRoomIndexes = new List<int>();

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

        endRooms.RemoveAll(item => bigRoomIndexes.Contains(item) || GetNeighbourCount(item) > 1);
        SetupSpecialRooms();
    }

    private bool VisitCell(int index)
    {
        if (floorPlan[index] != 0 || GetNeighbourCount(index) > 1 || floorPlanCount >= maxRooms || Random.value < 0.5f)
            return false;

        if (Random.value < 0.3f && index != 45)
        {
            foreach (var shape in roomShapes.OrderBy(_ => Random.value))
            {
                if (TryPlaceRoom(index, shape))
                {
                    return true;
                }
            }
        }

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

        CreateMinimapIcon(newCell);
    }

    private bool TryPlaceRoom(int origin, int[] offsets)
    {
        List<int> currentRoomIndexes = new List<int>() { origin };

        foreach (var offset in offsets)
        {
            int currentIndexChecked = origin + offset;
            if (currentIndexChecked < 10 || currentIndexChecked >= 90 || currentIndexChecked % 10 == 0 || currentIndexChecked % 10 == 9) return false;
            if (floorPlan[currentIndexChecked] != 0) return false;
            if (GetNeighbourCount(currentIndexChecked) > 1) return false;
            currentRoomIndexes.Add(currentIndexChecked);
        }

        if (currentRoomIndexes.Count == 1) return false;

        foreach (int index in currentRoomIndexes)
        {
            floorPlan[index] = 1;
            floorPlanCount++;
            cellQueue.Enqueue(index);
            bigRoomIndexes.Add(index);
        }

        SpawnLargeRoom(currentRoomIndexes);
        return true;
    }

    private void SpawnLargeRoom(List<int> largeRoomIndexes)
    {
        Cell newCell = null;
        float combinedX = 0;
        float combinedY = 0;

        foreach (int index in largeRoomIndexes)
        {
            combinedX += index % 10;
            combinedY += index / 10;
        }

        Vector2 centerPosition = new Vector2((combinedX / largeRoomIndexes.Count - 4.5f) * cellSize, -(combinedY / largeRoomIndexes.Count - 4.5f) * cellSize);

        newCell = Instantiate(cellPrefab, centerPosition, Quaternion.identity);

        if (largeRoomIndexes.Count == 4)
        {
            newCell.SetRoomSprite(largeRoom);
            newCell.SetRoomShape(RoomShape.TwoByTwo);
        }
        else if (largeRoomIndexes.Count == 3)
        {
            newCell.SetRoomSprite(lShapeRoom);
            newCell.RotateCell(largeRoomIndexes);
            newCell.SetRoomShape(RoomShape.LShape);
        }
        else if (largeRoomIndexes.Count == 2)
        {
            if (Mathf.Abs(largeRoomIndexes[0] - largeRoomIndexes[1]) > 1)
            {
                newCell.SetRoomSprite(verticalRoom);
                newCell.SetRoomShape(RoomShape.OneByTwo);
            }
            else
            {
                newCell.SetRoomSprite(horizontalRoom);
                newCell.SetRoomShape(RoomShape.TwoByOne);
            }
        }

        newCell.cellList = largeRoomIndexes;
        newCell.cellList.Sort();
        newCell.index = newCell.cellList[0];
        spawnedCells.Add(newCell);

        CreateMinimapIcon(newCell);
    }

    private void CreateMinimapIcon(Cell cell)
    {
        if (MinimapManager.instance == null || minimapIconPrefab == null || minimapParent == null) return;

        float avgX = 0f, avgY = 0f;
        foreach (int index in cell.cellList)
        {
            avgX += index % 10;
            avgY += index / 10;
        }
        avgX /= cell.cellList.Count;
        avgY /= cell.cellList.Count;

        int startGridX = 45 % 10;
        int startGridY = 45 / 10;
        float deltaX = avgX - startGridX;
        float deltaY = avgY - startGridY;

        Vector2 minimapPosition = new Vector2(deltaX * minimapCellSize, -deltaY * minimapCellSize);

        MinimapIcon icon = Instantiate(minimapIconPrefab, minimapParent);

        if (icon.TryGetComponent<RectTransform>(out var rectTransform))
        {
            rectTransform.anchoredPosition = minimapPosition;

            Vector2 newSize = new Vector2(minimapCellSize, minimapCellSize);
            switch (cell.roomShape)
            {
                case RoomShape.OneByTwo: newSize = new Vector2(minimapCellSize, minimapCellSize * 2); break;
                case RoomShape.TwoByOne: newSize = new Vector2(minimapCellSize * 2, minimapCellSize); break;
                case RoomShape.TwoByTwo:
                case RoomShape.LShape: newSize = new Vector2(minimapCellSize * 2, minimapCellSize * 2); break;
            }
            rectTransform.sizeDelta = newSize;
        }
        else
        {
            icon.transform.localPosition = minimapPosition;
        }

        foreach (int subCellIndex in cell.cellList)
        {
            MinimapManager.instance.RegisterMinimapIcon(subCellIndex, icon);
        }
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

        if (itemRoomIndex == -1 || shopRoomIndex == -1 || bossRoomIndex == -1 || secretRoomIndex == -1 || puzzleRoomIndex == -1)
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
            if (cell.cellList.Contains(itemRoomIndex)) { cell.SetSpecialRoomSprite(item); cell.SetRoomType(RoomType.Item); }
            if (cell.cellList.Contains(shopRoomIndex)) { cell.SetSpecialRoomSprite(shop); cell.SetRoomType(RoomType.Shop); }
            if (cell.cellList.Contains(puzzleRoomIndex)) { cell.SetSpecialRoomSprite(puzzle); cell.SetRoomType(RoomType.Puzzle); }
            if (cell.cellList.Contains(bossRoomIndex)) { cell.SetSpecialRoomSprite(boss); cell.SetRoomType(RoomType.Boss); }
            if (cell.cellList.Contains(secretRoomIndex)) { cell.SetSpecialRoomSprite(secret); cell.SetRoomType(RoomType.Secret); }
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
            if (floorPlan[i] == 0 && GetNeighbourCount(i) >= 3)
            {
                possibleSecretRooms.Add(i);
            }
        }
        return possibleSecretRooms.Count > 0 ? possibleSecretRooms[Random.Range(0, possibleSecretRooms.Count)] : -1;
    }

    private int GetNeighbourCount(int index)
    {
        if (index <= 10 || index >= 89 || index % 10 == 0 || index % 10 == 9) return 0;
        return (floorPlan[index - 1] > 0 ? 1 : 0) + (floorPlan[index + 1] > 0 ? 1 : 0) +
               (floorPlan[index - 10] > 0 ? 1 : 0) + (floorPlan[index + 10] > 0 ? 1 : 0);
    }
}