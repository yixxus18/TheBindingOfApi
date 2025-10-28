using System.Collections.Generic;
using UnityEngine;

public class MinimapManager : MonoBehaviour
{
    public static MinimapManager instance;

    private Dictionary<int, MinimapIcon> minimapIcons = new Dictionary<int, MinimapIcon>();
    private MinimapIcon currentRoomIcon;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void RegisterMinimapIcon(int cellIndex, MinimapIcon icon)
    {
        if (!minimapIcons.ContainsKey(cellIndex))
        {
            minimapIcons.Add(cellIndex, icon);
        }
    }

    public void OnPlayerEnterRoom(int cellIndex)
    {
        if (minimapIcons.TryGetValue(cellIndex, out MinimapIcon newRoomIcon))
        {
            if (currentRoomIcon != null && currentRoomIcon != newRoomIcon)
            {
                currentRoomIcon.SetState(MinimapIcon.RoomState.Visited);
            }

            newRoomIcon.SetState(MinimapIcon.RoomState.Current);
            currentRoomIcon = newRoomIcon;
        }
    }

    public void ClearMap()
    {
        foreach (var icon in minimapIcons.Values)
        {
            if (icon != null) Destroy(icon.gameObject);
        }
        minimapIcons.Clear();
        currentRoomIcon = null;
    }
}