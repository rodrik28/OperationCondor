using UnityEngine;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            Debug.Log("META ALCANZADA");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Victoria();
            }
        }
    }
}