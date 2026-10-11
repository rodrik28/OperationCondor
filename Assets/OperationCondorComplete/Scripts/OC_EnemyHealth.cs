using UnityEngine;
public class OC_EnemyHealth : MonoBehaviour
{
    [SerializeField] int maxHealth=3; int health; bool dead;
    void Awake(){ health=maxHealth; }
    public void TakeDamage(int amount)
    {
        if(dead) return; health-=amount; GetComponent<Animator>()?.SetTrigger("Hurt"); OC_AudioManager.Instance?.Hit();
        if(health<=0){ dead=true; GetComponent<Animator>()?.SetBool("Dead",true); GetComponent<Collider2D>().enabled=false; GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero; OC_VFX.Burst(transform.position,new Color(1f,.25f,.08f),28); Destroy(gameObject,.55f); }
    }
}
