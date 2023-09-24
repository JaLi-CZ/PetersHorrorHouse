using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideControlsButton : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(GameInfo.isDeviceComputer);
    }
}
