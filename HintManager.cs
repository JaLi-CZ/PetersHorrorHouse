using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HintManager : MonoBehaviour
{
    private static readonly float hintSoundVolume = 0.5f;

    private static HintManager instance;
    public static bool hintEnabled = true;
    private static readonly float hintDelay = 90f;
    private static float hintWait = hintDelay;
    private static readonly float maxHintTime = 20f, hintDotOpacity = 0.7f, pathOpacity = 0.2f;
    private static float currHintTime = 0f;
    public static Key key;
    private static LineRenderer pathRenderer;
    private static Vector3 closestVertexPos;
    private static bool showingHint = false;
    private static GameObject hintButton;
    private static NavMeshPath hintPath;
    private static AudioSource hintAudioSource;
    private static Texture2D circleTexture = null;
    private static Color dotColor = new Color(0f, 255f, 0f, hintDotOpacity);

    public GameObject hintButtonObj;

    void Awake()
    {   
        instance = this;
        hintEnabled = true;
        showingHint = false;
        currHintTime = 0f;
        ResetHintTimer();
        hintButton = hintButtonObj;
        hintAudioSource = GetComponent<AudioSource>();
        if(circleTexture == null) circleTexture = CreateCircleTexture(256);
    }

    void Start()
    {
        pathRenderer = gameObject.AddComponent<LineRenderer>();
        pathRenderer.startWidth = 0.06f;
        pathRenderer.endWidth = 0.06f;
        Color startColor = Color.cyan, endColor = Color.green;
        startColor.a = pathOpacity;
        endColor.a = pathOpacity;
        pathRenderer.startColor = startColor;
        pathRenderer.endColor = endColor;
        pathRenderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    void Update()
    {
        hintAudioSource.volume = hintSoundVolume * Settings.current.soundVolume / 100f;

        if (!hintEnabled || Key.activeKeys.Count == 0) {
            hintButton.SetActive(false);
            return;
        }

        if (showingHint)
        {
            currHintTime += Time.deltaTime;
            float t = currHintTime / maxHintTime;
            if (t > 1f) HideHint();
            else
            {
                UpdatePath();
                SetOpacity(1f-t);
            }
        } else
        {
            if (hintWait == 0)
            {
                if(!hintButton.activeSelf) hintAudioSource.Play();
                hintButton.SetActive(true);
            }
            else
            {
                hintWait = Mathf.Max(0, hintWait - Time.deltaTime);
                hintButton.SetActive(false);
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                GivePlayerHint();
            }
        }
    }

    void OnGUI() {
        if (!hintEnabled) return;
        if (showingHint) {
            DrawDot(key.obj.transform.position);
        }
    }

    private static void DrawDot(Vector3 pos)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);
        if (screenPos.x < 0f || screenPos.x > Screen.width || screenPos.y < 0f || screenPos.y > Screen.height
            || screenPos.z < Camera.main.nearClipPlane || screenPos.z > Camera.main.farClipPlane) return;

        float dotRadius = Screen.width * 0.02f;
        screenPos.y = Screen.height - screenPos.y;
        GUI.color = dotColor;
        GUI.DrawTexture(new Rect(screenPos.x - dotRadius * 0.5f, screenPos.y - dotRadius * 0.5f, dotRadius, dotRadius), circleTexture);
    }

    public static void ResetHintTimer()
    {
        hintWait = hintDelay;
    }

    public static void GivePlayerHint()
    {
        if (hintWait > 0 || Key.activeKeys.Count == 0) return;
        showingHint = true;
        currHintTime = 0f;
        key = Key.activeKeys[Random.Range(0, Key.activeKeys.Count)];
        if (InterstitialAds.instance != null) InterstitialAds.instance.ShowAd();
        ResetHintTimer();
        ShowPath(key.obj.transform.position);
        hintButton.SetActive(false);
    }

    public static void HideHint()
    {
        if (!showingHint) return;
        showingHint = false;
        key = null;
        pathRenderer.enabled = false;
    }

    private static void ShowPath(Vector3 toPos)
    {
        hintPath = new NavMeshPath();
        NavMesh.CalculatePath(Petr.player.transform.position, toPos, NavMesh.AllAreas, hintPath);
        int destFloor = Triangle.GetFloor(toPos);
        int pathFloor = hintPath.corners.Length == 0 ? -1 : Triangle.GetFloor(hintPath.corners[hintPath.corners.Length - 1]);
        if (hintPath.status != NavMeshPathStatus.PathComplete || destFloor != pathFloor)
        {
            float closestDist = -1f;
            closestVertexPos = Vector3.zero;
            foreach (Triangle triangle in Triangle.triangles)
            {
                foreach (Vector3 pos in new Vector3[] { triangle.a, triangle.b, triangle.c })
                {
                    if (Triangle.GetFloor(pos) != destFloor) continue;
                    float dist = Vector3.Distance(toPos, pos);
                    if (closestDist == -1f || dist < closestDist)
                    {
                        closestDist = dist;
                        closestVertexPos = pos;
                    }
                }
            }
            if (closestDist == -1f) return;
            NavMesh.CalculatePath(Petr.player.transform.position, closestVertexPos, NavMesh.AllAreas, hintPath);
        }
        if (hintPath.status == NavMeshPathStatus.PathComplete)
        {
            pathRenderer.enabled = true;
            pathRenderer.positionCount = hintPath.corners.Length;
            pathRenderer.SetPositions(hintPath.corners);
            SetOpacity(1f);
        }
    }

    private static void UpdatePath()
    {
        if(hintPath.corners.Length > 0)
        {
            Vector3 toPos = hintPath.corners[hintPath.corners.Length - 1];
            hintPath = new NavMeshPath();
            NavMesh.CalculatePath(Petr.player.transform.position, toPos, NavMesh.AllAreas, hintPath);
            pathRenderer.positionCount = hintPath.corners.Length;
            pathRenderer.SetPositions(hintPath.corners);
        }
    }

    private static void SetOpacity(float opacity)
    {
        opacity = Mathf.Pow(opacity, 2.5f);
        Color startC = pathRenderer.startColor, endC = pathRenderer.endColor;
        startC.a = opacity * pathOpacity;
        endC.a = opacity * pathOpacity;
        pathRenderer.startColor = startC;
        pathRenderer.endColor = endC;
    }

    private static Texture2D CreateCircleTexture(int resolution) {
        Texture2D texture = new Texture2D(resolution, resolution);
        Color[] colors = new Color[resolution * resolution];
        float center = resolution / 2f;
        float radiusSquared = center * center;
        for(int x=0; x<resolution; x++) {
            float dx = center - x;
            for(int y=0; y<resolution; y++) {
                float dy = center - y;
                if (dx*dx + dy*dy <= radiusSquared) colors[y * resolution + x] = Color.white;
            }
        }
        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }
}
