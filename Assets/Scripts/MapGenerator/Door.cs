using UnityEngine;

public class Door : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public void SetDoorSprite(Sprite door, EdgeDirection direction)
    {
        spriteRenderer.sprite = door;
        switch (direction)
        {
            case EdgeDirection.Up:
                transform.rotation = Quaternion.identity;
                break;

            case EdgeDirection.Down:
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;

            case EdgeDirection.Left:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;

            case EdgeDirection.Right:
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
        }
    }
}
