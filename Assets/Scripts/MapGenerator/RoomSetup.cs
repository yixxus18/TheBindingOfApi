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

        [Header("Referencias (Usar una según el tipo)")]
        [Tooltip("Arrastra aquí el ItemSO si el objetivo es 'Collect Item'")]
        public ItemSO itemTarget;

        [Tooltip("Arrastra aquí el RoomPuzzleSO si el objetivo es 'Solve Puzzle'")]
        public RoomPuzzleSO puzzleTarget;

        [Tooltip("Usa esto solo para Jefes o IDs manuales")]
        public string manualTargetId;
    }

    [Header("Configuración de la Sala")]
    public List<RoomObjectiveConfig> objectivesConfiguration;

    [Header("Puzzle Principal de la Sala")]
    public RoomPuzzleSO puzzleDeEstaSala;

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

            // Lógica inteligente para obtener el ID correcto
            switch (config.type)
            {
                case ObjectiveType.CollectItem:
                    if (config.itemTarget != null)
                    {
                        // Convierte el ID numérico del item a string automáticamente
                        newObj.targetId = config.itemTarget.itemID.ToString();
                    }
                    else
                    {
                        Debug.LogWarning($"Objetivo de Item en {gameObject.name} no tiene ItemSO asignado.");
                    }
                    break;

                case ObjectiveType.SolvePuzzle:
                    if (config.puzzleTarget != null)
                    {
                        newObj.targetId = config.puzzleTarget.puzzleID;
                    }
                    else
                    {
                        Debug.LogWarning($"Objetivo de Puzzle en {gameObject.name} no tiene RoomPuzzleSO asignado.");
                    }
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
        if (puzzleDeEstaSala == null) return;

        // Buscamos el TerminalActivator en la sala (el PC)
        TerminalActivator activator = GetComponentInChildren<TerminalActivator>();

        if (activator != null)
        {
            activator.puzzleContext = puzzleDeEstaSala;
        }
        else
        {
            // Fallback por si hay un manager suelto (menos recomendado con el nuevo sistema)
            ApiTerminalManager roomTerminal = FindFirstObjectByType<ApiTerminalManager>();
            if (roomTerminal != null && roomTerminal.isCentralTerminal)
            {
                roomTerminal.OpenTerminal(puzzleDeEstaSala);
            }
        }
    }
}