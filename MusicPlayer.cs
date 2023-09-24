using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Random = System.Random;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer instance;

    public AudioClip[] music;
    public AudioClip cutsceneMusic;
    public float wait = 2.3f;
    public float maxMusicVolume = 0.5f;

    private float animTime = 0f, animDuration;
    private bool animateStop = false;

    private float time = 0f;
    private AudioSource audioSource;
    private Random r = new Random();
    private bool hasFocus = true;
    private bool stopped = false;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (stopped)
        {
            audioSource.volume = 0f;
            return;
        }

        time += Time.deltaTime;
        if (time < wait)
        {
            audioSource.volume = 0f;
            return;
        }

        float musicVolume = Settings.current.musicVolume / 100f;
        audioSource.volume = musicVolume * maxMusicVolume;

        if (animateStop)
        {
            animTime += Time.deltaTime;
            if(animTime > animDuration)
            {
                animTime = 0f;
                animateStop = false;
                audioSource.Stop();
                stopped = true;
            } else
            {
                audioSource.volume *= Animation.Offset(animDuration, 0f, animTime);
            }
        }

        if (!audioSource.isPlaying && hasFocus) PlayMusic();
    }

    void OnApplicationFocus(bool focused)
    {
        hasFocus = focused;
    }

    private void PlayMusic()
    {
        audioSource.clip = music[r.Next(music.Length)];
        audioSource.Play();
    }

    public static void Play()
    {
        instance.animTime = 0f;
        instance.animateStop = false;
        instance.time = 0f;
        instance.stopped = false;
        instance.audioSource.clip = null;
    }

    public static void PlayCutsceneMusic()
    {
        instance.time = instance.wait;
        instance.stopped = false;
        instance.audioSource.clip = instance.cutsceneMusic;
        instance.audioSource.Play();
    }

    public static void Stop(float duration = 0.5f)
    {
        instance.animateStop = true;
        instance.animDuration = duration;
        instance.animTime = 0f;
    }
}
