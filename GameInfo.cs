using UnityEngine;
using UnityEngine.UI;

public class GameInfo : MonoBehaviour
{
    public static readonly string version = "2.1";
    public static readonly bool isProEdition = true;
    public static bool isDeviceComputer = true;

    public GameObject fullscreenText;

    void Awake()
    {
        isDeviceComputer = SystemInfo.deviceType != DeviceType.Handheld;

        GetComponent<Text>().text = (isProEdition ? "Pro" : "Free") + " Edition " + version;

        if(!isProEdition) fullscreenText.SetActive(false);
    }
}
