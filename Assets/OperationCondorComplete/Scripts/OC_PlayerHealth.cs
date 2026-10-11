using UnityEngine;
public class OC_PlayerHealth : MonoBehaviour
{
    [SerializeField] int maxHealth = 5;
    int health; bool dead;
    void Start(){ health=maxHealth; OC_GameManager.Instance?.UpdateHealth(health,maxHealth); }
    public void TakeDamage(int amount)
    {
        if(dead) return; health=Mathf.Max(0,health-amount); OC_GameManager.Instance?.UpdateHealth(health,maxHealth);
        GetComponent<OC_PlayerController>()?.HurtAnim(); OC_AudioManager.Instance?.Hit(); Camera.main?.GetComponent<OC_CameraFollow>()?.Shake(.17f,.16f); OC_VFX.Burst(transform.position,new Color(.2f,.9f,1f),18);
        if(health<=0){ dead=true; GetComponent<OC_PlayerController>()?.DeadAnim(); Invoke(nameof(Die),.35f); }
    }
    void Die(){ OC_GameManager.Instance?.GameOver(); gameObject.SetActive(false); }
}
