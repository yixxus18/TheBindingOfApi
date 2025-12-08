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

    private System.Collections.IEnumerator Start()
    {
        ConfigurarObjetivos();
        // Esperamos un poco más para asegurar que todas las salas y PCs se hayan generado
        yield return new WaitForSeconds(0.5f);
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
        // Verificar primero si tenemos puzzles configurados
        if (puzzlesInRoom == null || puzzlesInRoom.Count == 0)
        {
            Debug.LogWarning("[RoomSetup] No hay puzzles configurados en puzzlesInRoom. Asegúrate de asignarlos en el Inspector.");
            return;
        }

        if (pcTerminal == null)
        {
            pcTerminal = FindFirstObjectByType<TerminalActivator>();
            
            if (pcTerminal == null)
            {
                Debug.LogWarning("[RoomSetup] No se encontró ningún TerminalActivator en la escena. ¿Se generó una sala tipo Puzzle?");
                return;
            }
        }
        
        // Asignar los puzzles a la terminal
        pcTerminal.puzzleContexts = puzzlesInRoom;
        Debug.Log($"[RoomSetup] ✓ {puzzlesInRoom.Count} puzzle(s) asignados correctamente al PC '{pcTerminal.name}'.");
    }

    public List<RoomPuzzleSO> GetPuzzles()
    {
        return puzzlesInRoom;
    }
}