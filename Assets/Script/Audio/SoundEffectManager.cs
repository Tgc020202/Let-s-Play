using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public AudioClip buttonClickedSoundEffect;
    public AudioClip catchSoundEffect;
    public AudioClip caughtSoundEffect;
    public AudioClip redButtonSoundEffect;
    public AudioClip greenButtonSoundEffect;
    public AudioClip runButtonSoundEffect;
    public AudioClip missionSucceedSoundEffect;
    public AudioClip missionUnsucceedSoundEffect;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keeps the SoundManager persistent across scenes
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
