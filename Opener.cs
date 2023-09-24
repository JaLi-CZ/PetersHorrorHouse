using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Animation
{
    public static float LinearInterpolation(float a, float b, float t)
    {
        return a + t * (b - a);
    }

    public static Vector3 LinearInterpolation(Vector3 v1, Vector3 v2, float t)
    {
        return new Vector3(LinearInterpolation(v1.x, v2.x, t), LinearInterpolation(v1.y, v2.y, t), LinearInterpolation(v1.z, v2.z, t));
    }

    public static float Offset(float a, float b, float c)
    {
        return (c - a) / (b - a);
    }

    public static float EaseInOut(float t)
    {
        if (t < 0.5f) return 4f * t * t * t;
        else
        {
            float f = 2f * t - 2f;
            return 0.5f * f * f * f + 1f;
        }
    }

    public static float EaseOut(float t)
    {
        return Mathf.Cos((1f - t) * Mathf.PI * 0.5f);
    }

    public static float EaseIn(float t)
    {
        return 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
    }

    public static List<Animation> allAnimations = new List<Animation>();

    public static Animation wheelAnimation, openEscapeDoorsAnimation, cameraFlyAwayAnimation, ironMaidenDoorLeftAnimation, ironMaidenDoorRightAnimation;

    public GameObject obj = null;
    public Vector3 startPosition, startRotation, endPosition, endRotation;
    public Vector3 positionChange = Vector3.zero, rotationChange = Vector3.zero;
    public float duration = 1f;
    public bool isOpened = false, isAnimated = false;
    public AudioSource audioSource = null;
    public AudioClip openAudioClip = null, closeAudioClip = null;
    public float openLoudness = 0.5f, closeLoudness = 0.5f, lastLoudness = -1f;

    public float time = 0f;
    private float t = 0f;

    public Animation(GameObject obj, Vector3 posChange, Vector3 rotChange, float duration)
    {
        this.obj = obj;
        startPosition = obj.transform.position;
        startRotation = obj.transform.rotation.eulerAngles;
        positionChange = posChange;
        rotationChange = rotChange;
        endPosition = startPosition + positionChange;
        endRotation = startRotation + rotationChange;
        isOpened = true;
        this.duration = duration;
        audioSource = obj.GetComponent<AudioSource>();

        allAnimations.Add(this);
    }

    public Animation(GameObject obj, Vector3 posChange, Vector3 rotChange, float duration, AudioClip openAudioClip, AudioClip closeAudioClip, 
        float openLoudness, float closeLoudness)
    {
        this.obj = obj;
        startPosition = obj.transform.position;
        startRotation = obj.transform.rotation.eulerAngles;
        positionChange = posChange;
        rotationChange = rotChange;
        endPosition = startPosition + positionChange;
        endRotation = startRotation + rotationChange;
        isOpened = false;
        audioSource = obj.GetComponent<AudioSource>();
        this.duration = duration;
        this.openAudioClip = openAudioClip;
        this.closeAudioClip = closeAudioClip;
        this.openLoudness = openLoudness;
        this.closeLoudness = closeLoudness;

        allAnimations.Add(this);
    }

    public virtual float Interpolate(float t)
    {
        return EaseInOut(t);
    }

    public void CheckSettings()
    {
        if (audioSource == null) return;
        if (PauseManager.paused) audioSource.Pause();
        else
        {
            audioSource.volume = Settings.current.soundVolume / 100f;
            audioSource.UnPause();
        }
    }

    public void Animate()
    {
        if (!isAnimated) return;

        time += Time.deltaTime;
        t = Offset(0, duration, time);
        if (t >= 1f)
        {
            isAnimated = false;
            time = 0f;
            t = 0f;
            return;
        }
        float u = Interpolate(t);
        if (!isOpened) u = 1f - u;
        Vector3 currentPosition = LinearInterpolation(startPosition, endPosition, u);
        Vector3 currentRotation = LinearInterpolation(startRotation, endRotation, u);
        obj.transform.position = currentPosition;
        obj.transform.rotation = Quaternion.Euler(currentRotation);
    }

    public void Toggle()
    {
        if (isOpened) Close();
        else Open();
    }

    public void Open()
    {
        isOpened = true;
        isAnimated = true;
        if (time != 0f) time = duration - time;
        Animate();

        if (audioSource != null)
        {
            audioSource.Stop();
            lastLoudness = openLoudness;
            if (openAudioClip != null)
            {
                float offset = Offset(0, duration, time);
                audioSource.clip = openAudioClip;
                audioSource.time = offset * duration;
                audioSource.Play();
            }
        }
    }

    public void Close()
    {
        isOpened = false;
        isAnimated = true;
        if (time != 0f) time = duration - time;
        Animate();

        if(audioSource != null)
        {
            audioSource.Stop();
            lastLoudness = closeLoudness;
            if (closeAudioClip != null)
            {
                float offset = Offset(0, duration, time);
                audioSource.clip = closeAudioClip;
                audioSource.time = offset * duration;
                audioSource.Play();
            }
        }
    }
}

public class Opener : MonoBehaviour
{
    public static bool animateAll = true;
    public static bool mobileClick = false;
    private static readonly float playerReachDistance = 1.25f;
    private static int ignoreRaycastLayerMask;

    private Animation[] toggleAnims;
    public GameObject clickButton;

    void Awake()
    {
        animateAll = true;
        mobileClick = false;
    }

    void Start()
    {
        Animation.allAnimations.Clear();
        ignoreRaycastLayerMask = ~((1 << LayerMask.NameToLayer("Player")) + (1 << LayerMask.NameToLayer("Ignore Raycast")));
        toggleAnims = new Animation[34];
        GameObject[] objs = new GameObject[toggleAnims.Length];
        foreach(GameObject obj in GameObject.FindObjectsOfType<GameObject>())
        {
            if(obj.name.Contains("OPEN"))
            {
                string[] parts = obj.name.Split('_');
                int id = int.Parse(parts[1]);
                objs[id] = obj;
            }
            else if (obj.name == "END_DOORS_COLLIDER_IRON")
            {
                Animation.openEscapeDoorsAnimation = new Animation(obj, Vector3.zero, new Vector3(0f, 0f, -105f), 3f);
            }
            else if (obj.name == "END_DOORS_COLLIDER_IRON_OpeningWheel")
            {
                Animation.wheelAnimation = new Animation(obj, Vector3.zero, new Vector3(225f, 0f, 0f), 2f);
            }
        }
        AudioClip
            basketOpen = ToggleSoundManager.instance.basketOpen,
            basketClose = ToggleSoundManager.instance.basketClose,
            briefcaseOpen = ToggleSoundManager.instance.briefcaseOpen,
            briefcaseClose = ToggleSoundManager.instance.briefcaseClose,
            shelfOpen = ToggleSoundManager.instance.shelfOpen,
            shelfClose = ToggleSoundManager.instance.shelfClose,
            slidingDoors = ToggleSoundManager.instance.slidingDoors,
            closetOpen = ToggleSoundManager.instance.closetOpen,
            closetClose = ToggleSoundManager.instance.closetClose,
            drawerOpen = ToggleSoundManager.instance.drawerOpen,
            drawerClose = ToggleSoundManager.instance.drawerClose,
            ironMaidenOpen = ToggleSoundManager.instance.ironMaidenOpen,
            ironMaidenClose = ToggleSoundManager.instance.ironMaidenClose,
            pianoOpen = ToggleSoundManager.instance.pianoOpen,
            pianoClose = ToggleSoundManager.instance.pianoClose;

        float
            basketOpenLoudness = ToggleSoundManager.basketOpenLoudness,
            basketCloseLoudness = ToggleSoundManager.basketCloseLoudness,
            briefcaseOpenLoudness = ToggleSoundManager.briefcaseOpenLoudness,
            briefcaseCloseLoudness = ToggleSoundManager.briefcaseCloseLoudness,
            shelfOpenLoudness = ToggleSoundManager.shelfOpenLoudness,
            shelfCloseLoudness = ToggleSoundManager.shelfCloseLoudness,
            slidingDoorsLoudness = ToggleSoundManager.slidingDoorsLoudness,
            closetOpenLoudness = ToggleSoundManager.closetOpenLoudness,
            closetCloseLoudness = ToggleSoundManager.closetCloseLoudness,
            drawerOpenLoudness = ToggleSoundManager.drawerOpenLoudness,
            drawerCloseLoudness = ToggleSoundManager.drawerCloseLoudness,
            ironMaidenOpenLoudness = ToggleSoundManager.ironMaidenOpenLoudness,
            ironMaidenCloseLoudness = ToggleSoundManager.ironMaidenCloseLoudness,
            pianoOpenLoudness = ToggleSoundManager.pianoOpenLoudness,
            pianoCloseLoudness = ToggleSoundManager.pianoCloseLoudness;


        AudioClip Open = ironMaidenOpen, Close = ironMaidenClose;
        toggleAnims[0] = new Animation(objs[0], Vector3.zero, new Vector3(0f, 0f, 135f), 1.2f, shelfOpen, shelfClose, shelfOpenLoudness, shelfCloseLoudness);
        toggleAnims[1] = new Animation(objs[1], Vector3.zero, new Vector3(0f, 0f, 107f), 1.2f, shelfOpen, shelfClose, shelfOpenLoudness, shelfCloseLoudness);
        toggleAnims[2] = new Animation(objs[2], Vector3.zero, new Vector3(0f, 0f, -107f), 1.2f, shelfOpen, shelfClose, shelfOpenLoudness, shelfCloseLoudness);
        toggleAnims[3] = new Animation(objs[3], Vector3.zero, new Vector3(95f, 0f, 0f), 1.2f, basketOpen, basketClose, basketOpenLoudness, basketCloseLoudness);
        toggleAnims[4] = new Animation(objs[4], new Vector3(0.729f, 0f, 0f), Vector3.zero, 1.2f, slidingDoors, slidingDoors, slidingDoorsLoudness, slidingDoorsLoudness);
        toggleAnims[5] = new Animation(objs[5], new Vector3(-0.729f, 0f, 0f), Vector3.zero, 1.2f, slidingDoors, slidingDoors, slidingDoorsLoudness, slidingDoorsLoudness);
        toggleAnims[6] = new Animation(objs[6], new Vector3(0f, 0f, 0.4f), Vector3.zero, 1.2f, drawerOpen, drawerClose, drawerOpenLoudness, drawerCloseLoudness);
        toggleAnims[7] = new Animation(objs[7], new Vector3(0f, 0f, 0.4f), Vector3.zero, 1.2f, drawerOpen, drawerClose, drawerOpenLoudness, drawerCloseLoudness);
        toggleAnims[8] = new Animation(objs[8], new Vector3(0f, 0f, 0.4f), Vector3.zero, 1.2f, drawerOpen, drawerClose, drawerOpenLoudness, drawerCloseLoudness);
        toggleAnims[9] = new Animation(objs[9], new Vector3(0f, 0f, 0.4f), Vector3.zero, 1.2f, drawerOpen, drawerClose, drawerOpenLoudness, drawerCloseLoudness);
        toggleAnims[10] = new Animation(objs[10], new Vector3(-0.35f, 0f, 0f), Vector3.zero, 1.2f, drawerOpen, drawerClose, drawerOpenLoudness, drawerCloseLoudness);
        toggleAnims[11] = new Animation(objs[11], new Vector3(-0.35f, 0f, 0f), Vector3.zero, 1.2f, drawerOpen, drawerClose, drawerOpenLoudness, drawerCloseLoudness);
        toggleAnims[12] = new Animation(objs[12], new Vector3(-0.35f, 0f, 0f), Vector3.zero, 1.2f, drawerOpen, drawerClose, drawerOpenLoudness, drawerCloseLoudness);
        toggleAnims[13] = new Animation(objs[13], Vector3.zero, new Vector3(0f, 0f, 100f), 1.2f, closetOpen, closetClose, closetOpenLoudness, closetCloseLoudness);
        toggleAnims[14] = new Animation(objs[14], Vector3.zero, new Vector3(0f, 0f, -120f), 1.2f, closetOpen, closetClose, closetOpenLoudness, closetCloseLoudness);
        toggleAnims[15] = new Animation(objs[15], Vector3.zero, new Vector3(0f, 0f, 110f), 1.2f, shelfOpen, shelfClose, shelfOpenLoudness, shelfCloseLoudness);
        toggleAnims[16] = new Animation(objs[16], Vector3.zero, new Vector3(0f, 0f, -110f), 1.2f, shelfOpen, shelfClose, shelfOpenLoudness, shelfCloseLoudness);
        toggleAnims[17] = new Animation(objs[17], new Vector3(0f, 0f, -0.35f), Vector3.zero, 1.2f, drawerOpen, drawerClose, drawerOpenLoudness, drawerCloseLoudness);
        toggleAnims[18] = new Animation(objs[18], new Vector3(0f, 0f, -0.35f), Vector3.zero, 1.2f, drawerOpen, drawerClose, drawerOpenLoudness, drawerCloseLoudness);
        toggleAnims[19] = new Animation(objs[19], new Vector3(0f, 0f, -0.4f), Vector3.zero, 1.2f, drawerOpen, drawerClose, drawerOpenLoudness, drawerCloseLoudness);
        toggleAnims[20] = new Animation(objs[20], new Vector3(0f, 0f, -0.4f), Vector3.zero, 1.2f, drawerOpen, drawerClose, drawerOpenLoudness, drawerCloseLoudness);
        toggleAnims[21] = new Animation(objs[21], new Vector3(0f, 0f, -0.4f), Vector3.zero, 1.2f, drawerOpen, drawerClose, drawerOpenLoudness, drawerCloseLoudness);
        toggleAnims[22] = new Animation(objs[22], Vector3.zero, new Vector3(0f, 0f, 106f), 1.2f, shelfOpen, shelfClose, shelfOpenLoudness, shelfCloseLoudness);
        toggleAnims[23] = new Animation(objs[23], Vector3.zero, new Vector3(0f, 0f, -110f), 1.2f, shelfOpen, shelfClose, shelfOpenLoudness, shelfCloseLoudness);
        toggleAnims[24] = new Animation(objs[24], Vector3.zero, new Vector3(0f, 0f, -110f), 1.2f, shelfOpen, shelfClose, shelfOpenLoudness, shelfCloseLoudness);
        toggleAnims[25] = new Animation(objs[25], Vector3.zero, new Vector3(0f, 0f, -120f), 1.2f, shelfOpen, shelfClose, shelfOpenLoudness, shelfCloseLoudness);
        toggleAnims[26] = new Animation(objs[26], Vector3.zero, new Vector3(0f, 0f, -110f), 1.2f, shelfOpen, shelfClose, shelfOpenLoudness, shelfCloseLoudness);
        toggleAnims[27] = new Animation(objs[27], Vector3.zero, new Vector3(0f, 0f, 110f), 1.2f, shelfOpen, shelfClose, shelfOpenLoudness, shelfCloseLoudness);
        toggleAnims[28] = new Animation(objs[28], Vector3.zero, new Vector3(0f, 0f, 120f), 1.2f, shelfOpen, shelfClose, shelfOpenLoudness, shelfCloseLoudness);
        toggleAnims[29] = new Animation(objs[29], Vector3.zero, new Vector3(0f, 0f, 120f), 1.2f, ironMaidenOpen, ironMaidenClose, ironMaidenOpenLoudness, ironMaidenCloseLoudness);
        toggleAnims[30] = new Animation(objs[30], Vector3.zero, new Vector3(0f, 0f, -110f), 1.2f, ironMaidenOpen, ironMaidenClose, ironMaidenOpenLoudness, ironMaidenCloseLoudness);
        toggleAnims[31] = new Animation(objs[31], Vector3.zero, new Vector3(89f, 0f, 0f), 1.2f, briefcaseOpen, briefcaseClose, briefcaseOpenLoudness, briefcaseCloseLoudness);
        toggleAnims[32] = new Animation(objs[32], Vector3.zero, new Vector3(89f, 0f, 0f), 1.2f, briefcaseOpen, briefcaseClose, briefcaseOpenLoudness, briefcaseCloseLoudness);
        toggleAnims[33] = new Animation(objs[33], Vector3.zero, new Vector3(-110f, 0f, 0f), 1.2f, pianoOpen, pianoClose, pianoOpenLoudness, pianoCloseLoudness);

        Animation.ironMaidenDoorLeftAnimation = toggleAnims[29];
        Animation.ironMaidenDoorRightAnimation = toggleAnims[30];
    }

    private bool lastTimeLookedAtOpenable = false, lastTimeLookedAtKey = false;
    void Update()
    {
        foreach(Animation animation in Animation.allAnimations) animation.CheckSettings();

        if (PauseManager.paused) return;
        if(GameOverlay.instance.gameEnded)
        {
            GameOverlay.instance.crosshair.gameObject.SetActive(false);
            GameOverlay.instance.centerText.gameObject.SetActive(false);
            GameOverlay.instance.keyStatus.gameObject.SetActive(false);
            return;
        }

        bool leftClick = false;
        if(GameInfo.isDeviceComputer) leftClick = Input.GetMouseButtonDown(0);
        else if(mobileClick) {
            mobileClick = false;
            leftClick = true;
        }
        GameObject target = GetTargetObject();
        bool notNull = target != null;
        if (notNull && target.name.Contains("OPEN"))
        {
            string[] parts = target.name.Split('_');
            int id = int.Parse(parts[1]);
            Animation data = toggleAnims[id];
            if (leftClick)
            {
                data.Toggle();
                Petr.HearToggle(data.lastLoudness);
            }
            GameOverlay.instance.crosshair.GetComponent<Image>().color = data.isOpened ? Color.red : Color.green;
            GameOverlay.SetCenterText(data.isOpened ? 42 : 41);
            GameOverlay.instance.centerText.gameObject.SetActive(true);
            lastTimeLookedAtOpenable = true;
            clickButton.SetActive(true);
        }
        else if (notNull && target.name.Contains("KEY") && target.GetComponent<Renderer>().enabled)
        {
            lastTimeLookedAtKey = true;
            if (target.name.StartsWith("PIANO_KEY"))
            {
                int id = int.Parse(target.name.Split('=')[1]);
                GameOverlay.instance.crosshair.GetComponent<Image>().color = Color.white;
                GameOverlay.SetCenterText(Piano.notes[id], Color.cyan);
                GameOverlay.instance.centerText.gameObject.SetActive(true);
                if (leftClick)
                {
                    Piano.gameObjects[id] = target;
                    Piano.PlayNote(id);
                    Petr.HearPiano();
                }
            }
            else
            {
                if (leftClick)
                {
                    target.SetActive(false);
                    Key.keysFound++;
                    Key key = Key.RemoveFromActive(target);
                    HintManager.ResetHintTimer();
                    if(key == HintManager.key) HintManager.HideHint();
                    SoundPlayer.PlayKeyCollect();
                    GameOverlay.UpdateKeyStatus();
                    Petr.HearKeyCollect();
                }
                else
                {
                    GameOverlay.instance.crosshair.GetComponent<Image>().color = Color.cyan;
                    GameOverlay.SetCenterText(43, new Color(1f, 0.5f, 0f));
                    GameOverlay.instance.centerText.gameObject.SetActive(true);
                }
            }
            clickButton.SetActive(true);
        }
        else if(notNull && target.name.Contains("END_DOORS"))
        {
            lastTimeLookedAtOpenable = true;
            if (Key.keysFound >= Settings.current.requiredKeys)
            {
                if (leftClick)
                {
                    GameOverlay.EndGame(true);
                }
                else
                {
                    GameOverlay.instance.crosshair.GetComponent<Image>().color = Color.green;
                    GameOverlay.SetCenterText(44, Color.green);
                    GameOverlay.instance.centerText.gameObject.SetActive(true);
                }
            } else
            {
                int missingKeys = Settings.current.requiredKeys - Key.keysFound;
                GameOverlay.instance.crosshair.GetComponent<Image>().color = Color.red;
                GameOverlay.SetCenterText(45, Color.red, missingKeys);
                GameOverlay.instance.centerText.gameObject.SetActive(true);
            }
            clickButton.SetActive(true);
        }
        else
        {
            if (lastTimeLookedAtOpenable)
            {
                lastTimeLookedAtOpenable = false;
                GameOverlay.instance.centerText.GetComponent<Text>().color = Color.white;
                GameOverlay.instance.crosshair.GetComponent<Image>().color = Color.gray;
                GameOverlay.instance.centerText.gameObject.SetActive(false);
            }
            if(lastTimeLookedAtKey)
            {
                lastTimeLookedAtKey = false;
                GameOverlay.instance.crosshair.GetComponent<Image>().color = Color.gray;
                GameOverlay.instance.centerText.gameObject.SetActive(false);
            }
            clickButton.SetActive(false);
        }
        if(animateAll) foreach (Animation openData in toggleAnims) if(openData != null) openData.Animate();
    }

    public GameObject GetTargetObject()
    {
        GameObject targetObject = null;
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 20f, ignoreRaycastLayerMask))
            targetObject = hitInfo.collider.gameObject;
        if (CustomDistance(transform.position, hitInfo.point) <= playerReachDistance) return targetObject;
        return null;
    }
    private float CustomDistance(Vector3 start, Vector3 end)
    {
        float dx = start.x - end.x, dy = start.y - end.y, dz = start.z - end.z;
        return Mathf.Sqrt(dx * dx + dy * dy * 0.3f + dz * dz);
    }

    public void OnMobileClick() {
        mobileClick = true;
    }
}
