using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // UI Components
    private AudioSource audioSource;

    // Audio
    public AudioClip buttonClickedSoundEffect;
    public AudioClip catchSoundEffect;
    public AudioClip caughtSoundEffect;
    public AudioClip redButtonSoundEffect;
    public AudioClip greenButtonSoundEffect;
    public AudioClip runButtonSoundEffect;
    public AudioClip missionSucceedSoundEffect;
    public AudioClip missionUnsucceedSoundEffect;

    // Instance
    public static SoundManager Instance { get; private set; }

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

        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
