using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAudioController : MonoBehaviour
{
    // Audio
    public AudioSource BackgroundMusic;

    void Start()
    {
        BackgroundMusic = GameObject.Find("AudioManager/BossBackgroundMusic")?.GetComponent<AudioSource>();
    }
}
