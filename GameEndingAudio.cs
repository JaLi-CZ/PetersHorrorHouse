using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndingAudio : MonoBehaviour
{
    public static GameEndingAudio instance;

    public AudioClip youWinSound, youLoseSound, runAwaySound, cinematicHitSound;
    private AudioSource audioSource;

    void Start()
    {
        instance = this;

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        audioSource.volume = Settings.current.soundVolume / 100f;
        if(PauseManager.paused)
        {
            audioSource.Pause();
        } else
        {
            audioSource.UnPause();
        }
    } 

    public static void PlayYouWinSound()
    {
        PlaySound(instance.youWinSound);
    }

    public static void PlayYouLoseSound()
    {
        PlaySound(instance.youLoseSound);
    }

    public static void PlayRunAwaySound()
    {
        PlaySound(instance.runAwaySound);
    }

    public static void PlayCinematicHitSound()
    {
        PlaySound(instance.cinematicHitSound);
    }

    public static void PlaySound(AudioClip audioClip)
    {
        instance.audioSource.PlayOneShot(audioClip);
    }
}
