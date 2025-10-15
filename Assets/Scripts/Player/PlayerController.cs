using UnityEngine;
using System.Collections;
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movementInput;
    public bool canMove = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!canMove)
        {
            movementInput = Vector2.zero;
            return;
        }
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        movementInput = new Vector2(moveX, moveY).normalized;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movementInput * moveSpeed;
    }

    private bool isTransitioning = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTransitioning) return;

        DoorTrigger door = other.GetComponent<DoorTrigger>();

        if (door != null)
        {
            StartCoroutine(TransitionCooldown());
            float playerOffsetFromWall = 3.0f;
            switch (door.doorDirection)
            {
                case EdgeDirection.Up:
                    transform.position += new Vector3(0, playerOffsetFromWall, 0);
                    break;
                case EdgeDirection.Down:
                    transform.position -= new Vector3(0, playerOffsetFromWall, 0);
                    break;
                case EdgeDirection.Left:
                    transform.position -= new Vector3(playerOffsetFromWall, 0, 0);
                    break;
                case EdgeDirection.Right:
                    transform.position += new Vector3(playerOffsetFromWall, 0, 0);
                    break;
            }
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 0.1f);
            foreach (var hitCollider in hitColliders)
            {
                PolygonCollider2D roomBounds = hitCollider.GetComponent<PolygonCollider2D>();
                if (roomBounds != null)
                {
                    break;
                }
            }
        }
    }

    private IEnumerator TransitionCooldown()
    {
        isTransitioning = true;
        yield return new WaitForSeconds(0.5f);
        isTransitioning = false;
    }
}