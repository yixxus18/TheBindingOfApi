using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct PuzzleEventBinding
{
    public int puzzleID;
    public UnityEvent onSolved;
}

public class PuzzleEventListener : MonoBehaviour
{
    public PuzzleEventBinding[] puzzleEvents;

    private void OnEnable()
    {
        ApiTerminalManager.OnAnyPuzzleSolved += HandlePuzzleSolved;
    }

    private void OnDisable()
    {
        ApiTerminalManager.OnAnyPuzzleSolved -= HandlePuzzleSolved;
    }

    private void HandlePuzzleSolved(int solvedPuzzleID)
    {
        foreach (var binding in puzzleEvents)
        {
            if (binding.puzzleID == solvedPuzzleID)
            {
                binding.onSolved.Invoke();
            }
        }
    }
}