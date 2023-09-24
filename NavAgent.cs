using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using SysRandom = System.Random;

public class Triangle
{
    public static Triangle[] triangles;
    private static int currentIndex = 0;
    private static float totalArea = 0;
    private static float[] floorChanceFactor = new float[] { 1.6f, 1.3f, 1.2f};

    public readonly Vector3 a, b, c;
    public readonly float area;

    public Triangle(Vector3 a, Vector3 b, Vector3 c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        area = 0.5f * Vector3.Cross(b - a, c - a).magnitude * floorChanceFactor[GetFloor(a)];
        triangles[currentIndex++] = this;
        totalArea += area;
    }

    public Vector3 GetRandomPoint()
    {
        float r1 = UnityEngine.Random.value, r2 = UnityEngine.Random.value, sqrt = Mathf.Sqrt(r1);
        return (1f - sqrt) * a + (sqrt * (1f - r2)) * b + (r2 * sqrt) * c;
    }

    public static Triangle GetRandomTriangle()
    {
        float r = UnityEngine.Random.value * totalArea;
        foreach (Triangle t in triangles)
        {
            if (r < t.area) return t;
            r -= t.area;
        }
        return null;
    }

    public static int GetFloor(Vector3 point)
    {
        if (point.y > 5f) return 2;
        if (point.y > 2.3f) return 1;
        return 0;
    }
}

public class NavAgent : MonoBehaviour
{
    public class GuillotineAnimation : Animation
    {
        public GuillotineAnimation(GameObject obj) : base(obj, new Vector3(0, -0.9f, 0), Vector3.zero, 0.65f) { }

        public override float Interpolate(float t)
        {
            return Animation.EaseIn(t);
        }
    }


    public static bool playerKilled = false;
    private static float killingDistance = 0.82f;
    public static NavAgent instance;

    public GameObject player, hearthbeat, guillatineCutter, clown, startLookAt;
    public Light clownLight;
    public AudioClip guillatineSound, clownJumpscareSound;
    public Camera playerCamera, petrCamera;
    public float minDistanceFromPetrToNotFeelYou = 1f;
    public GuillotineAnimation guillotineAnimation;

    private Vector3 playerStartingPos, petrStartingPos;
    private float clownLightIntensity = 0f;
    private bool guilattineFalled = false, clownJumpscared = false;
    private AudioSource guillotineAudioSource, clownAudioSource;
    private NavMeshAgent agent;
    private NavMeshPath path;
    private NavMeshTriangulation triangulation;
    private float idleTime = 3f;
    private float currentIdleTime = 0f;
    private bool idleLook1 = false, idleLook2 = false;
    private SysRandom random;
    private Vector3 lastAgentPos;
    private float playerKillAnimTime = 0f;
    private NavMeshPath pathToPlayer;
    private float cantSeePlayerTime = 100f;
    private float heartbeatVolume = 0f;

    void Start()
    {
        instance = this;

        playerKilled = false;

        Petr.petrModel = gameObject;
        Petr.player = player;
        playerStartingPos = player.transform.position;
        petrStartingPos = transform.position;

        random = new SysRandom();
        agent = GetComponent<NavMeshAgent>();
        Petr.agent = agent;
        triangulation = NavMesh.CalculateTriangulation();
        if (Triangle.triangles == null)
        {
            Triangle.triangles = new Triangle[triangulation.indices.Length / 3];
            for (int i = 0; i < Triangle.triangles.Length; i++)
            {
                int idx = i * 3;
                new Triangle(
                    triangulation.vertices[triangulation.indices[idx]],
                    triangulation.vertices[triangulation.indices[idx + 1]],
                    triangulation.vertices[triangulation.indices[idx + 2]]
                );
            }
        }
        lastAgentPos = agent.transform.position;

        guillotineAnimation = new GuillotineAnimation(guillatineCutter);
        guilattineFalled = false;

        guillotineAudioSource = guillatineCutter.GetComponent<AudioSource>();
        guillotineAudioSource.clip = guillatineSound;

        SetVisible(clown, false);
        clownAudioSource = clown.GetComponent<AudioSource>();
        clownAudioSource.clip = clownJumpscareSound;

        //PrintFloorVisits(10000);
    }
    private void PrintFloorVisits(int visits, bool visualize=true)
    {
        int[] floorVisits = new int[3];
        for (int i = 0; i < visits; i++)
        {
            Vector3 destination = GetRandomDestination();

            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(agent.transform.position, destination, NavMesh.AllAreas, path);
            floorVisits[Triangle.GetFloor(destination)]++;
            if (visualize)
            {
                Color color;
                switch (Triangle.GetFloor(destination))
                {
                    case 0: color = Color.red; break;
                    case 1: color = Color.yellow; break;
                    case 2: color = Color.green; break;
                    default: color = Color.white; break;
                }
                GameObject dot = new GameObject("RedDot" + i);
                dot.transform.localScale *= 3f;
                dot.transform.Rotate(90f, 0f, 0f);
                SpriteRenderer renderer = dot.AddComponent<SpriteRenderer>();
                Texture2D texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, color);
                texture.Apply();
                renderer.sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
                dot.transform.position = destination;
                renderer.sortingOrder = 999;
            }
        }
        Debug.Log("Basement: " + floorVisits[0] + " ; Ground Floor: " + floorVisits[1] + " ; First Floor: " + floorVisits[2]);
    }

    void Update()
    {
        if (Time.timeScale == 0f || GameOverlay.instance.gameEnded || Petr.playingCutscene)
        {
            if (Petr.playingCutscene)
            {
                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
                Petr.skeleton.transform.position = Petr.currentCutscenePetrPosition;
                Petr.skeleton.transform.rotation = Petr.currentCutscenePetrRotation;
            }
            hearthbeat.GetComponent<AudioSource>().Pause();
            guillotineAudioSource.Pause();
            clownAudioSource.Pause();
            return;
        }

        hearthbeat.GetComponent<AudioSource>().UnPause();
        guillotineAudioSource.UnPause();
        clownAudioSource.UnPause();

        idleTime = Settings.current.idleTime;
        float sfxVolume = Settings.current.soundVolume / 100f;
        guillotineAudioSource.volume = sfxVolume;
        clownAudioSource.volume = sfxVolume;

        if(!guilattineFalled && RayHasCleanPath(playerCamera, guillatineCutter)
            && Vector3.Distance(playerCamera.transform.position, guillatineCutter.transform.position) < 3f)
        {
            guilattineFalled = true;
            guillotineAnimation.isAnimated = true;
            guillotineAudioSource.Play();
            Petr.HearGuillotine();
        }
        if(guillotineAnimation.isAnimated)
        {
            guillotineAnimation.Animate();
        }

        if (!clownJumpscared && RayHasCleanPath(playerCamera, clown)
            && Vector3.Distance(playerCamera.transform.position, clown.transform.position) < 1.4f)
        {
            clownJumpscared = true;
            SetVisible(clown, true);
            Movement.LookAt(clown.transform.position);
            clownLight.enabled = true;
            clownLightIntensity = 2f;
            clownLight.intensity = clownLightIntensity;
            clownAudioSource.Play();
            Petr.HearClown();
        }
        if(clownLight.enabled)
        {
            clownLightIntensity -= Time.deltaTime * 1.5f;
            if(clownLightIntensity <= 0f)
            {
                clownLightIntensity = 0f;
                clownLight.intensity = 0f;
                clownLight.enabled = false;
            } else
            {
                clownLight.intensity = (float) (clownLightIntensity * random.NextDouble());
            }
        }

        if(agent.enabled) pathToPlayer = GetPath(player.transform.position);
        float distanceFromPlayer = GetPathLength(pathToPlayer);

        float soundVolume = Settings.current.soundVolume / 100f;
        if (distanceFromPlayer >= 0f && distanceFromPlayer <= 15f) heartbeatVolume = Mathf.Pow(1f - distanceFromPlayer / 15f, 1.5f);
        else heartbeatVolume = 0f;
        int hearthbeatEnabled = Settings.current.hearthbeat ? 1 : 0;
        hearthbeat.GetComponent<AudioSource>().volume = heartbeatVolume * soundVolume * hearthbeatEnabled;

        if (playerKilled)
        {
            playerKillAnimTime += Time.deltaTime;
            MoveAgentBack(distanceFromPlayer);
            agent.transform.LookAt(playerCamera.transform.position);
            agent.transform.rotation = Quaternion.Euler(0, agent.transform.eulerAngles.y, agent.transform.eulerAngles.z);
            if (playerKillAnimTime > 2.15f)
            {
                GameOverlay.ShowNight();
                ResetTransforms();

                playerKillAnimTime = 0f;
                playerKilled = false;
                agent.enabled = true;

                if (!GameOverlay.instance.gameEnded) MusicPlayer.Play();

                pathToPlayer = GetPath(player.transform.position);
                distanceFromPlayer = GetPathLength(pathToPlayer);
            }
            else return;
        }

        Petr.distanceFromPlayer = distanceFromPlayer;
        if (distanceFromPlayer == float.PositiveInfinity)
        {
            Debug.Log("Player is unreachable for Petr!");
        }

        bool canSeePlayer = PetrCanSeePlayer();
        //VisualizeRay(petrCamera, player);
        if (canSeePlayer || distanceFromPlayer <= minDistanceFromPetrToNotFeelYou)
        {
            NavMeshPath path = GetPath(player.transform.position);
            if (path.status == NavMeshPathStatus.PathComplete) SetPath(path);
        }

        if (canSeePlayer)
        {
            if (cantSeePlayerTime > 5f)
            {
                Petr.PlayRandomCanSeePlayerVoice();
            }
            cantSeePlayerTime = 0f;
        }
        else cantSeePlayerTime += Time.deltaTime;

        if (pathToPlayer.status == NavMeshPathStatus.PathComplete && distanceFromPlayer < killingDistance)
        {
            MoveAgentBack(distanceFromPlayer);
            agent.isStopped = true;
            Petr.SetAnimationState(Petr.FIGHT);
            // kill player
            playerKilled = true;
            Vector3 lookAtPos = petrCamera.transform.position - new Vector3(0f, 0.25f, 0f);
            playerCamera.transform.position = new Vector3(playerCamera.transform.position.x,
                lookAtPos.y+0.1f, playerCamera.transform.position.z);
            Movement.LookAt(lookAtPos);
            agent.enabled = false;
            agent.transform.LookAt(playerCamera.transform.position);
            agent.transform.rotation = Quaternion.Euler(0, agent.transform.eulerAngles.y, agent.transform.eulerAngles.z);
            Petr.ResetVoice();
            MusicPlayer.Stop(0.15f);
            SoundPlayer.PlayKill();
        }
        else if(agent.remainingDistance == 0f) // destination reached
        {
            lastAgentPos = agent.transform.position;
            agent.isStopped = true;
            Petr.SetAnimationState(Petr.IDLE);
            currentIdleTime += Time.deltaTime;
            if (currentIdleTime > idleTime)
            {
                SetPath(GetPath(GetRandomDestination()));
            }
            else
            {
                if (!idleLook1)
                {
                    if (currentIdleTime > idleTime * 0.4f)
                    {
                        LookAround();
                        idleLook1 = true;
                    }
                }
                else if (!idleLook2)
                {
                    if (currentIdleTime > idleTime * 0.7f)
                    {
                        LookAround();
                        idleLook2 = true;
                    }
                }
            }
        }
        else
        {
            agent.isStopped = false;
            Petr.SetAnimationState(Petr.WALK);
        }

        VisualizePath(agent.path);

        lastAgentPos = agent.transform.position;
    }

    private void MoveAgentBack(float distFromPlayer)
    {
        agent.transform.position = lastAgentPos;
    }

    private void SetVisible(GameObject obj, bool b)
    {
        obj.GetComponent<Renderer>().enabled = b;
        foreach (Transform child in obj.transform)
        {
            child.GetComponent<Renderer>().enabled = b;
            foreach (Transform c in child)
            {
                c.GetComponent<Renderer>().enabled = b;
            }
        }
    }

    public void SetPath(NavMeshPath path)
    {
        agent.isStopped = false;
        agent.path = path;
        Petr.SetAnimationState(Petr.WALK);
        currentIdleTime = 0f;
        idleLook1 = false;
        idleLook2 = false;
    }

    public IEnumerator LookAt(Vector3 point, float duration)
    {
        Quaternion startRotation = agent.transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(point - agent.transform.position);

        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            agent.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        agent.transform.rotation = targetRotation;
    }

    public void LookAround()
    {
        Vector3 point = GetRandomPointAroundAgent();
        if (random.NextDouble() < 0.25) point = new Vector3(player.transform.position.x, point.y, player.transform.position.z);
        StartCoroutine(LookAt(point, idleTime * 0.25f));
    }
                            
    public Vector3 GetRandomPointAroundAgent()
    {
        return new Vector3(agent.transform.position.x + (float)(random.NextDouble()*3),
            agent.transform.position.y, agent.transform.position.z + (float)(random.NextDouble()*3));
    }

    public Vector3 GetRandomDestination()
    {
        while (true)
        {
            Vector3 destination = GetRandomDestinationUnsafe();
            if (GetPath(destination).status == NavMeshPathStatus.PathComplete) return destination;
        }
    }

    public Vector3 GetRandomDestinationUnsafe()
    {
        Triangle triangle = Triangle.GetRandomTriangle();
        return Triangle.GetRandomTriangle().GetRandomPoint();
    }

    public NavMeshPath GetPath(Vector3 destination)
    {
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(destination, path);
        return path;
    }

    public float GetPathLength(NavMeshPath path)
    {
        if (path == null || path.status != NavMeshPathStatus.PathComplete) return float.PositiveInfinity;
        float distance = 0f;
        Vector3[] corners = path.corners;
        for (int i = 0; i < corners.Length - 1; i++)
        {
            distance += Vector3.Distance(corners[i], corners[i + 1]);
        }
        return distance;
    }

    private static LineRenderer previousLineRenderer;
    public static void VisualizeRay(Camera cam, GameObject obj)
    {
        Color color = RayHasCleanPath(cam, obj) ? Color.green : Color.red;
        if (previousLineRenderer != null)
        {
            GameObject.Destroy(previousLineRenderer.gameObject);
        }
        GameObject lineObject = new GameObject("Line");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.SetPositions(new Vector3[] { cam.transform.position, obj.transform.position });
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineMaterial.color = color;
        lineRenderer.material = lineMaterial;

        previousLineRenderer = lineRenderer;
    }

    public static void VisualizePath(NavMeshPath path)
    {
        return;
        GameObject pathObject = GameObject.Find("Path");
        if (pathObject != null)
        {
            GameObject.Destroy(pathObject);
        }
        pathObject = new GameObject("Path");
        LineRenderer lineRenderer = pathObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.red;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.positionCount = path.corners.Length;
        lineRenderer.SetPositions(path.corners);
    }



    public bool PetrCanSeePlayer()
    {
        float dist = Vector3.Distance(petrCamera.transform.position, player.transform.position);
        return dist <= Settings.current.petrSurveillance && CameraCanSee(petrCamera, player);
    }

    public static bool CameraCanSee(Camera camera, GameObject obj)
    {
        return RayHasCleanPath(camera, obj) && CameraIsLookingAt(camera, obj);
    }
    public static bool RayHasCleanPath(Camera camera, GameObject obj)
    {
        Vector3 cameraPos = camera.transform.position;
        Vector3 playerPos = obj.transform.position;

        RaycastHit hitInfo;
        if (Physics.Linecast(cameraPos, playerPos, out hitInfo))
        {
            if (hitInfo.collider.gameObject == obj) return true;
        }
        return false;
    }
    public static bool CameraIsLookingAt(Camera camera, GameObject obj) // ignores obstacles
    {
        Vector3 viewportPoint = camera.WorldToViewportPoint(obj.transform.position);
        return viewportPoint.z > 0 && viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1;
    }

    public static void ResetTransforms()
    {
        instance.agent.enabled = false;
        instance.lastAgentPos = instance.petrStartingPos;
        instance.transform.position = instance.petrStartingPos;
        instance.agent.enabled = true;
        instance.player.transform.position = instance.playerStartingPos;
        Movement.LookAt(instance.startLookAt.transform.position);
    }
}
