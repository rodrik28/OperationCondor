using UnityEngine;

public class OC_AudioManager : MonoBehaviour
{
    public static OC_AudioManager Instance { get; private set; }
    [SerializeField] AudioClip shootClip, hitClip, jumpClip, winClip, loseClip, clickClip, musicClip;
    AudioSource musicSource;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = 0.22f;
        if (musicClip != null) { musicSource.clip = musicClip; musicSource.Play(); }
    }

    void PlayOne(AudioClip clip, float volume = 0.65f)
    {
        if (clip != null) AudioSource.PlayClipAtPoint(clip, Camera.main != null ? Camera.main.transform.position : Vector3.zero, volume);
    }
    public void Shoot() => PlayOne(shootClip, 0.45f);
    public void Hit() => PlayOne(hitClip, 0.55f);
    public void Jump() => PlayOne(jumpClip, 0.35f);
    public void Win() => PlayOne(winClip, 0.6f);
    public void Lose() => PlayOne(loseClip, 0.6f);
    public void Click() => PlayOne(clickClip, 0.45f);
}
