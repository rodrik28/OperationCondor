using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 14f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private int damage = 1;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(float direction)
    {
        rb.linearVelocity =
            new Vector2(speed * direction, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            return;

        EnemyHealth enemy =
            other.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            enemy.RecibirDanio(damage);
        }

        Destroy(gameObject);
    }
}