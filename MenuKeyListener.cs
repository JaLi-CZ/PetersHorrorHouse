using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuKeyListener : MonoBehaviour
{
    public GameObject menuPanel, settingsPanel, controlsPanel;

    void Update()
    {
        if(menuPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return)) Menu.instance.SetGameScene();
            else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.S)) Menu.instance.ShowSettingsPanel();
            else if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl) ||
                Input.GetKeyDown(KeyCode.RightControl)) Menu.instance.ShowControlsPanel();
        }
        else if(settingsPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return)) Menu.instance.OnSave();
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Menu.instance.difficultyDropdownExtended) Menu.instance.difficulty.GetComponent<Dropdown>().Hide();
                else if (Menu.instance.movementDropdownExtended) Menu.instance.movement.GetComponent<Dropdown>().Hide();
                else Menu.instance.OnCancel();
            }
            else if (Input.GetKeyDown(KeyCode.R)) Menu.instance.OnDifficultyReset();
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.mouseScrollDelta.y > 0)
            {
                if (Settings.current.difficulty > 0)
                {
                    Menu.instance.difficulty.GetComponent<Dropdown>().value--;
                    Menu.instance.OnDifficultyChange();
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.mouseScrollDelta.y < 0)
            {
                if (Settings.current.difficulty < 3)
                {
                    Menu.instance.difficulty.GetComponent<Dropdown>().value++;
                    Menu.instance.OnDifficultyChange();
                }
            }
        }
        else if(controlsPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape)) Menu.instance.ShowMenuPanel();
        }
    }
}
