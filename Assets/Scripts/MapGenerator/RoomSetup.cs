using UnityEngine;
using System.Collections.Generic;

public class RoomSetup : MonoBehaviour
{
    public RoomPuzzleSO puzzleDeEstaSala;
    public List<DungeonObjective> objetivosDeEstaSala;
    private bool isSetup = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isSetup && collision.CompareTag("Player"))
        {
            ApiTerminalManager.instance.currentRoomPuzzle = puzzleDeEstaSala;
            DungeonObjectiveManager.instance.SetDungeonObjectives(objetivosDeEstaSala);
            isSetup = true; 
        }
    }
}