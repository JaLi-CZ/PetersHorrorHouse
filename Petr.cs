using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Voice
{
    public static Voice[] voices = null;

    public AudioClip audioClip;
    public readonly int id;
    public readonly bool international;

    public Voice(int id, bool international)
    {
        this.id = id;
        this.international = international;
        LoadByLanguage();
    }

    private void LoadByLanguage()
    {
        if(international)
        {
            if (audioClip == null) audioClip = LoadAudioClip(id + "");
            return;
        }
        audioClip = LoadAudioClip(Translator.currentLanguage + "/" + id);
    }

    public static void LoadVoices()
    {
        if (voices != null) return;
        voices = new Voice[18];
        for (int i = 0; i < voices.Length; i++) voices[i] = new Voice(i, i <= 3);
    }

    public static void SwitchLanguage(string language)
    {
        if (voices == null) return;
        foreach (Voice voice in voices) if(voice != null) voice.LoadByLanguage();
    }

    private static AudioClip LoadAudioClip(string name)
    {
        return Resources.Load<AudioClip>("voice/" + name);
    }
}

public class Petr : MonoBehaviour
{
    public static Petr instance;

    public static readonly string lastCutsceneFile = "last-cutscene.txt";
    public static readonly int IDLE = 0, WALK = 1, FIGHT = 2;
    public static NavMeshAgent agent;
    public static GameObject petrModel, petrBody, player;
    public static float distanceFromPlayer = float.PositiveInfinity;
    public static Light playerLight;
    public static AudioSource audioSource;
    public static int moveStyle;
    public static bool playingCutscene = false;
    public static Vector3 currentCutscenePetrPosition = Vector3.zero;
    public static Quaternion currentCutscenePetrRotation = Quaternion.identity;
    public static GameObject skeleton;
    public static int cutsceneId = -1;
    public static bool maxVolume = false;

    private static Light[] cutsceneLights;
    private static float cutsceneLightIntensity = 0f;
    private static GameObject villagerBody, villagerHead, deadBodyBoxingBag, electricChairSkull;
    private static Camera cutsceneCamera;
    private static Animator animator = null, cutsceneCameraAnimator = null;
    private static int lastTrashtalkId = -1, lastCutsceneId = -1;
    private static int[] 
        deathVoiceIds = new int[] { 0, 1, 3, 4 },
        cutsceneVoiceIds = new int[] { -1, 14, 17, 16, 15 },
        canSeePlayerVoiceIds = new int[] { 11, 12 },
        trashTalkVoiceIds = new int[] { 2, 5, 6, 7, 8, 9, 10 };
    // 1 OK    2 ?    3 ?    4 ?
    private static float[] cutsceneTrashTalkTimes = new float[] { 0f,  4.3f,  8f,    5.3f,  1.6f  };
    private static float[] cutsceneSoundTimes = new float[]     { 0f,  10.1f, 10.7f, 6.3f,  6.4f  };
    private static float[] cutsceneAnimationTimes = new float[] { -1f, 11.6f, -1f,   9f,    -1f   };
    private static float[] cutsceneEndTimes = new float[]       { 0f,  12.2f, 14.1f, 10.3f, 10.8f };
    private float trashtalkTime = 0f, trashTalkDelay = 12f, trashTalkDelayRandomization = 5.5f, currentTrashTalkDelay = 7f;
    private float cutsceneTime = 0f, cutsceneDoneTime = 0f;
    private bool cutsceneTrashTalkPlayed = false, cutsceneAnimationPlayed = false, cutsceneSoundPlayed = false, cutsceneEnded = false,
        cutsceneDone = false;

    public Camera cutsceneCam;
    public GameObject skeletonObj, petrBodyObj;
    public Light[] cutsceneLightsObjs;
    public GameObject villagerBodyObj, villagerHeadObj, deadBodyBoxingBagObj, electricChairSkullObj;

    void Start()
    {
        instance = this;
        maxVolume = false;
        distanceFromPlayer = float.PositiveInfinity;
        playingCutscene = false;
        animator = GetComponent<Animator>();
        cutsceneCamera = cutsceneCam;
        cutsceneCamera.enabled = false;
        skeleton = skeletonObj;
        petrBody = petrBodyObj;
        cutsceneLights = cutsceneLightsObjs;
        villagerBody = villagerBodyObj;
        villagerHead = villagerHeadObj;
        deadBodyBoxingBag = deadBodyBoxingBagObj;
        electricChairSkull = electricChairSkullObj;
        cutsceneCameraAnimator = cutsceneCamera.GetComponent<Animator>();
        audioSource = skeleton.GetComponent<AudioSource>();
        moveStyle = Settings.current.movementStyle;

        Voice.LoadVoices();
    }

    void Update()
    {
        if (PauseManager.paused)
        {
            audioSource.Pause();
            return;
        }
        else audioSource.UnPause();

        if (playingCutscene)
        {
            audioSource.volume = Settings.current.soundVolume / 100f;
            audioSource.spatialBlend = 0f;

            if (cutsceneDone)
            {
                cutsceneDoneTime += Time.deltaTime;
                if (cutsceneDoneTime > 3f)
                {
                    SceneManager.LoadScene("Menu");
                    if (InterstitialAds.instance != null) InterstitialAds.instance.ShowAd();
                }
            }
            else
            {
                cutsceneTime += Time.deltaTime;

                GameOverlay.instance.Update();

                if (!cutsceneTrashTalkPlayed && cutsceneTime > cutsceneTrashTalkTimes[cutsceneId])
                {
                    cutsceneTrashTalkPlayed = true;
                    PlayVoice(cutsceneVoiceIds[cutsceneId]);
                }

                if (!cutsceneSoundPlayed && cutsceneTime > cutsceneSoundTimes[cutsceneId])
                {
                    cutsceneSoundPlayed = true;
                    SoundPlayer.PlayCutscene(cutsceneId);
                }

                if (!cutsceneEnded && cutsceneTime > cutsceneEndTimes[cutsceneId])
                {
                    cutsceneEnded = true;
                    GameOverlay.SetOpacity(1f, new GameObject[] { GameOverlay.instance.blackBackgroundPanel.gameObject });
                    MusicPlayer.Stop(0.1f);
                    cutsceneDone = true;
                }

                if (cutsceneId == 1)
                {
                    if (cutsceneAnimationPlayed) NavAgent.instance.guillotineAnimation.Animate();
                    else
                    {
                        if (cutsceneTime > cutsceneAnimationTimes[cutsceneId])
                        {
                            cutsceneAnimationPlayed = true;
                            NavAgent.instance.guillotineAnimation.Open();
                        } else NavAgent.instance.guillotineAnimation.obj.transform.position = NavAgent.instance.guillotineAnimation.startPosition;
                    }
                }
                else if (cutsceneId == 3)
                {
                    petrBody.GetComponent<Renderer>().enabled = cutsceneTime > 4.3f;
                    cutsceneLights[cutsceneId].intensity = Animation.LinearInterpolation(0, cutsceneLightIntensity, 
                        Animation.EaseOut(Mathf.Min(2f, cutsceneEndTimes[cutsceneId]-cutsceneTime)/2f));
                    if (cutsceneAnimationPlayed)
                    {
                        Animation.ironMaidenDoorLeftAnimation.Animate();
                        Animation.ironMaidenDoorRightAnimation.Animate();
                    }
                    else
                    {
                        if (cutsceneTime > cutsceneAnimationTimes[cutsceneId])
                        {
                            cutsceneAnimationPlayed = true;
                            Animation.ironMaidenDoorLeftAnimation.Close();
                            Animation.ironMaidenDoorRightAnimation.Close();
                        }
                        else
                        {
                            Vector3 leftRot = Animation.ironMaidenDoorLeftAnimation.endRotation, rightRot = Animation.ironMaidenDoorRightAnimation.endRotation;
                            Animation.ironMaidenDoorLeftAnimation.obj.transform.rotation = Quaternion.Euler(leftRot.x, leftRot.y, leftRot.z);
                            Animation.ironMaidenDoorRightAnimation.obj.transform.rotation = Quaternion.Euler(rightRot.x, rightRot.y, rightRot.z);
                        }
                    }
                }
            }
            return;
        }

        float volumeFactor = maxVolume ? 1f : Animation.EaseIn(Mathf.Max(0f, 20f - distanceFromPlayer) / 20f);
        audioSource.volume = Settings.current.soundVolume * volumeFactor / 100f;

        moveStyle = Settings.current.movementStyle;
        if (moveStyle == 4) TargetPlayer();

        trashtalkTime += Time.deltaTime;
        if(trashtalkTime > currentTrashTalkDelay)
        {
            currentTrashTalkDelay = trashTalkDelay + ((Random.value * 2 - 1) * trashTalkDelayRandomization);
            trashtalkTime = 0f;
            PlayRandomTrashTalkVoice();
        }
    }

    public static void SetAnimationState(int state)
    {
        if (playingCutscene) return;
        if(state == IDLE)
        {
            animator.speed = 1f;
            animator.SetBool("walking", false);
            animator.SetBool("fighting", false);
            animator.SetInteger("cutscene", 0);
        } else if(state == WALK)
        {
            animator.SetBool("fighting", false);
            animator.SetBool("walking", true);
            animator.SetInteger("cutscene", 0);
            animator.speed = Settings.current.speed * 1.25f;
        } else if(state == FIGHT)
        {
            animator.speed = 1f;
            animator.SetBool("walking", true);
            animator.SetBool("fighting", true);
            animator.SetInteger("cutscene", 0);
        }
    }

    public static void PlayCutscene(int cutsceneId)
    {
        if (cutsceneId < 1 || cutsceneId > 4 || playingCutscene) return;
        Opener.animateAll = false;
        HintManager.hintEnabled = false;
        Petr.cutsceneId = cutsceneId;
        playingCutscene = true;
        animator.speed = 1f;
        cutsceneLightIntensity = cutsceneLights[cutsceneId].intensity;
        switch (cutsceneId)
        {
            case 1:
                currentCutscenePetrPosition = new Vector3(2.3936f, 0.214037f, -5.94347f);
                currentCutscenePetrRotation = Quaternion.Euler(0, -56f, 0f);
                villagerBody.SetActive(false);
                villagerHead.SetActive(false);
                break;
            case 2:
                currentCutscenePetrPosition = new Vector3(3.1741f, 0.214037f, 3.16983f);
                currentCutscenePetrRotation = Quaternion.Euler(0f, 40.9f, 0f);
                deadBodyBoxingBag.SetActive(false);
                break;
            case 3:
                currentCutscenePetrPosition = new Vector3(4.68032f, 0.214037f, -0.302862f);
                currentCutscenePetrRotation = Quaternion.Euler(0f, -164.902f, 0f);
                Animation.ironMaidenDoorLeftAnimation.duration = 1.7f;
                Animation.ironMaidenDoorRightAnimation.duration = 1.7f;
                Animation.ironMaidenDoorLeftAnimation.audioSource = null;
                Animation.ironMaidenDoorRightAnimation.audioSource = null;
                break;
            default:
                currentCutscenePetrPosition = new Vector3(5.88212f, 0.214037f, -5.63183f);
                currentCutscenePetrRotation = Quaternion.Euler(0f, -90f, 0f);
                electricChairSkull.SetActive(false);
                break;
        }
        animator.SetBool("walking", false);
        animator.SetBool("fighting", false);
        animator.SetInteger("cutscene", cutsceneId);
        cutsceneCameraAnimator.SetInteger("cutscene", cutsceneId);

        Key.HideAllKeys();
        int i = 0;
        foreach (Light light in cutsceneLights)
        {
            if (light != null) light.gameObject.SetActive(i == cutsceneId);
            i++;
        }
        playerLight.gameObject.SetActive(false);
        Movement.HideMobileUI();
        MusicPlayer.PlayCutsceneMusic();
        ResetVoice();
        GameOverlay.instance.gameObject.SetActive(false);
        GameOverlay.instance.currentNightPanel.gameObject.SetActive(false);
        GameOverlay.AnimateOpacity(5f, false, new GameObject[] {GameOverlay.instance.blackBackgroundPanel.gameObject});

        cutsceneCamera.enabled = true;

        FileManager.Write(lastCutsceneFile, cutsceneId+"");
        lastCutsceneId = cutsceneId;
    }
    public static void PlayRandomCutscene()
    {
        if (lastCutsceneId == -1)
        {
            try
            {
                lastCutsceneId = int.Parse(FileManager.Read(lastCutsceneFile));
            }
            catch (System.Exception) { }
        }
        int cutsceneId;
        do
        {
            cutsceneId = Random.Range(1, 5);
        } while (cutsceneId == lastCutsceneId);
        PlayCutscene(cutsceneId);
    }

    public static int ChooseRandomIntFromArray(int[] array)
    {
        return array[Random.Range(0, array.Length)];
    }

    public static void PlayVoice(int voiceId, float volume = 1f)
    {
        if (!Settings.current.speaking) return;
        audioSource.PlayOneShot(Voice.voices[voiceId].audioClip, volume);
    }
    public static void PlayRandomDeathVoice()
    {
        PlayVoice(ChooseRandomIntFromArray(deathVoiceIds), 25f);
    }
    public static void PlayRandomTrashTalkVoice()
    {
        if (NavAgent.playerKilled || GameOverlay.instance.gameEnded) return;
        int trashtalkId;
        do
        {
            trashtalkId = ChooseRandomIntFromArray(trashTalkVoiceIds);
        }
        while (trashtalkId == lastTrashtalkId);
        lastTrashtalkId = trashtalkId;
        PlayVoice(trashtalkId);
    }
    public static void PlayRandomCanSeePlayerVoice()
    {
        ResetVoice();
        instance.trashtalkTime = 0f;
        PlayVoice(ChooseRandomIntFromArray(canSeePlayerVoiceIds));
    }

    public static void ResetVoice()
    {
        audioSource.Stop();
    }


    public static void TargetPlayer()
    {
        if(agent.enabled) agent.SetDestination(player.transform.position);
    }

    public static void HearFootstep(float volume)
    {
        if (moveStyle == 0 || moveStyle == 4) return;
        if (moveStyle == 1)
        {
            if ((Random.value + 0.5f) * volume * 3f > distanceFromPlayer) TargetPlayer();
        }
        else if (moveStyle == 2)
        {
            if ((Random.value + 0.5f) * volume * 5f > distanceFromPlayer) TargetPlayer();
        }
        else // moveStyle == 3
        {
            if ((Random.value + 0.5f) * volume * 8f > distanceFromPlayer) TargetPlayer();
        }
    }
    public static void HearKeyCollect()
    {
        if (moveStyle == 0 || moveStyle == 4) return;
        if (moveStyle == 1)
        {
            if ((Random.value + 0.5f) * 3f > distanceFromPlayer) TargetPlayer();
        }
        else if (moveStyle == 2)
        {
            if ((Random.value + 0.5f) * 6.5f > distanceFromPlayer) TargetPlayer();
        }
        else // moveStyle == 3
        {
            if ((Random.value + 0.5f) * 12f > distanceFromPlayer) TargetPlayer();
        }
    }
    public static void HearToggle(float loudness)
    {
        if (moveStyle == 0 || moveStyle == 4) return;
        if (moveStyle == 1)
        {
            if ((Random.value * 0.5f + 0.5f) * 9f * loudness > distanceFromPlayer) TargetPlayer();
        }
        else if (moveStyle == 2)
        {
            if ((Random.value * 0.5f + 0.5f) * 16f * loudness > distanceFromPlayer) TargetPlayer();
        }
        else // moveStyle == 3
        {
            if ((Random.value * 0.5f + 0.5f) * 24f * loudness > distanceFromPlayer) TargetPlayer();
        }
    }
    public static void HearGuillotine()
    {
        if (moveStyle == 0 || moveStyle == 4) return;
        if (moveStyle == 1)
        {
            if ((Random.value * 0.5f + 0.5f) * 12f > distanceFromPlayer) TargetPlayer();
        }
        else if (moveStyle == 2)
        {
            if ((Random.value * 0.5f + 0.5f) * 20f > distanceFromPlayer) TargetPlayer();
        }
        else // moveStyle == 3
        {
            if ((Random.value * 0.5f + 0.5f) * 34f > distanceFromPlayer) TargetPlayer();
        }
    }
    public static void HearClown()
    {
        if (moveStyle == 0 || moveStyle == 4) return;
        if (moveStyle == 1)
        {
            if ((Random.value * 0.5f + 0.5f) * 14f > distanceFromPlayer) TargetPlayer();
        }
        else if (moveStyle == 2)
        {
            if ((Random.value * 0.5f + 0.5f) * 22f > distanceFromPlayer) TargetPlayer();
        }
        else // moveStyle == 3
        {
            if ((Random.value * 0.5f + 0.5f) * 38f > distanceFromPlayer) TargetPlayer();
        }
    }
    public static void HearPiano()
    {
        if (moveStyle == 0 || moveStyle == 4) return;
        if (moveStyle == 1)
        {
            if ((Random.value * 0.5f + 0.5f) * 6.5f > distanceFromPlayer) TargetPlayer();
        }
        else if (moveStyle == 2)
        {
            if ((Random.value * 0.5f + 0.5f) * 10f > distanceFromPlayer) TargetPlayer();
        }
        else // moveStyle == 3
        {
            if ((Random.value * 0.5f + 0.5f) * 20f > distanceFromPlayer) TargetPlayer();
        }
    }
}
