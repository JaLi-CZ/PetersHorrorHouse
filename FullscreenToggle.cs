using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullscreenToggle : MonoBehaviour
{
    private static bool isFullscreen, firstTime = true;

    void Start()
    {
        if (firstTime)
        {
            firstTime = false;
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
            isFullscreen = true;
        }
    }

    void Update()
    {
        if (!GameInfo.isProEdition) return;
        if (Input.GetKeyDown(KeyCode.F11))
        {
            isFullscreen = !isFullscreen;
            if (isFullscreen) Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
            else Screen.SetResolution((int)(Screen.currentResolution.width * 0.6f), (int)(Screen.currentResolution.height * 0.6f), false);
        }
    }
}
