using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [Header("Disparo")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Configuración")]
    [SerializeField] private float fireCooldown = 0.20f;

    private SpriteRenderer spriteRenderer;
    private float nextFireTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.jKey.wasPressedThisFrame &&
            Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    private void Shoot()
    {
        float direction =
            spriteRenderer.flipX ? -1f : 1f;

        Vector3 position = firePoint.position;

        position.x =
            transform.position.x +
            (0.6f * direction);

        GameObject bullet =
            Instantiate(
                bulletPrefab,
                position,
                Quaternion.identity
            );

        Bullet bulletScript =
            bullet.GetComponent<Bullet>();

        bulletScript.SetDirection(direction);
    }
}