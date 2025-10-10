using Unity.Cinemachine;
using UnityEngine;

public class CameraConfinerManager : MonoBehaviour
{
    public static CameraConfinerManager instance;
    private CinemachineConfiner2D confiner;

    void Awake()
    {
        instance = this;
        confiner = FindFirstObjectByType<CinemachineConfiner2D>();
    }

    public void UpdateBounds(PolygonCollider2D newBounds)
    {
        if (confiner == null) return;

        confiner.BoundingShape2D = newBounds;
        if (newBounds != null)
        {
            confiner.InvalidateBoundingShapeCache();
        }
    }
}