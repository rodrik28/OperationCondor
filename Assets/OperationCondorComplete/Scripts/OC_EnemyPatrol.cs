using UnityEngine;
[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Animator))]
public class OC_EnemyPatrol : MonoBehaviour
{
    [SerializeField] float speed=2.1f, range=2.5f, damageCooldown=.8f;
    float startX, dir=1, nextDamage; SpriteRenderer sr; Animator anim;
    void Start(){ startX=transform.position.x; sr=GetComponent<SpriteRenderer>(); anim=GetComponent<Animator>(); anim.SetFloat("Speed",1); }
    void FixedUpdate()
    {
        if(Mathf.Abs(transform.position.x-startX)>=range) dir=-Mathf.Sign(transform.position.x-startX);
        var rb=GetComponent<Rigidbody2D>(); rb.linearVelocity=new Vector2(dir*speed,rb.linearVelocity.y); sr.flipX=dir<0;
    }
    void OnCollisionStay2D(Collision2D c)
    {
        var p=c.gameObject.GetComponent<OC_PlayerHealth>();
        if(p!=null && Time.time>=nextDamage){ nextDamage=Time.time+damageCooldown; p.TakeDamage(1); }
    }
}
