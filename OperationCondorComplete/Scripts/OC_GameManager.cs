using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class OC_GameManager : MonoBehaviour
{
    public static OC_GameManager Instance { get; private set; }
    [SerializeField] GameObject gameOverPanel, victoryPanel, pausePanel;
    [SerializeField] Image healthFill;
    [SerializeField] Text statusText;
    bool ended, paused;
    void Awake(){ Instance=this; Time.timeScale=1f; }
    void Update()
    {
        if(ended || Keyboard.current==null) return;
        if(Keyboard.current.escapeKey.wasPressedThisFrame){ paused=!paused; Time.timeScale=paused?0:1; if(pausePanel!=null) pausePanel.SetActive(paused); }
    }
    public void UpdateHealth(int current,int max){ if(healthFill!=null) healthFill.fillAmount=(float)current/max; if(statusText!=null) statusText.text=$"VIDA  {current}/{max}     J: DISPARAR     ESPACIO: SALTAR"; }
    public void GameOver(){ ended=true; Time.timeScale=0; if(gameOverPanel!=null) gameOverPanel.SetActive(true); OC_AudioManager.Instance?.Lose(); }
    public void Win(){ ended=true; Time.timeScale=0; if(victoryPanel!=null) victoryPanel.SetActive(true); OC_AudioManager.Instance?.Win(); }
    public void Restart(){ Time.timeScale=1; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void Menu(){ Time.timeScale=1; SceneManager.LoadScene("MainMenu_Complete"); }
    public void Resume(){ paused=false; Time.timeScale=1; if(pausePanel!=null) pausePanel.SetActive(false); }
    public void Configure(GameObject go,GameObject win,GameObject pause,Image fill,Text status){ gameOverPanel=go;victoryPanel=win;pausePanel=pause;healthFill=fill;statusText=status; }
}
