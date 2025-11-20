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

    [Tooltip("¡IMPORTANTE! Arrastra aquí el objeto PC de la escena que tiene el script TerminalActivator")]
    public TerminalActivator pcTerminal; // <-- CAMBIO CLAVE

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
        // Si asignamos el PC manualmente en el inspector
        if (pcTerminal != null)
        {
            // Le pasamos la lista de puzzles al PC
            pcTerminal.puzzleContexts = puzzlesInRoom;
            Debug.Log($"[RoomSetup] Puzzles asignados al PC '{pcTerminal.name}': {puzzlesInRoom.Count}");
        }
        else
        {
            // Intento de respaldo automático (por si se te olvida asignar)
            TerminalActivator activator = GetComponentInChildren<TerminalActivator>();
            if (activator != null)
            {
                activator.puzzleContexts = puzzlesInRoom;
                Debug.Log($"[RoomSetup] PC encontrado automáticamente: {activator.name}");
            }
            else
            {
                Debug.LogError("[RoomSetup] ¡No se encontró ningún PC (TerminalActivator) para asignar los puzzles! Arrástralo al Inspector.");
            }
        }
    }
}