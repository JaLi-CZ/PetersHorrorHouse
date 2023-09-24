using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    public static Movement instance = null;
    public static bool isShifting = false;

    public Camera camera;
    public Light light;
    public GameObject hearthbeatObj;
    public float cameraYOffset = 0.69f, shiftCameraYOffset = 0.1f;
    public float shiftSpeed = 3.5f;
    public float lightYOffset = 0.5f;
    public float speed = 3f;
    public float maxYChange = 0.3f;
    public GameObject mobileUIPanel;
    public FixedJoystick moveJoystick;
    private RectTransform moveJoystickBounds;
    public float footstepDistance = 1.25f;
    public float footstepDistanceRandomize = 0.15f;
    public Image crouchStandImage;
    public Sprite crouchImage, standImage;

    private CharacterController controller;
    private float mouseX = 0f, mouseY = 0f;
    private float shiftingAnimationOffset = 0f;
    private Vector3 lastPosition;
    private float currentFootstepDistance = 0f, walkedDistance = 0f;
    private bool showedNightLastFrame = false;

    void Awake()
    {
        Translator.ModifyTexts();
    }

    void Start()
    {
        instance = this;
        currentFootstepDistance = footstepDistance;
        controller = GetComponent<CharacterController>();
        isShifting = false;
        if (GameInfo.isDeviceComputer) mobileUIPanel.SetActive(false);
        else {
            mobileUIPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        lastPosition = transform.position;
        moveJoystickBounds = moveJoystick.GetComponent<RectTransform>();
    }

    void Update()
    {
        if (PauseManager.paused || GameOverlay.instance.gameEnded || GameOverlay.instance.showingCurrentNight || NavAgent.playerKilled || Petr.playingCutscene)
        {
            if (GameOverlay.instance.showingCurrentNight) showedNightLastFrame = true;
            return;
        }

        if (!isShifting)
        {
            float distance = Vector3.Distance(lastPosition, transform.position);
            walkedDistance += distance;
            if (distance == 0f) walkedDistance = footstepDistance * 0.7f;
            if (walkedDistance > currentFootstepDistance)
            {
                walkedDistance = 0f;
                currentFootstepDistance = footstepDistance + (Random.value * 2 - 1) * footstepDistanceRandomize;
                SoundPlayer.PlayFootstep();
            }

        }

        lastPosition = transform.position;

        Vector3 move = Vector3.zero;
        float currCamYOffset = cameraYOffset;

        if (GameInfo.isDeviceComputer)
        {
            isShifting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
               || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) move += camera.transform.forward;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) move -= camera.transform.forward;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) move += camera.transform.right;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) move -= camera.transform.right;
        }
        else
        {
            float x = moveJoystick.Horizontal, y = moveJoystick.Vertical;
            if (x != 0f && y != 0f)
            {
                Vector3 forward = Vector3.Scale(camera.transform.forward, new Vector3(1, 0, 1)).normalized;
                move = forward * y + camera.transform.right * x;
            }
        }

        move = new Vector3(move.x, 0, move.z);
        move.Normalize();

        move *= speed * Time.deltaTime;

        if (isShifting)
        {
            shiftingAnimationOffset += Time.deltaTime * shiftSpeed;
            if(shiftingAnimationOffset > 1f) shiftingAnimationOffset = 1f;
            currCamYOffset = Animation.LinearInterpolation(cameraYOffset, shiftCameraYOffset, shiftingAnimationOffset);
            move *= 0.2f;
        } else if(shiftingAnimationOffset > 0f)
        {
            shiftingAnimationOffset -= Time.deltaTime * shiftSpeed;
            if(shiftingAnimationOffset < 0f) shiftingAnimationOffset = 0f;
            currCamYOffset = Animation.LinearInterpolation(cameraYOffset, shiftCameraYOffset, shiftingAnimationOffset);
            move *= 0.6f;
        }

        if (!controller.isGrounded) move = new Vector3(move.x, -4.5f*Time.deltaTime, move.z);

        controller.Move(move);

        float sensitivity = Settings.current.mouseSensitivity / 100f;
        if (GameInfo.isDeviceComputer)
        {
            mouseX += Input.GetAxis("Mouse X") * sensitivity;
            mouseY += Input.GetAxis("Mouse Y") * sensitivity;
        }
        else
        {
            Vector2 rotation = Vector2.zero;
            foreach (Touch touch in Input.touches)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(moveJoystickBounds, touch.rawPosition))
                {
                    if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
                        rotation = touch.deltaPosition * sensitivity;
                }
            }
            mouseX += rotation.x * sensitivity * 0.25f;
            mouseY += rotation.y * sensitivity * 0.25f;
        }

        mouseY = Mathf.Clamp(mouseY, -85f, 85f);

        hearthbeatObj.transform.position = transform.position;
        camera.transform.position = transform.position + new Vector3(0, currCamYOffset, 0);
        camera.transform.rotation = Quaternion.Euler(-mouseY, mouseX, 0);
        light.transform.position = transform.position + new Vector3(0, lightYOffset, 0);

        if(showedNightLastFrame)
        {
            showedNightLastFrame = false;
            LookAt(NavAgent.instance.startLookAt.transform.position);
        }
    }

    public static void HideMobileUI()
    {
        instance.mobileUIPanel.SetActive(false);
    }

    public static void LookAt(Vector3 point)
    {
        Vector3 direction = point - instance.camera.transform.position;
        direction.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        Vector3 eulerAngles = targetRotation.eulerAngles;
        instance.mouseX = eulerAngles.y;
        instance.mouseY = -eulerAngles.x;
        while (instance.mouseY > 90f) instance.mouseY -= 180f;
        while (instance.mouseY < -90f) instance.mouseY += 180f;
        instance.camera.transform.rotation = Quaternion.Euler(-instance.mouseY, instance.mouseX, 0);
    }

    public float ClosestYDown(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position, Vector3.down, out hit, 100f /*Raycast maximal distance*/)) return hit.point.y;
        return position.y;
    }

    public float GetLowestYVertex(GameObject obj)
    {
        Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        float lowestY = Mathf.Infinity;

        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y < lowestY)
            {
                lowestY = vertices[i].y;
            }
        }

        return lowestY;
    }

    public static void ToggleShifting()
    {
        if (GameInfo.isDeviceComputer) return;
        isShifting = !isShifting;
        instance.crouchStandImage.sprite = isShifting ? instance.standImage : instance.crouchImage;
    }
}
