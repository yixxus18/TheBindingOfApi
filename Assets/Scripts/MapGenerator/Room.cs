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
    private Cell previousCell;

    private float doorInset = 1.5f;

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

            if (associatedCell != null)
            {
                associatedCell.SetRoomState(RoomState.Current);
                if (previousCell != null && previousCell != associatedCell)
                    previousCell.SetRoomState(RoomState.Visited);
                previousCell = associatedCell;
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
        float halfOffsetX = offsetX / 2f;
        float halfOffsetY = offsetY / 2f;

        Vector2 boxSize = Vector2.zero;
        Vector2[] polygonPoints = new Vector2[0];
        Vector2 polygonOffset = Vector2.zero;
        float verticalAdjustment = 0.5f;

        switch (cell.roomShape)
        {
            case RoomShape.OneByOne:
                boxSize = new Vector2(offsetX, offsetY);
                polygonPoints = new Vector2[] { new Vector2(-halfOffsetX, halfOffsetY), new Vector2(halfOffsetX, halfOffsetY), new Vector2(halfOffsetX, -halfOffsetY), new Vector2(-halfOffsetX, -halfOffsetY) };
                break;
            case RoomShape.TwoByOne:
                boxSize = new Vector2(offsetX * 2, offsetY);
                polygonPoints = new Vector2[] { new Vector2(-offsetX, halfOffsetY), new Vector2(offsetX, halfOffsetY), new Vector2(offsetX, -halfOffsetY), new Vector2(-offsetX, -halfOffsetY) };
                break;
            case RoomShape.OneByTwo:
                boxSize = new Vector2(offsetX, offsetY * 2);
                polygonPoints = new Vector2[] { new Vector2(-halfOffsetX, offsetY), new Vector2(halfOffsetX, offsetY), new Vector2(halfOffsetX, -offsetY), new Vector2(-halfOffsetX, -offsetY) };
                break;
            case RoomShape.TwoByTwo:
                boxSize = new Vector2(offsetX * 2, offsetY * 2);
                polygonPoints = new Vector2[] { new Vector2(-offsetX, offsetY), new Vector2(offsetX, offsetY), new Vector2(offsetX, -offsetY), new Vector2(-offsetX, -offsetY) };
                break;
            case RoomShape.LShape:
                boxSize = new Vector2(offsetX * 2, offsetY * 2);
                var cellA = cell.cellList[0]; var cellB = cell.cellList[1]; var cellC = cell.cellList[2];
                float fineOffsetX = offsetX * 0.167f; float fineOffsetY = offsetY * 0.167f;

                if (cellA + 1 == cellB && cellA + 10 == cellC)
                {
                    polygonPoints = new Vector2[] { new Vector2(-offsetX, offsetY), new Vector2(offsetX, offsetY), new Vector2(offsetX, 0), new Vector2(0, 0), new Vector2(0, -offsetY), new Vector2(-offsetX, -offsetY) };
                    polygonOffset = new Vector2(fineOffsetX, -fineOffsetY);
                }
                else if (cellA + 1 == cellB && cellB + 9 == cellC)
                {
                    polygonPoints = new Vector2[] { new Vector2(-offsetX, offsetY), new Vector2(offsetX, offsetY), new Vector2(offsetX, -offsetY), new Vector2(0, -offsetY), new Vector2(0, 0), new Vector2(-offsetX, 0) };
                    polygonOffset = new Vector2(-fineOffsetX, -fineOffsetY);
                }
                else if (cellA + 1 == cellB && cellB + 10 == cellC)
                {
                    polygonPoints = new Vector2[] { new Vector2(-offsetX, offsetY), new Vector2(offsetX, offsetY), new Vector2(offsetX, -offsetY), new Vector2(0, -offsetY), new Vector2(0, 0), new Vector2(-offsetX, 0) };
                    polygonOffset = new Vector2(-fineOffsetX, -fineOffsetY);
                }
                else if (cellA + 10 == cellB && cellB + 1 == cellC)
                {
                    polygonPoints = new Vector2[] { new Vector2(-offsetX, offsetY), new Vector2(0, offsetY), new Vector2(0, 0), new Vector2(offsetX, 0), new Vector2(offsetX, -offsetY), new Vector2(-offsetX, -offsetY) };
                    polygonOffset = new Vector2(fineOffsetX, fineOffsetY);
                }
                else if (cellA + 9 == cellB && cellA + 10 == cellC)
                {
                    polygonPoints = new Vector2[] { new Vector2(0, offsetY), new Vector2(offsetX, offsetY), new Vector2(offsetX, -offsetY), new Vector2(-offsetX, -offsetY), new Vector2(-offsetX, 0), new Vector2(0, 0) };
                    polygonOffset = new Vector2(-fineOffsetX, fineOffsetY);
                }
                break;
        }

        if (triggerCollider != null) triggerCollider.size = boxSize;
        if (cameraConfiner != null)
        {
            cameraConfiner.SetPath(0, polygonPoints);
            if (cell.roomShape != RoomShape.LShape) cameraConfiner.offset = new Vector2(0, verticalAdjustment);
            else cameraConfiner.offset = new Vector2(polygonOffset.x, polygonOffset.y + verticalAdjustment);
        }
    }

    public void SetupOneByOne(Cell cell, int[] floorplan, List<Cell> cellList)
    {
        var currentCell = cell.cellList[0];
        float hOffset = (RoomManager.instance.offsetX / 2f) - doorInset;
        float vOffset = (RoomManager.instance.offsetY / 2f) - doorInset;

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
        float hOffset = (RoomManager.instance.offsetX / 2f) - doorInset;
        float vOuterOffset = RoomManager.instance.offsetY - doorInset;
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
        float hOuterOffset = RoomManager.instance.offsetX - doorInset;
        float hInnerOffset = RoomManager.instance.offsetX / 2f;
        float vOffset = (RoomManager.instance.offsetY / 2f) - doorInset;

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
        float hOuterOffset = RoomManager.instance.offsetX - doorInset;
        float hInnerOffset = RoomManager.instance.offsetX / 2f;
        float vOuterOffset = RoomManager.instance.offsetY - doorInset;
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

        // Calculate Centroid
        float sumX = 0, sumY = 0;
        foreach (int idx in cell.cellList)
        {
            sumX += idx % 10;
            sumY += idx / 10;
        }
        float centroidX = sumX / 3f;
        float centroidY = sumY / 3f;

        foreach (int idx in cell.cellList)
        {
            int gridX = idx % 10;
            int gridY = idx / 10;

            // Calculate local position relative to the room center
            Vector2 localCenter = new Vector2(
                (gridX - centroidX) * offsetX,
                -(gridY - centroidY) * offsetY
            );

            // Check Up (-10)
            if (!cell.cellList.Contains(idx - 10))
            {
                TryPlaceDoor(idx, localCenter + new Vector2(0, v_offset_single - doorInset), EdgeDirection.Up, floorplan, cellList, cell);
            }

            // Check Down (+10)
            if (!cell.cellList.Contains(idx + 10))
            {
                TryPlaceDoor(idx, localCenter + new Vector2(0, -v_offset_single + doorInset), EdgeDirection.Down, floorplan, cellList, cell);
            }

            // Check Left (-1)
            if (!cell.cellList.Contains(idx - 1))
            {
                TryPlaceDoor(idx, localCenter + new Vector2(-h_offset_single + doorInset, 0), EdgeDirection.Left, floorplan, cellList, cell);
            }

            // Check Right (+1)
            if (!cell.cellList.Contains(idx + 1))
            {
                TryPlaceDoor(idx, localCenter + new Vector2(h_offset_single - doorInset, 0), EdgeDirection.Right, floorplan, cellList, cell);
            }
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