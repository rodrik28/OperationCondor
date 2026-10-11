using UnityEngine;
using UnityEngine.SceneManagement;
public class OC_MainMenu : MonoBehaviour
{
    [SerializeField] GameObject optionsPanel;
    public void Play(){ OC_AudioManager.Instance?.Click(); SceneManager.LoadScene("Level01_Complete"); }
    public void Options(){ OC_AudioManager.Instance?.Click(); if(optionsPanel!=null) optionsPanel.SetActive(!optionsPanel.activeSelf); }
    public void Quit(){ Application.Quit(); }
    public void SetVolume(float v){ AudioListener.volume=v; }
    public void Configure(GameObject p){ optionsPanel=p; }
}
