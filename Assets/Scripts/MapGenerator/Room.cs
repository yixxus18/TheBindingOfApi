using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EdgeDirection
{
    Up,
    Down,
    Left,
    Right
}

[RequireComponent(typeof(BoxCollider2D), typeof(PolygonCollider2D))]
public class Room : MonoBehaviour
{
    public PolygonCollider2D cameraConfiner;
    private BoxCollider2D triggerCollider;
    private Cell associatedCell;

    private void Awake()
    {
        triggerCollider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (CameraConfinerManager.instance != null && cameraConfiner != null)
            {
                CameraConfinerManager.instance.UpdateBounds(cameraConfiner);
            }
            if (MinimapManager.instance != null && associatedCell != null)
            {
                MinimapManager.instance.OnPlayerEnterRoom(associatedCell.index);
            }
        }
    }

    public void SetupRoom(Cell currentCell)
    {
        this.associatedCell = currentCell;
        ResizeColliders(currentCell);
        if (currentCell.roomType == RoomType.Secret) return;

        var floorplan = MapGenerator.instance.getFloorPlan;
        var spawnedCells = MapGenerator.instance.getSpawnedCells;

        switch (currentCell.roomShape)
        {
            case RoomShape.OneByOne:
                SetupOneByOne(currentCell, floorplan, spawnedCells);
                break;
            case RoomShape.OneByTwo:
                SetupOneByTwo(currentCell, floorplan, spawnedCells);
                break;
            case RoomShape.TwoByOne:
                SetupTwoByOne(currentCell, floorplan, spawnedCells);
                break;
            case RoomShape.TwoByTwo:
                SetupTwoByTwo(currentCell, floorplan, spawnedCells);
                break;
            case RoomShape.LShape:
                SetupLShapeRoom(currentCell, floorplan, spawnedCells);
                break;
        }
    }

    private void ResizeColliders(Cell cell)
    {
        var rm = RoomManager.instance;
        if (rm == null) return;

        float offsetX = rm.offsetX;
        float offsetY = rm.offsetY;
        float visualWidth = rm.roomWidthInTiles;
        float visualHeight = rm.roomHeightInTiles;

        Vector2 boxSize = Vector2.zero;
        Vector2[] polygonPoints = new Vector2[0];

        switch (cell.roomShape)
        {
            case RoomShape.OneByOne:
                boxSize = new Vector2(offsetX, offsetY);
                polygonPoints = new Vector2[]
                {
                new Vector2(-visualWidth / 2, visualHeight / 2),
                new Vector2(visualWidth / 2, visualHeight / 2),
                new Vector2(visualWidth / 2, -visualHeight / 2),
                new Vector2(-visualWidth / 2, -visualHeight / 2)
                };
                break;

            case RoomShape.TwoByOne:
                boxSize = new Vector2(offsetX * 2, offsetY);
                polygonPoints = new Vector2[]
                {
                new Vector2(-visualWidth, visualHeight / 2),
                new Vector2(visualWidth, visualHeight / 2),
                new Vector2(visualWidth, -visualHeight / 2),
                new Vector2(-visualWidth, -visualHeight / 2)
                };
                break;

            case RoomShape.OneByTwo:
                boxSize = new Vector2(offsetX, offsetY * 2);
                polygonPoints = new Vector2[]
                {
                new Vector2(-visualWidth / 2, visualHeight),
                new Vector2(visualWidth / 2, visualHeight),
                new Vector2(visualWidth / 2, -visualHeight),
                new Vector2(-visualWidth / 2, -visualHeight)
                };
                break;

            case RoomShape.TwoByTwo:
                boxSize = new Vector2(offsetX * 2, offsetY * 2);
                polygonPoints = new Vector2[]
                {
                new Vector2(-visualWidth, visualHeight),
                new Vector2(visualWidth, visualHeight),
                new Vector2(visualWidth, -visualHeight),
                new Vector2(-visualWidth, -visualHeight)
                };
                break;

            case RoomShape.LShape:
                boxSize = new Vector2(offsetX * 2, offsetY * 2);
                var cellA = cell.cellList[0];
                var cellB = cell.cellList[1];
                var cellC = cell.cellList[2];

                if (cellA + 1 == cellB && cellA + 10 == cellC) // L hacia abajo-derecha
                {
                    polygonPoints = new Vector2[]
                    {
                    new Vector2(-visualWidth, visualHeight),
                    new Vector2(visualWidth, visualHeight),
                    new Vector2(visualWidth, 0),
                    new Vector2(0, 0),
                    new Vector2(0, -visualHeight),
                    new Vector2(-visualWidth, -visualHeight)
                    };
                }
                else if (cellA + 1 == cellB && cellB + 9 == cellC) // L hacia abajo-izquierda
                {
                    polygonPoints = new Vector2[]
                    {
                    new Vector2(-visualWidth, visualHeight),
                    new Vector2(0, visualHeight),
                    new Vector2(0, -visualHeight),
                    new Vector2(visualWidth, -visualHeight),
                    new Vector2(visualWidth, 0),
                    new Vector2(-visualWidth, 0)
                    };
                }
                else if (cellA + 10 == cellB && cellB + 1 == cellC) // L hacia arriba-derecha
                {
                    polygonPoints = new Vector2[]
                    {
                    new Vector2(-visualWidth, visualHeight),
                    new Vector2(visualWidth, visualHeight),
                    new Vector2(visualWidth, 0),
                    new Vector2(0, 0),
                    new Vector2(0, -visualHeight),
                    new Vector2(-visualWidth, -visualHeight)
                    };
                    transform.rotation = Quaternion.Euler(0, 0, 90);
                }
                else // L hacia arriba-izquierda
                {
                    polygonPoints = new Vector2[]
                    {
                    new Vector2(-visualWidth, visualHeight),
                    new Vector2(0, visualHeight),
                    new Vector2(0, 0),
                    new Vector2(visualWidth, 0),
                    new Vector2(visualWidth, -visualHeight),
                    new Vector2(-visualWidth, -visualHeight)
                    };
                    transform.rotation = Quaternion.Euler(0, 0, 270);
                }
                break;
        }

        if (triggerCollider != null)
        {
            triggerCollider.size = boxSize;
        }

        if (cameraConfiner != null)
        {
            cameraConfiner.SetPath(0, polygonPoints);
        }
    }




    public void SetupOneByOne(Cell cell, int[] floorplan, List<Cell> cellList)
    {
        var currentCell = cell.cellList[0];
        float hOffset = RoomManager.instance.offsetX / 2f - 1.0f;
        float vOffset = RoomManager.instance.offsetY / 2f - 1.0f;

        TryPlaceDoor(currentCell, new Vector2(0, vOffset), EdgeDirection.Up, floorplan, cellList, cell);
        TryPlaceDoor(currentCell, new Vector2(0, -vOffset), EdgeDirection.Down, floorplan, cellList, cell);
        TryPlaceDoor(currentCell, new Vector2(-hOffset, 0), EdgeDirection.Left, floorplan, cellList, cell);
        TryPlaceDoor(currentCell, new Vector2(hOffset, 0), EdgeDirection.Right, floorplan, cellList, cell);
    }
    public void SetupOneByTwo(Cell cell, int[] floorplan, List<Cell> cellList)
    {
        if (cell.cellList.Count < 2) return;
        var cellA = cell.cellList[0];
        var cellB = cell.cellList[1];
        float hOffset = RoomManager.instance.offsetX / 2f - 1.0f;
        float vOuterOffset = RoomManager.instance.offsetY - 1.0f;
        float vInnerOffset = RoomManager.instance.offsetY / 2f;

        TryPlaceDoor(cellA, new Vector2(0, vOuterOffset), EdgeDirection.Up, floorplan, cellList, cell);
        TryPlaceDoor(cellA, new Vector2(-hOffset, vInnerOffset), EdgeDirection.Left, floorplan, cellList, cell);
        TryPlaceDoor(cellA, new Vector2(hOffset, vInnerOffset), EdgeDirection.Right, floorplan, cellList, cell);

        TryPlaceDoor(cellB, new Vector2(0, -vOuterOffset), EdgeDirection.Down, floorplan, cellList, cell);
        TryPlaceDoor(cellB, new Vector2(-hOffset, -vInnerOffset), EdgeDirection.Left, floorplan, cellList, cell);
        TryPlaceDoor(cellB, new Vector2(hOffset, -vInnerOffset), EdgeDirection.Right, floorplan, cellList, cell);
    }
    public void SetupTwoByOne(Cell cell, int[] floorplan, List<Cell> cellList)
    {
        if (cell.cellList.Count < 2) return;
        var cellA = cell.cellList[0];
        var cellB = cell.cellList[1];
        float hOuterOffset = RoomManager.instance.offsetX - 1.0f;
        float hInnerOffset = RoomManager.instance.offsetX / 2f;
        float vOffset = RoomManager.instance.offsetY / 2f - 1.0f;

        TryPlaceDoor(cellA, new Vector2(-hInnerOffset, vOffset), EdgeDirection.Up, floorplan, cellList, cell);
        TryPlaceDoor(cellA, new Vector2(-hOuterOffset, 0), EdgeDirection.Left, floorplan, cellList, cell);
        TryPlaceDoor(cellA, new Vector2(-hInnerOffset, -vOffset), EdgeDirection.Down, floorplan, cellList, cell);

        TryPlaceDoor(cellB, new Vector2(hInnerOffset, vOffset), EdgeDirection.Up, floorplan, cellList, cell);
        TryPlaceDoor(cellB, new Vector2(hInnerOffset, -vOffset), EdgeDirection.Down, floorplan, cellList, cell);
        TryPlaceDoor(cellB, new Vector2(hOuterOffset, 0), EdgeDirection.Right, floorplan, cellList, cell);
    }
    public void SetupTwoByTwo(Cell cell, int[] floorplan, List<Cell> cellList)
    {
        if (cell.cellList.Count < 4) return;
        var cellA = cell.cellList[0];
        var cellB = cell.cellList[1];
        var cellC = cell.cellList[2];
        var cellD = cell.cellList[3];
        float hOuterOffset = RoomManager.instance.offsetX - 1.0f;
        float hInnerOffset = RoomManager.instance.offsetX / 2f;
        float vOuterOffset = RoomManager.instance.offsetY - 1.0f;
        float vInnerOffset = RoomManager.instance.offsetY / 2f;

        TryPlaceDoor(cellA, new Vector2(-hInnerOffset, vOuterOffset), EdgeDirection.Up, floorplan, cellList, cell);
        TryPlaceDoor(cellB, new Vector2(hInnerOffset, vOuterOffset), EdgeDirection.Up, floorplan, cellList, cell);
        TryPlaceDoor(cellA, new Vector2(-hOuterOffset, vInnerOffset), EdgeDirection.Left, floorplan, cellList, cell);
        TryPlaceDoor(cellC, new Vector2(-hOuterOffset, -vInnerOffset), EdgeDirection.Left, floorplan, cellList, cell);
        TryPlaceDoor(cellC, new Vector2(-hInnerOffset, -vOuterOffset), EdgeDirection.Down, floorplan, cellList, cell);
        TryPlaceDoor(cellD, new Vector2(hInnerOffset, -vOuterOffset), EdgeDirection.Down, floorplan, cellList, cell);
        TryPlaceDoor(cellB, new Vector2(hOuterOffset, vInnerOffset), EdgeDirection.Right, floorplan, cellList, cell);
        TryPlaceDoor(cellD, new Vector2(hOuterOffset, -vInnerOffset), EdgeDirection.Right, floorplan, cellList, cell);
    }
    public void SetupLShapeRoom(Cell cell, int[] floorplan, List<Cell> cellList)
    {
        if (cell.cellList.Count < 3) return;

        var rm = RoomManager.instance;
        float offsetX = rm.offsetX;
        float offsetY = rm.offsetY;
        float h_offset_single = offsetX / 2f;
        float v_offset_single = offsetY / 2f;
        float door_inset = 1.0f;

        var cellA = cell.cellList[0];
        var cellB = cell.cellList[1];
        var cellC = cell.cellList[2];

        if (cellA + 1 == cellB && cellA + 10 == cellC)
        {
            Vector2 localCenterA = new Vector2(-offsetX / 3f, offsetY / 3f);
            Vector2 localCenterB = new Vector2(offsetX * 2f / 3f, offsetY / 3f);
            Vector2 localCenterC = new Vector2(-offsetX / 3f, -offsetY * 2f / 3f);

            TryPlaceDoor(cellA, localCenterA + new Vector2(0, v_offset_single - door_inset), EdgeDirection.Up, floorplan, cellList, cell);
            TryPlaceDoor(cellA, localCenterA + new Vector2(-h_offset_single + door_inset, 0), EdgeDirection.Left, floorplan, cellList, cell);
            TryPlaceDoor(cellB, localCenterB + new Vector2(0, v_offset_single - door_inset), EdgeDirection.Up, floorplan, cellList, cell);
            TryPlaceDoor(cellB, localCenterB + new Vector2(h_offset_single - door_inset, 0), EdgeDirection.Right, floorplan, cellList, cell);
            TryPlaceDoor(cellC, localCenterC + new Vector2(0, -v_offset_single + door_inset), EdgeDirection.Down, floorplan, cellList, cell);
            TryPlaceDoor(cellC, localCenterC + new Vector2(-h_offset_single + door_inset, 0), EdgeDirection.Left, floorplan, cellList, cell);
        }
        else if (cellA + 1 == cellB && cellB + 9 == cellC)
        {
            Vector2 localCenterA = new Vector2(-offsetX * 2f / 3f, offsetY / 3f);
            Vector2 localCenterB = new Vector2(offsetX / 3f, offsetY / 3f);
            Vector2 localCenterC = new Vector2(offsetX / 3f, -offsetY * 2f / 3f);

            TryPlaceDoor(cellA, localCenterA + new Vector2(0, v_offset_single - door_inset), EdgeDirection.Up, floorplan, cellList, cell);
            TryPlaceDoor(cellA, localCenterA + new Vector2(-h_offset_single + door_inset, 0), EdgeDirection.Left, floorplan, cellList, cell);
            TryPlaceDoor(cellB, localCenterB + new Vector2(0, v_offset_single - door_inset), EdgeDirection.Up, floorplan, cellList, cell);
            TryPlaceDoor(cellB, localCenterB + new Vector2(h_offset_single - door_inset, 0), EdgeDirection.Right, floorplan, cellList, cell);
            TryPlaceDoor(cellC, localCenterC + new Vector2(0, -v_offset_single + door_inset), EdgeDirection.Down, floorplan, cellList, cell);
            TryPlaceDoor(cellC, localCenterC + new Vector2(h_offset_single - door_inset, 0), EdgeDirection.Right, floorplan, cellList, cell);
        }
        else if (cellA + 10 == cellB && cellB + 1 == cellC)
        {
            Vector2 localCenterA = new Vector2(-offsetX / 3f, offsetY * 2f / 3f);
            Vector2 localCenterB = new Vector2(-offsetX / 3f, -offsetY / 3f);
            Vector2 localCenterC = new Vector2(offsetX * 2f / 3f, -offsetY / 3f);

            TryPlaceDoor(cellA, localCenterA + new Vector2(0, v_offset_single - door_inset), EdgeDirection.Up, floorplan, cellList, cell);
            TryPlaceDoor(cellA, localCenterA + new Vector2(-h_offset_single + door_inset, 0), EdgeDirection.Left, floorplan, cellList, cell);
            TryPlaceDoor(cellB, localCenterB + new Vector2(0, -v_offset_single + door_inset), EdgeDirection.Down, floorplan, cellList, cell);
            TryPlaceDoor(cellB, localCenterB + new Vector2(-h_offset_single + door_inset, 0), EdgeDirection.Left, floorplan, cellList, cell);
            TryPlaceDoor(cellC, localCenterC + new Vector2(0, -v_offset_single + door_inset), EdgeDirection.Down, floorplan, cellList, cell);
            TryPlaceDoor(cellC, localCenterC + new Vector2(h_offset_single - door_inset, 0), EdgeDirection.Right, floorplan, cellList, cell);
        }
        else if (cellA + 9 == cellB && cellA + 10 == cellC)
        {
            Vector2 localCenterA = new Vector2(offsetX / 3f, offsetY * 2f / 3f);
            Vector2 localCenterB = new Vector2(-offsetX * 2f / 3f, -offsetY / 3f);
            Vector2 localCenterC = new Vector2(offsetX / 3f, -offsetY / 3f);

            TryPlaceDoor(cellA, localCenterA + new Vector2(0, v_offset_single - door_inset), EdgeDirection.Up, floorplan, cellList, cell);
            TryPlaceDoor(cellA, localCenterA + new Vector2(h_offset_single - door_inset, 0), EdgeDirection.Right, floorplan, cellList, cell);
            TryPlaceDoor(cellB, localCenterB + new Vector2(0, -v_offset_single + door_inset), EdgeDirection.Down, floorplan, cellList, cell);
            TryPlaceDoor(cellB, localCenterB + new Vector2(-h_offset_single + door_inset, 0), EdgeDirection.Left, floorplan, cellList, cell);
            TryPlaceDoor(cellC, localCenterC + new Vector2(0, -v_offset_single + door_inset), EdgeDirection.Down, floorplan, cellList, cell);
            TryPlaceDoor(cellC, localCenterC + new Vector2(h_offset_single - door_inset, 0), EdgeDirection.Right, floorplan, cellList, cell);
        }
    }
    private void TryPlaceDoor(int fromIndex, Vector2 positionOffset, EdgeDirection direction, int[] floorplan, List<Cell> cellList, Cell currentCell)
    {
        int neighbourIndex = fromIndex + GetOffset(direction);
        if (neighbourIndex < 0 || neighbourIndex >= floorplan.Length || floorplan[neighbourIndex] != 1) return;

        var foundCell = cellList.FirstOrDefault(x => x.cellList.Contains(neighbourIndex));
        if (foundCell == null || foundCell.roomType == RoomType.Secret) return;

        var door = Instantiate(RoomManager.instance.doorPrefab, transform);
        door.transform.localPosition = positionOffset;
        SetupDoor(door, direction, currentCell.roomType == RoomType.Regular ? foundCell.roomType : currentCell.roomType);
    }
    private void SetupDoor(Door door, EdgeDirection direction, RoomType roomType)
    {
        var doorTypes = GetDoorOptions(roomType);
        if (doorTypes == null)
        {
            Destroy(door.gameObject);
            return;
        }
        door.SetDoorSprite(doorTypes.horizontalDoor, direction);
        var trigger = door.gameObject.AddComponent<DoorTrigger>();
        trigger.doorDirection = direction;
    }
    private DoorScriptable GetDoorOptions(RoomType roomType)
    {
        return RoomManager.instance.doors.FirstOrDefault(x => x.roomType == roomType);
    }
    private int GetOffset(EdgeDirection direction)
    {
        switch (direction)
        {
            case EdgeDirection.Up: return -10;
            case EdgeDirection.Down: return 10;
            case EdgeDirection.Right: return 1;
            case EdgeDirection.Left: return -1;
        }
        return 0;
    }
}