using UnityEngine;

public class MinimapIcon : MonoBehaviour
{
    public SpriteRenderer iconRenderer;
    public Color hiddenColor = new Color(0, 0, 0, 0);
    public Color visitedColor = Color.gray;
    public Color currentColor = Color.white;

    private void Awake()
    {
        if (iconRenderer == null) iconRenderer = GetComponent<SpriteRenderer>();
        SetState(RoomState.Hidden);
    }

    public enum RoomState { Hidden, Visited, Current }

    public void SetState(RoomState state)
    {
        switch (state)
        {
            case RoomState.Hidden:
                iconRenderer.color = hiddenColor;
                break;
            case RoomState.Visited:
                iconRenderer.color = visitedColor;
                break;
            case RoomState.Current:
                iconRenderer.color = currentColor;
                break;
        }
    }
}