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

public class Room : MonoBehaviour
{
    public void SetupRoom(Cell currentCell)
    {
        if (currentCell.roomType == RoomType.Secret) return;

        var floorplan = MapGenerator.instance.getFloorPlan;
        var cellList = MapGenerator.instance.getSpawnedCells;

        switch (currentCell.roomShape)
        {
            case RoomShape.OneByOne:
                SetupOneByOne(currentCell, floorplan, cellList);
                break;

                // Nota: La lógica para salas más grandes (1x2, 2x2) se complica.
                // Por ahora, nos centraremos en que las salas 1x1 funcionen perfectamente.
                // Puedes añadir los otros casos más adelante si los necesitas.
        }
    }

    public void SetupOneByOne(Cell cell, int[] floorplan, List<Cell> cellList)
    {
        var currentCell = cell.cellList[0];

        // Obtener los tamaños del RoomManager para que las puertas siempre estén bien posicionadas
        float roomHalfWidth = RoomManager.instance.offsetX / 2f;
        float roomHalfHeight = RoomManager.instance.offsetY / 2f;

        TryPlaceDoor(currentCell, new Vector2(0, roomHalfHeight), EdgeDirection.Up, floorplan, cellList, cell);
        TryPlaceDoor(currentCell, new Vector2(0, -roomHalfHeight), EdgeDirection.Down, floorplan, cellList, cell);
        TryPlaceDoor(currentCell, new Vector2(-roomHalfWidth, 0), EdgeDirection.Left, floorplan, cellList, cell);
        TryPlaceDoor(currentCell, new Vector2(roomHalfWidth, 0), EdgeDirection.Right, floorplan, cellList, cell);
    }

    // El resto de tu código de puertas puede permanecer igual.
    // Solo he copiado las funciones necesarias para que funcione.
    private void TryPlaceDoor(int fromIndex, Vector2 positionOffset, EdgeDirection direction, int[] floorplan, List<Cell> cellList, Cell currentCell)
    {
        int neighbourIndex = fromIndex + GetOffset(direction);

        if (neighbourIndex < 0 || neighbourIndex >= floorplan.Length) return;
        if (floorplan[neighbourIndex] != 1) return;

        var foundCell = cellList.FirstOrDefault(x => x.cellList.Contains(neighbourIndex));
        if (foundCell == null || foundCell.roomType == RoomType.Secret) return;

        var door = Instantiate(RoomManager.instance.doorPrefab, transform);
        float doorInset = 0.5f; // Ajusta este valor si la puerta aparece muy dentro o fuera

        // Ajusta la posición para que quede exactamente en el borde
        switch (direction)
        {
            case EdgeDirection.Up: positionOffset.y -= doorInset; break;
            case EdgeDirection.Down: positionOffset.y += doorInset; break;
            case EdgeDirection.Left: positionOffset.x += doorInset; break;
            case EdgeDirection.Right: positionOffset.x -= doorInset; break;
        }
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