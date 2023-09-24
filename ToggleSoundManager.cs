using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSoundManager : MonoBehaviour
{
    public static ToggleSoundManager instance;

    public static float 
        basketOpenLoudness = 0.4f, basketCloseLoudness = 0.5f,
        briefcaseOpenLoudness = 0.3f, briefcaseCloseLoudness = 0.5f,
        shelfOpenLoudness = 0.6f, shelfCloseLoudness = 0.6f,
        slidingDoorsLoudness = 0.5f,
        closetOpenLoudness = 0.7f, closetCloseLoudness = 0.8f,
        drawerOpenLoudness = 0.5f, drawerCloseLoudness = 0.5f,
        ironMaidenOpenLoudness = 0.8f, ironMaidenCloseLoudness = 1f,
        pianoOpenLoudness = 0.4f, pianoCloseLoudness = 0.7f;

    public AudioClip basketOpen, basketClose, briefcaseOpen, briefcaseClose, shelfOpen, shelfClose, slidingDoors,
        closetOpen, closetClose, drawerOpen, drawerClose, ironMaidenOpen, ironMaidenClose, pianoOpen, pianoClose;

    void Start()
    {
        instance = this;
    }
}
