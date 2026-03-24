using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM")]
    [SerializeField] private AudioClip titleBGM;
    [SerializeField] private AudioClip gameBGM;

    [Header("SFX")]
    [SerializeField] private AudioClip normalMoleSFX;
    [SerializeField] private AudioClip trapMoleSFX;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayTitleBGM()
    {
        PlayBGM(titleBGM);
    }

    public void PlayGameBGM()
    {
        PlayBGM(gameBGM);
    }

    public void PlayNormalMoleSFX()
    {
        PlaySFX(normalMoleSFX);
    }

    public void PlayTrapMoleSFX()
    {
        PlaySFX(trapMoleSFX);
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null) return;
        if (clip == null) return;

        //if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource == null) return;

        bgmSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null) return;
        if (clip == null) return;

        sfxSource.PlayOneShot(clip);
    }
}