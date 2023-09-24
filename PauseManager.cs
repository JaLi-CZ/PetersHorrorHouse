using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class PauseManager : MonoBehaviour
{
    public static bool paused = false;
    private static Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

    public GameObject pauseMenuPanel;
    public GameObject loadingPanel;
    public RectTransform sensitivity, sensitivityInfo, soundVolume, soundVolumeInfo, musicVolume, musicVolumeInfo;

    void Start()
    {
        loadingPanel.SetActive(false);
        SetPaused(false);
    }

    void Update()
    {
        if(paused && Input.GetKeyDown(KeyCode.Space)) SetPaused(false);
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;
            SetPaused(paused);
        }
        else if(paused && Input.GetKeyDown(KeyCode.Return))
        {
            OnMenuButtonClick();
        }
    }

    public void SetPaused(bool b)
    {
        paused = b;
        pauseMenuPanel.SetActive(b);
        HintManager.hintEnabled = !b;
        Time.timeScale = b ? 0f : 1f;
        if(b)
        {
            float s = Settings.current.mouseSensitivity;
            sensitivity.GetComponent<Slider>().value = s > 100f ? Mathf.Sqrt(s) * 10f : s;
            soundVolume.GetComponent<Slider>().value = Settings.current.soundVolume;
            musicVolume.GetComponent<Slider>().value = Settings.current.musicVolume;
            if(GameInfo.isDeviceComputer) Cursor.lockState = CursorLockMode.None;
        } else
        {
            Settings.Save(Settings.current);
            if (GameInfo.isDeviceComputer) Cursor.lockState = CursorLockMode.Locked;
        }
        if (!GameInfo.isDeviceComputer) Cursor.lockState = CursorLockMode.None;
        else Cursor.visible = paused;
    }

    public float Round(float x, int places)
    {
        return (float) Math.Round(x, places);
    }

    public void OnSensitivityChange()
    {
        float value = sensitivity.GetComponent<Slider>().value;
        if (value > 100f) value = value * value / 100f;
        Settings.current.mouseSensitivity = value;
        sensitivityInfo.GetComponent<Text>().text = Round(value, 1) + " %";
    }
    public void OnSoundVolumeChange()
    {
        float value = soundVolume.GetComponent<Slider>().value;
        Settings.current.soundVolume = value;
        soundVolumeInfo.GetComponent<Text>().text = Round(value, 1) + " %";
    }
    public void OnMusicVolumeChange()
    {
        float value = musicVolume.GetComponent<Slider>().value;
        Settings.current.musicVolume = value;
        musicVolumeInfo.GetComponent<Text>().text = Round(value, 1) + " %";
    }
    public void OnContinueButtonClick()
    {
        SetPaused(false);
    }
    public void OnMenuButtonClick()
    {
        loadingPanel.SetActive(true);
        SceneManager.LoadScene("Menu");
    }
}
