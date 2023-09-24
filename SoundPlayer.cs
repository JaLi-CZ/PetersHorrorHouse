using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public static SoundPlayer instance;

    public AudioClip[] footstepSounds;
    public AudioClip keyCollectSound, killSound, newNightSound;
    public AudioClip guillotineCutscene, boxingCutscene, ironMaidenCutscene, electricChairCutscene;
    public float maxFootstepVolume = 0.3f;

    private AudioSource audioSource;
    private int lastIdx = -1;

    void Start()
    {
        instance = this;

        audioSource = GetComponent<AudioSource>();
        audioSource.volume = Settings.current.soundVolume / 100f;
    }

    void Update()
    {
        if (PauseManager.paused)
        {
            audioSource.Pause();
            return;
        }
        else
        {
            audioSource.UnPause();
        }
        audioSource.volume = Settings.current.soundVolume / 100f;
    }


    public void Footstep()
    {
        int idx;
        while ((idx = Random.Range(0, footstepSounds.Length)) == lastIdx);
        lastIdx = idx;
        float volume = (Random.value * 0.5f + 0.5f);
        audioSource.PlayOneShot(footstepSounds[idx], maxFootstepVolume * volume);
        Petr.HearFootstep(volume);
    }
    public void KeyCollect()
    {
        audioSource.PlayOneShot(keyCollectSound);
    }
    public void Kill()
    {
        audioSource.PlayOneShot(killSound);
        Petr.PlayRandomDeathVoice();
    }
    public void NewNight()
    {
        audioSource.PlayOneShot(newNightSound);
    }

    public void GuillotineCutscene()
    {
        audioSource.PlayOneShot(guillotineCutscene);
    }
    public void BoxingCutscene()
    {
        audioSource.PlayOneShot(boxingCutscene);
    }
    public void IronMaidenCutscene()
    {
        audioSource.PlayOneShot(ironMaidenCutscene);
    }
    public void ElectricChairCutscene()
    {
        audioSource.PlayOneShot(electricChairCutscene);
    }


    public static void PlayFootstep()
    {
        instance.Footstep();
    }
    public static void PlayKeyCollect()
    {
        instance.KeyCollect();
    }
    public static void PlayKill()
    {
        instance.Kill();
    }
    public static void PlayNewNight()
    {
        instance.NewNight();
    }
    
    public static void PlayCutscene(int id)
    {
        switch (id)
        {
            case 1: instance.GuillotineCutscene(); break;
            case 2: instance.BoxingCutscene(); break;
            case 3: instance.IronMaidenCutscene(); break;
            case 4: instance.ElectricChairCutscene(); break;
        }
    }
}
