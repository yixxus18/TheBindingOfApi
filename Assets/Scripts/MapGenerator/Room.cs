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
    [Header("Component References")]
    public PolygonCollider2D cameraConfiner;
    private BoxCollider2D triggerCollider;

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
        }
    }

    public void SetupRoom(Cell currentCell)
    {
        ResizeColliders();

        if (currentCell.roomType == RoomType.Secret) return;

        var floorplan = MapGenerator.instance.getFloorPlan;
        var cellList = MapGenerator.instance.getSpawnedCells;

        switch (currentCell.roomShape)
        {
            case RoomShape.OneByOne:
                SetupOneByOne(currentCell, floorplan, cellList);
                break;
        }
    }

    // CAMBIO: Lógica de redimensionamiento corregida para usar los valores exactos.
    private void ResizeColliders()
    {
        if (RoomManager.instance == null) return;

        // Usamos los Offsets del RoomManager como la fuente de verdad para el tamaño TOTAL de la sala.
        float totalWidth = RoomManager.instance.offsetX;
        float totalHeight = RoomManager.instance.offsetY;

        // 1. Redimensionar el BoxCollider2D (para detectar al jugador).
        // Este debe tener el tamaño total para que el trigger funcione en toda la sala.
        if (triggerCollider != null)
        {
            triggerCollider.size = new Vector2(totalWidth, totalHeight);
        }

        // 2. Redimensionar el PolygonCollider2D (para los límites de la cámara).
        // Este también debe tener el tamaño total exacto.
        if (cameraConfiner != null)
        {
            float halfWidth = totalWidth / 2f;
            float halfHeight = totalHeight / 2f;

            Vector2[] points = new Vector2[4];
            points[0] = new Vector2(-halfWidth, halfHeight);
            points[1] = new Vector2(halfWidth, halfHeight);
            points[2] = new Vector2(halfWidth, -halfHeight);
            points[3] = new Vector2(-halfWidth, -halfHeight);

            cameraConfiner.points = points;
        }
    }

    // CAMBIO: Ambas puertas ahora se mueven hacia adentro.
    public void SetupOneByOne(Cell cell, int[] floorplan, List<Cell> cellList)
    {
        var currentCell = cell.cellList[0];

        // El offset se calcula sobre el tamaño TOTAL de la sala.
        float roomHalfWidth = RoomManager.instance.offsetX / 2f;
        float roomHalfHeight = RoomManager.instance.offsetY / 2f;

        // Ambas puertas un poco hacia adentro para un mejor efecto visual.
        float verticalDoorOffset = roomHalfHeight - 1.0f;
        float horizontalDoorOffset = roomHalfWidth - 1.0f;

        TryPlaceDoor(currentCell, new Vector2(0, verticalDoorOffset), EdgeDirection.Up, floorplan, cellList, cell);
        TryPlaceDoor(currentCell, new Vector2(0, -verticalDoorOffset), EdgeDirection.Down, floorplan, cellList, cell);
        TryPlaceDoor(currentCell, new Vector2(-horizontalDoorOffset, 0), EdgeDirection.Left, floorplan, cellList, cell);
        TryPlaceDoor(currentCell, new Vector2(horizontalDoorOffset, 0), EdgeDirection.Right, floorplan, cellList, cell);
    }

    private void TryPlaceDoor(int fromIndex, Vector2 positionOffset, EdgeDirection direction, int[] floorplan, List<Cell> cellList, Cell currentCell)
    {
        int neighbourIndex = fromIndex + GetOffset(direction);

        if (neighbourIndex < 0 || neighbourIndex >= floorplan.Length) return;
        if (floorplan[neighbourIndex] != 1) return;

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