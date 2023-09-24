using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverlay : MonoBehaviour
{
    private class CameraFlyAwayAnimation : Animation
    {
        public CameraFlyAwayAnimation(GameObject obj) : base(obj, new Vector3(28f, 10f, 2f), new Vector3(15f, -60f, 0f), 12f) { }

        public override float Interpolate(float t)
        {
            return Animation.EaseIn(Animation.EaseIn(t));
        }
    }

    public static GameOverlay instance = null;
    public int currentNight = 0;
    public bool gameEnded = false, showingCurrentNight = false;
    private Vector3 petrEndingPosition = new Vector3(7.915f, 2.367f, 3.78f);

    public RectTransform centerText, crosshair, keyStatus;
    public RectTransform gameEndPanel, gameEndText;
    public RectTransform currentNightPanel, currentNightText, blackBackgroundPanel;
    public Camera endingCamera;
    public Light endingLight;
    public GameObject watermark;

    private float opacityAnimationTime = 0f, endingAnimationTime = 0f;
    public GameObject[] animateOpacityObjects = null;
    private float animateOpacityDuration = 1f;
    private bool animateOpacity = false;
    private bool animateBackward = false;
    private bool endGame = false, endGameWon = false;
    private bool runAwaySoundPlayed = false, youWinSoundPlayed = false, cinematicHitSoundPlayed = false;
    private bool wheelAnimatedLastFrame = false, doorsAnimatedLastFrame = false, petrEndingSceneAnimatedLastFrame = false;
    private bool endingTextFadedOut = false, musicStopped = false;
    private bool showNight = false, showNightFadedOut = false, petrTrashtalked = false;
    private float showNightAnimTime = 0f;

    void Awake()
    {
        instance = null;
    }

    void Start()
    {
        instance = this;

        Animation.cameraFlyAwayAnimation = new CameraFlyAwayAnimation(endingCamera.gameObject);

        Key.keysFound = 0;
        UpdateKeyStatus();

        endingCamera.gameObject.SetActive(false);
        endingLight.gameObject.SetActive(false);

        ShowNight();
    }

    public void Update()
    {
        if (endGame) // 100% win
        {
            if(!musicStopped)
            {
                musicStopped = true;
                MusicPlayer.Stop(3f);
                Petr.ResetVoice();
                watermark.SetActive(false);
            }
            endingAnimationTime += Time.deltaTime;
            if(endingAnimationTime <= 2f)
            {
                if (endingAnimationTime > 0.1f && !runAwaySoundPlayed)
                {
                    GameEndingAudio.PlayRunAwaySound();
                    runAwaySoundPlayed = true;
                }
                Petr.petrModel.SetActive(false);
                wheelAnimatedLastFrame = true;
                Animation.wheelAnimation.Animate();
            }
            else if(endingAnimationTime <= 8f)
            {
                doorsAnimatedLastFrame = true;
                if (wheelAnimatedLastFrame)
                {
                    wheelAnimatedLastFrame = false;
                    Animation.openEscapeDoorsAnimation.Open();
                    instance.gameEndText.GetComponent<Text>().text = Translator.GetText(endGameWon ? 36 : 37);
                    instance.gameEndText.GetComponent<Text>().color = endGameWon ? Color.green : Color.red;
                    AnimateOpacity(2f, true);
                }
                Animation.openEscapeDoorsAnimation.Animate();
                if(endingAnimationTime > 6.3f && !cinematicHitSoundPlayed)
                {
                    GameEndingAudio.PlayCinematicHitSound();
                    cinematicHitSoundPlayed = true;
                }
            }
            else if(endingAnimationTime < 15.5f)
            {
                petrEndingSceneAnimatedLastFrame = true;
                if (endGameWon)
                {
                    if (endingAnimationTime > 15f && !youWinSoundPlayed)
                    {
                        GameEndingAudio.PlayYouWinSound();
                        youWinSoundPlayed = true;
                    }

                    if (!petrTrashtalked && endingAnimationTime > 12f)
                    {
                        petrTrashtalked = true;
                        Petr.maxVolume = true;
                        Petr.PlayVoice(13);
                    }

                    if (doorsAnimatedLastFrame)
                    {
                        BatteryFlicker.secondLight = endingLight;
                        BatteryFlicker.instance.speed *= 3;
                        doorsAnimatedLastFrame = false;
                        Petr.SetAnimationState(Petr.IDLE);
                        endingCamera.gameObject.SetActive(true);
                        endingLight.gameObject.SetActive(true);
                        Petr.petrModel.SetActive(true);
                        Petr.player.SetActive(false);
                        Petr.playerLight.gameObject.SetActive(false);
                        Animation.cameraFlyAwayAnimation.Open();
                        AnimateOpacity(0.3f, false);
                    }
                    Animation.cameraFlyAwayAnimation.Animate();
                    BatteryFlicker.intensityFactor = Mathf.Max(0f, Mathf.Pow(16f-endingAnimationTime, 3.7f)/20f);
                    BatteryFlicker.instance.Flicker();
                    Petr.petrModel.transform.position = petrEndingPosition;
                    Petr.petrModel.transform.LookAt(new Vector3(endingCamera.transform.position.x, 
                        Petr.petrModel.transform.position.y, endingCamera.transform.position.z));
                }
                else
                {
                    endingAnimationTime = 15.5f;
                }
            }
            else if(endingAnimationTime <= 21.5f)
            {
                Petr.petrModel.transform.position = petrEndingPosition;
                Petr.petrModel.transform.LookAt(new Vector3(endingCamera.transform.position.x,
                        Petr.petrModel.transform.position.y, endingCamera.transform.position.z));
                Animation.cameraFlyAwayAnimation.Animate();

                if (petrEndingSceneAnimatedLastFrame)
                {
                    petrEndingSceneAnimatedLastFrame = false;
                    instance.animateOpacityObjects = new GameObject[] { instance.gameEndText.gameObject, instance.gameEndPanel.gameObject };
                    SetOpacity(1f);
                    instance.gameEndText.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
                }

                if (endingAnimationTime >= 19.5f)
                {
                    if (!endingTextFadedOut) {
                        endingTextFadedOut = true;
                        AnimateOpacity(1f, false, new GameObject[] { instance.gameEndPanel.gameObject, instance.gameEndText.gameObject });
                    }
                }

                float size = Mathf.Clamp01((21.5f - endingAnimationTime)/6f) * 0.5f + 0.5f;
                instance.gameEndText.gameObject.transform.localScale = new Vector3(size, size, size);
            } else
            {
                LoadMenu();
            }
        }
        if(showNight)
        {
            GameObject[] objs = new GameObject[] { currentNightPanel.gameObject, currentNightText.gameObject };
            animateOpacityObjects = objs;
            showNightAnimTime += Time.deltaTime;
            if (showNightAnimTime < 1.8f)
            {
                SetOpacity(1f);
            }
            else if(showNightAnimTime < 4f)
            {
                if (!showNightFadedOut)
                {
                    NavAgent.ResetTransforms();

                    if (gameEnded) SetOpacity(1f, new GameObject[] { blackBackgroundPanel.gameObject });

                    showNightFadedOut = true;
                    showingCurrentNight = false;
                    AnimateOpacity(2f, false, objs);
                }
            }
            else
            {
                showNight = false;
                foreach (GameObject obj in objs) obj.SetActive(false);
                if (gameEnded)
                {
                    if(endGameWon) LoadMenu();
                    else Petr.PlayRandomCutscene();
                }
            }
        }
        if (animateOpacity)
        {
            opacityAnimationTime += Time.deltaTime;
            if(opacityAnimationTime > animateOpacityDuration)
            {
                animateOpacity = false;
                opacityAnimationTime = 0f;
                SetOpacity(animateBackward ? 1f : 0f);
                return;
            }
            float opacity = animateBackward ? Animation.EaseOut(Animation.Offset(0f, animateOpacityDuration, opacityAnimationTime))
                : Animation.EaseIn(Animation.Offset(animateOpacityDuration, 0f, opacityAnimationTime));
            SetOpacity(opacity);
        }
    }

    public static void UpdateKeyStatus()
    {
        instance.keyStatus.GetComponent<Text>().text = Key.keysFound + " / " + Settings.current.requiredKeys;
    }

    public static void AnimateOpacity(float duration, bool backward, GameObject[] objs = default(GameObject[]))
    {
        if (objs == default(GameObject[])) objs = new GameObject[] { instance.blackBackgroundPanel.gameObject };
        instance.animateOpacityObjects = objs;
        instance.animateOpacityDuration = duration;
        instance.animateBackward = backward;
        instance.animateOpacity = true;
        instance.opacityAnimationTime = 0f;
    }

    public static void SetOpacity(float opacity, GameObject[] objs = default(GameObject[]))
    {
        if (objs == default(GameObject[])) objs = instance.animateOpacityObjects;
        if (opacity <= 0f)
        {
            opacity = 0f;
            foreach(GameObject obj in objs) obj.SetActive(false);
        } else
        {
            if (opacity > 1f) opacity = 1f;
            foreach(GameObject obj in objs)
            {
                Image image = obj.GetComponent<Image>();
                Text text = obj.GetComponent<Text>();
                if (image != null) image.color = new Color(0f, 0f, 0f, opacity);
                else if(text != null) text.color = new Color(text.color.r, text.color.g, text.color.b, opacity);
                obj.SetActive(true);
            }
        }
    }

    public static void SetCenterText(int textId, Color color = default(Color), int missingKeys = -1)
    {
        SetCenterText(Translator.GetText(textId), color, missingKeys);
    }
    public static void SetCenterText(string text, Color color = default(Color), int missingKeys = -1)
    {
        if (color == default(Color)) color = Color.white;
        instance.centerText.GetComponent<Text>().color = color;
        if (missingKeys > 0) text = text.Replace("%", missingKeys+"");
        instance.centerText.GetComponent<Text>().text = text;
    }

    public static void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public static void EndGame(bool won)
    {
        instance.gameEnded = true;
        instance.endGameWon = won;
        Animation.wheelAnimation.Open();
        instance.endGame = true;
        instance.endingAnimationTime = 0f;
        instance.animateOpacity = false;
        Movement.HideMobileUI();
        HintManager.hintEnabled = false;
        SetOpacity(0f);
    }

    public static void ShowNight(int night = default(int))
    {
        int nightTranslationId;
        if (night == default(int)) night = ++instance.currentNight;
        switch(night)
        {
            case 1: nightTranslationId = 33; break;
            case 2: nightTranslationId = 34; break;
            case 3: nightTranslationId = 35; break;
            default: nightTranslationId = 37; instance.gameEnded = true; instance.endGameWon = false; break;
        }
        string nightString = Translator.GetText(nightTranslationId);
        instance.currentNightText.gameObject.GetComponent<Text>().text = nightString;
        instance.showNightAnimTime = 0f;
        instance.showNight = false;
        instance.showNightFadedOut = false;
        instance.showNight = true;
        instance.showingCurrentNight = true;
        if (night == 1) SoundPlayer.PlayNewNight();
    }
}
