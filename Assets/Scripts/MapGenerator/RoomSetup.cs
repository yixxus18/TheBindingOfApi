using UnityEngine;
using System.Collections.Generic;

public class RoomSetup : MonoBehaviour
{
    [System.Serializable]
    public class RoomObjectiveConfig
    {
        public string description;
        public ObjectiveType type;
        public int requiredAmount = 1;

        [Header("Referencias")]
        public ItemSO itemTarget;
        public RoomPuzzleSO puzzleTarget;
        public string manualTargetId;
    }

    [Header("Configuración de la Sala")]
    public List<RoomObjectiveConfig> objectivesConfiguration;

    [Header("Puzzles y Terminal")]
    [Tooltip("Arrastra aquí los puzzles que se pueden resolver en esta sala")]
    public List<RoomPuzzleSO> puzzlesInRoom;

    [Tooltip("HUB: Arrástralo manual. NIVEL 1: Déjalo vacío (se asigna solo).")]
    public TerminalActivator pcTerminal;

    private void Start()
    {
        ConfigurarObjetivos();
        ConfigurarPuzzleEnTerminal();
    }

    private void ConfigurarObjetivos()
    {
        if (DungeonObjectiveManager.instance == null || objectivesConfiguration.Count == 0) return;

        List<DungeonObjective> finalObjectives = new List<DungeonObjective>();

        foreach (var config in objectivesConfiguration)
        {
            DungeonObjective newObj = new DungeonObjective
            {
                description = config.description,
                type = config.type,
                requiredAmount = config.requiredAmount,
                currentAmount = 0,
                isCompleted = false
            };

            switch (config.type)
            {
                case ObjectiveType.CollectItem:
                    if (config.itemTarget != null) newObj.targetId = config.itemTarget.itemID.ToString();
                    break;
                case ObjectiveType.SolvePuzzle:
                    if (config.puzzleTarget != null) newObj.targetId = config.puzzleTarget.puzzleID.ToString();
                    break;
                case ObjectiveType.KillBoss:
                default:
                    newObj.targetId = config.manualTargetId;
                    break;
            }
            finalObjectives.Add(newObj);
        }

        DungeonObjectiveManager.instance.SetDungeonObjectives(finalObjectives);
    }

    private void ConfigurarPuzzleEnTerminal()
    {
        if (pcTerminal == null)
        {
            pcTerminal = FindFirstObjectByType<TerminalActivator>();
        }
        if (pcTerminal != null)
        {
            pcTerminal.puzzleContexts = puzzlesInRoom;
            Debug.Log($"[RoomSetup] Puzzles asignados al PC '{pcTerminal.name}' existente en escena.");
        }
    }

    public List<RoomPuzzleSO> GetPuzzles()
    {
        return puzzlesInRoom;
    }
}