using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float distance = 3f;

    private Vector3 startPosition;
    private bool movingRight = true;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        float direction = movingRight ? 1f : -1f;

        transform.Translate(
            Vector2.right * direction * speed * Time.deltaTime
        );

        if (transform.position.x >= startPosition.x + distance)
        {
            movingRight = false;

            if (spriteRenderer != null)
                spriteRenderer.flipX = true;
        }
        else if (transform.position.x <= startPosition.x - distance)
        {
            movingRight = true;

            if (spriteRenderer != null)
                spriteRenderer.flipX = false;
        }
    }
}