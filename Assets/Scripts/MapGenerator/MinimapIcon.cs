using UnityEngine;

public class MinimapIcon : MonoBehaviour
{
    public SpriteRenderer iconRenderer;

    // Ya no dependemos del Alfa del HiddenColor para ocultar,
    // pero mantenemos la variable por si acaso.
    public Color hiddenColor = new Color(0, 0, 0, 0);
    public Color visitedColor = Color.gray;
    public Color currentColor = Color.white;

    private void Awake()
    {
        if (iconRenderer == null)
            iconRenderer = GetComponent<SpriteRenderer>();

        // Aseguramos que empiece oculto
        SetState(RoomState.Hidden);
    }

    public enum RoomState
    {
        Hidden,
        Visited,
        Current
    }

    public void SetState(RoomState state)
    {
        if (iconRenderer == null) return;

        switch (state)
        {
            case RoomState.Hidden:
                // AQUÍ ESTÁ LA CLAVE: Apagamos el renderizado
                iconRenderer.enabled = false;
                break;

            case RoomState.Visited:
                // Encendemos y ponemos color gris
                iconRenderer.enabled = true;
                iconRenderer.color = visitedColor;
                break;

            case RoomState.Current:
                // Encendemos y ponemos color blanco
                iconRenderer.enabled = true;
                iconRenderer.color = currentColor;
                break;
        }
    }

    public void SetRotation(float angle)
    {
        transform.localRotation = Quaternion.Euler(0, 0, angle);
    }
}