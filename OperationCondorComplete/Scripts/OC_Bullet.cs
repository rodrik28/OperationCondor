using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
public class OC_Bullet : MonoBehaviour
{
    [SerializeField] float speed=15f; [SerializeField] int damage=1; [SerializeField] float life=2f;
    Rigidbody2D rb;
    void Awake(){ rb=GetComponent<Rigidbody2D>(); }
    void Start(){ Destroy(gameObject,life); }
    public void SetDirection(float dir){ rb.linearVelocity=new Vector2(speed*dir,0); transform.localScale=new Vector3(Mathf.Abs(transform.localScale.x)*dir,transform.localScale.y,1); }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.GetComponent<OC_PlayerController>()!=null) return;
        var e=other.GetComponent<OC_EnemyHealth>();
        if(e!=null){ e.TakeDamage(damage); OC_VFX.Burst(transform.position,new Color(1f,.65f,.15f),12); Destroy(gameObject); return; }
        if(!other.isTrigger) Destroy(gameObject);
    }
}
