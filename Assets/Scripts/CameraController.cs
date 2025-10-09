using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;
    public float transitionSpeed = 2f;
    private bool isMoving = false;
    private PlayerController player;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        player = FindFirstObjectByType<PlayerController>();
    }

    public void MoveToNextRoom(Vector3 targetPosition)
    {
        if (!isMoving)
        {
            StartCoroutine(TransitionTo(targetPosition));
        }
    }

    private IEnumerator TransitionTo(Vector3 targetPosition)
    {
        isMoving = true;
        player.canMove = false;
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime);
            elapsedTime += Time.deltaTime * transitionSpeed;
            yield return null;
        }
        transform.position = targetPosition;

        isMoving = false;
        player.canMove = true;
    }
}