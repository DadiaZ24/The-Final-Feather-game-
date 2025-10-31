using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource inGameMusic;
    [Header("Clips")]
    public AudioClip gameOverMusic;
    public AudioClip gameOverVoice;
    public AudioClip gameOverSound;

    void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlayMusic(musicSource.clip);
    }

    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (!clip) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = volume;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (!clip) return;

        sfxSource.PlayOneShot(clip);
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)
            musicSource.Stop();
    }

    public void StopAllSounds()
    {
        musicSource.Stop();
        sfxSource.Stop();
    }

    public void StartGameSounds()
    {
        StopAllSounds();
        PlayMusic(inGameMusic.clip, 0.1f);
    }
}
