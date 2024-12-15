using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerAudioController : MonoBehaviour
{
    // Audio
    public AudioSource BackgroundMusic;

    void Start()
    {
        BackgroundMusic = GameObject.Find("AudioManager/WorkerBackgroundMusic")?.GetComponent<AudioSource>();
    }
}
