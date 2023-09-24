using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropdownController : MonoBehaviour
{
    public int id;

    private void Start()
    {
        if(id == 0) Menu.instance.difficultyDropdownExtended = true;
        else if(id == 1) Menu.instance.movementDropdownExtended = true;
    }

    private void OnDestroy()
    {
        if (Menu.instance == null) return;
        if (id == 0) Menu.instance.difficultyDropdownExtended = false;
        else if (id == 1) Menu.instance.movementDropdownExtended = false;
    }
}
