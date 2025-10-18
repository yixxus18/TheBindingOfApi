using UnityEngine;
using System.Collections.Generic;

public class RoomSetup : MonoBehaviour
{

    [Header("Objetivos De Esta Sala")]
    public List<DungeonObjective> objectives;

    [Header("Puzzle De Esta Sala")]
    public RoomPuzzleSO puzzleDeEstaSala;

    private void Start()
    {
        if (DungeonObjectiveManager.instance != null && objectives.Count > 0)
        {
            DungeonObjectiveManager.instance.SetDungeonObjectives(objectives);
        }

        if (ApiTerminalManager.instance != null && puzzleDeEstaSala != null)
        {
            ApiTerminalManager.instance.currentRoomPuzzle = puzzleDeEstaSala;
        }
    }
}