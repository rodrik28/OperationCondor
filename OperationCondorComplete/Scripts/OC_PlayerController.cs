using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Animator))]
public class OC_PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] float moveSpeed = 7f;
    [SerializeField] float jumpForce = 12f;
    [SerializeField] float coyoteTime = .12f;
    [SerializeField] float jumpBuffer = .12f;
    [Header("Combate")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float fireCooldown = .18f;
    [Header("Suelo")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundRadius = .18f;
    [SerializeField] LayerMask groundLayer;

    Rigidbody2D rb; SpriteRenderer sr; Animator anim;
    float input, coyote, buffer, nextShot; bool facingRight = true;

    void Awake(){ rb=GetComponent<Rigidbody2D>(); sr=GetComponent<SpriteRenderer>(); anim=GetComponent<Animator>(); }
    void Update()
    {
        if (Keyboard.current == null) return;
        input = 0;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input=-1;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input=1;
        if (input != 0){ facingRight = input > 0; sr.flipX = !facingRight; }

        bool grounded = IsGrounded();
        coyote = grounded ? coyoteTime : coyote - Time.deltaTime;
        if (Keyboard.current.spaceKey.wasPressedThisFrame) buffer = jumpBuffer; else buffer -= Time.deltaTime;
        if (buffer > 0 && coyote > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            buffer=0; coyote=0; OC_AudioManager.Instance?.Jump();
        }
        if (Keyboard.current.spaceKey.wasReleasedThisFrame && rb.linearVelocity.y > 0) rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y*.5f);

        if ((Keyboard.current.jKey.wasPressedThisFrame || Keyboard.current.kKey.wasPressedThisFrame) && Time.time >= nextShot) Shoot();

        anim.SetFloat("Speed", Mathf.Abs(input));
        anim.SetBool("Grounded", grounded);
        anim.SetFloat("YVelocity", rb.linearVelocity.y);
    }
    void FixedUpdate(){ rb.linearVelocity = new Vector2(input*moveSpeed, rb.linearVelocity.y); }
    bool IsGrounded()=> groundCheck != null && Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    void Shoot()
    {
        nextShot = Time.time + fireCooldown;
        anim.SetTrigger("Shoot");
        Vector3 p = firePoint != null ? firePoint.position : transform.position;
        p.x = transform.position.x + (facingRight ? .62f : -.62f);
        var go = Instantiate(bulletPrefab, p, Quaternion.identity);
        var b = go.GetComponent<OC_Bullet>(); if (b != null) b.SetDirection(facingRight ? 1 : -1);
        OC_AudioManager.Instance?.Shoot();
    }
    public void HurtAnim(){ anim.SetTrigger("Hurt"); }
    public void DeadAnim(){ anim.SetBool("Dead", true); }
    public void Configure(GameObject bullet, Transform fire, Transform check, LayerMask layer){ bulletPrefab=bullet; firePoint=fire; groundCheck=check; groundLayer=layer; }
}
