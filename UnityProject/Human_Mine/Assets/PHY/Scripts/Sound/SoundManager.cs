using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        if (sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    public void StopSfx()
    {
        if (sfxSource == null)
        {
            return;
        }

        sfxSource.Stop();
    }
}