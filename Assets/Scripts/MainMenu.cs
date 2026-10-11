using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Jugar()
    {
        SceneManager.LoadScene("Level01");
    }

    public void Salir()
    {
        Application.Quit();
    }
}