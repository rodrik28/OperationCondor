using UnityEngine;
public class OC_Goal : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other){ if(other.GetComponent<OC_PlayerController>()!=null) OC_GameManager.Instance?.Win(); }
}
