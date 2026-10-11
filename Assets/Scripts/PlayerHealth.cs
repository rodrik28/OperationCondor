using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int vidaMaxima = 3;

    private int vidaActual;

    private void Start()
    {
        vidaActual = vidaMaxima;
        Debug.Log("VIDA: " + vidaActual);
    }

    public void RecibirDanio(int cantidad)
    {
        vidaActual -= cantidad;

        Debug.Log("VIDA: " + vidaActual);

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        GameManager.Instance.GameOver();
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            RecibirDanio(1);
        }
    }
}