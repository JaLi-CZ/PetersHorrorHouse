using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Settings
{
    public static readonly string fileName = "settings.txt";
    public static readonly Settings[] difficulties = new Settings[]
    {
        new Settings(0, 1.6f, 0, false, 15f, 4f,    4.5f, 15, 7,  true  , true),
        new Settings(1, 2.7f, 1, true,  8f,  6.5f,  3f,   12, 9,  true  , true),
        new Settings(2, 3.4f, 2, true,  5f,  12.5f, 2f,   10, 10, true  , true),
        new Settings(3, 4.4f, 3, true,  3f,  20f,   1.5f, 20, 20, false , true),
        null
    };

    public static Settings current = Load(), previous = null;

    // easy == 0 ; medium == 1 ; hard == 2 ; extreme == 3 ; custom == 4
    public int difficulty = 4;
    public float speed = 3f;
    public int movementStyle = 1;
    public float visibility = 6f;
    public float petrSurveillance = 20f;
    public float idleTime = 3f;
    public int hiddenKeys = 10, requiredKeys = 7;
    public bool hearthbeat = true, batteryFlickering = false, speaking = true;
    public float mouseSensitivity = 100f;
    public float soundVolume = 100f, musicVolume = 100f;

    public Settings(int difficulty, float speed, int movementStyle, bool batteryFlickering, float visibility, 
        float petrSurveillance, float idleTime, int hiddenKeys, int requiredKeys, bool hearthbeat, bool speaking)
    {
        this.difficulty = difficulty;
        this.speed = speed;
        this.movementStyle = movementStyle;
        this.batteryFlickering = batteryFlickering;
        this.visibility = visibility;
        this.petrSurveillance = petrSurveillance;
        this.idleTime = idleTime;
        this.hiddenKeys = hiddenKeys;
        this.requiredKeys = requiredKeys;
        this.hearthbeat = hearthbeat;
        this.speaking = speaking;
        if (current != null)
        {
            this.mouseSensitivity = current.mouseSensitivity;
            this.soundVolume = current.soundVolume;
            this.musicVolume = current.musicVolume;
        }
    }

    public Settings(int difficulty, float speed, int movementStyle, bool batteryFlickering, float visibility, 
        float petrSurveillance, float idleTime, int hiddenKeys, int requiredKeys, bool hearthbeat, bool speaking,
        float mouseSensitivity, float soundVolume, float musicVolume)
    { 
        this.difficulty = difficulty;
        this.speed = speed;
        this.movementStyle = movementStyle;
        this.batteryFlickering = batteryFlickering;
        this.visibility = visibility;
        this.petrSurveillance = petrSurveillance;
        this.idleTime = idleTime;
        this.hiddenKeys = hiddenKeys;
        this.requiredKeys = requiredKeys;
        this.hearthbeat = hearthbeat;
        this.speaking = speaking;
        this.mouseSensitivity = mouseSensitivity;
        this.soundVolume = soundVolume;
        this.musicVolume = musicVolume;
    }

    public Settings Copy()
    {
        return new Settings(difficulty, speed, movementStyle, batteryFlickering, visibility, petrSurveillance, idleTime,
            hiddenKeys, requiredKeys, hearthbeat, speaking, mouseSensitivity, soundVolume, musicVolume);
    }

    public void CopyVariableSettings()
    {
        if(current == null)
        {
            mouseSensitivity = 100f;
            soundVolume = 100f;
            musicVolume = 100f;
            speaking = true;
        } else
        {
            mouseSensitivity = current.mouseSensitivity;
            soundVolume = current.soundVolume;
            musicVolume = current.musicVolume;
            speaking = current.speaking;
        }
    }

    public bool IsMoreDifficultOrEqual(Settings settings)
    {
        if (settings.speed < speed) return false;
        if (settings.movementStyle < movementStyle) return false;
        if (batteryFlickering && !settings.batteryFlickering) return false;
        if (settings.visibility > visibility) return false;
        if (settings.petrSurveillance < petrSurveillance) return false;
        if (settings.idleTime > idleTime) return false;
        if (settings.requiredKeys < requiredKeys) return false;
        int settingsFreeKeys = settings.hiddenKeys - settings.requiredKeys;
        int freeKeys = hiddenKeys - requiredKeys;
        if (settingsFreeKeys > freeKeys) return false;
        if (!hearthbeat && settings.hearthbeat) return false;
        return true;
    }

    public static Settings GetDifficulty(int difficulty)
    {
        Settings settings = difficulties[difficulty];
        if(settings == null) return null;
        settings = settings.Copy();
        settings.CopyVariableSettings();
        return settings;
    }

    public static void Save(Settings s)
    {
        try
        {
            string data = $"difficulty {s.difficulty}\nspeed {s.speed}\nmovement {s.movementStyle}" +
                $"\nflickering {(s.batteryFlickering ? 1 : 0)}\n" +
            $"visibility {s.visibility}\npetr-surveillance {s.petrSurveillance}\nidle {s.idleTime}" +
            $"\nhidden-keys {s.hiddenKeys}\nrequired-keys {s.requiredKeys}\n" +
            $"hearthbeat {(s.hearthbeat ? 1 : 0)}\nspeaking {(s.speaking ? 1 : 0)}\nsensitivity {s.mouseSensitivity}" +
            $"\nsound {s.soundVolume}\nmusic {s.musicVolume}";
            FileManager.Write(fileName, data);
        } catch (Exception e)
        {
            Debug.Log("Failed to save Settings data: " + e);
        }
    }

    public static Settings Load()
    {
        Settings settings = GetDifficulty(1);
        try {
        string data = FileManager.Read(fileName);
        string[] lines = data.Split('\n');
            foreach (string line in lines)
            {
                try
                {
                    string[] parts = line.Split(' ');
                    string key = parts[0], value = parts[1];
                    switch (key)
                    {
                        case "difficulty":
                            int diff = int.Parse(value);
                            if (diff < 0) diff = 0;
                            else if (diff > 4) diff = 4;
                            settings.difficulty = diff;
                            break;

                        case "speed":
                            float speed = float.Parse(value);
                            if (speed < 0.5f) speed = 0.5f;
                            else if (speed > 8f) speed = 8f;
                            settings.speed = speed;
                            break;

                        case "movement":
                            int movement = int.Parse(value);
                            if (movement < 0) movement = 0;
                            else if (movement > 4) movement = 4;
                            settings.movementStyle = movement;
                            break;

                        case "flickering":
                            bool flickering = value != "0";
                            settings.batteryFlickering = flickering;
                            break;

                        case "visibility":
                            float visibility = float.Parse(value);
                            if (visibility < 1f) visibility = 1f;
                            else if (visibility > 20f) visibility = 20f;
                            settings.visibility = visibility;
                            break;

                        case "petr-surveillance":
                            float surveillance = float.Parse(value);
                            if (surveillance < 1f) surveillance = 1f;
                            else if (surveillance > 30f) surveillance = 30f;
                            settings.petrSurveillance = surveillance;
                            break;

                        case "idle":
                            float idleTime = float.Parse(value);
                            if (idleTime < 0.2f) idleTime = 0.2f;
                            else if (idleTime > 8f) idleTime = 8f;
                            settings.idleTime = idleTime;
                            break;

                        case "hidden-keys":
                            int hiddenKeys = int.Parse(value);
                            if (hiddenKeys < 1) hiddenKeys = 1;
                            else if (hiddenKeys > 100) hiddenKeys = 100;
                            settings.hiddenKeys = hiddenKeys;
                            break;

                        case "required-keys":
                            int requiredKeys = int.Parse(value);
                            if (requiredKeys < 1) requiredKeys = 1;
                            else if (requiredKeys > 100) requiredKeys = 100;
                            settings.requiredKeys = requiredKeys;
                            break;

                        case "hearthbeat":
                            bool hearthbeat = value != "0";
                            settings.hearthbeat = hearthbeat;
                            break;

                        case "speaking":
                            bool speaking = value != "0";
                            settings.speaking = speaking;
                            break;

                        case "sensitivity":
                            float sensitivity = float.Parse(value);
                            if (sensitivity < 1f) sensitivity = 1f;
                            else if (sensitivity > 900f) sensitivity = 900f;
                            settings.mouseSensitivity = sensitivity;
                            break;

                        case "sound":
                            float soundVolume = float.Parse(value);
                            if (soundVolume < 0f) soundVolume = 0f;
                            else if (soundVolume > 100f) soundVolume = 100f;
                            settings.soundVolume = soundVolume;
                            break;

                        case "music":
                            float musicVolume = float.Parse(value);
                            if (musicVolume < 0f) musicVolume = 0f;
                            else if (musicVolume > 100f) musicVolume = 100f;
                            settings.musicVolume = musicVolume;
                            break;
                    }
                } catch (Exception e) {
                    Debug.Log($"ERROR while reading line '{line}' Settings File: " + e);
                }
            }
        } catch (Exception) { /* File doesn't probably exists */}
        if(settings.requiredKeys > settings.hiddenKeys) settings.requiredKeys = settings.hiddenKeys;
        return settings;
    }
}

public class Menu : MonoBehaviour
{
    public static Menu instance;

    public RectTransform canvas, menuPanel, settingsPanel, controlsPanel, loadingPanel, background;

    public RectTransform difficulty, speed, movement, batteryFlickering, visibility,
        petrSurveillance, idleTime, hiddenKeys, requiredKeys, hearthbeat, speaking, sensitivity, soundVolume, musicVolume;

    public RectTransform speedInfo, visibilityInfo, petrSurveillanceInfo, idleTimeInfo, sensitivityInfo, soundVolumeInfo, musicVolumeInfo;

    private AudioSource audioSource;
    private bool programIsChangingDifficulty = false;
    public bool difficultyDropdownExtended = false, movementDropdownExtended = false;

    void Awake()
    {
        instance = null;
    }

    void Start()
    {
        instance = this;

        audioSource = canvas.GetComponent<AudioSource>();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public float Round(float x, int places)
    {
        return (float) Math.Round(x, places);
    }

    public void SetSettings(Settings settings)
    {
        difficulty.GetComponent<Dropdown>().value = settings.difficulty;
        speed.GetComponent<Slider>().value = settings.speed;
        movement.GetComponent<Dropdown>().value = settings.movementStyle;
        batteryFlickering.GetComponent<Toggle>().isOn = settings.batteryFlickering;
        visibility.GetComponent<Slider>().value = settings.visibility;
        petrSurveillance.GetComponent<Slider>().value = settings.petrSurveillance;
        idleTime.GetComponent<Slider>().value = settings.idleTime;
        hiddenKeys.GetComponent<InputField>().text = settings.hiddenKeys + "";
        requiredKeys.GetComponent<InputField>().text = settings.requiredKeys + "";
        hearthbeat.GetComponent<Toggle>().isOn = settings.hearthbeat;
        speaking.GetComponent<Toggle>().isOn = settings.speaking;
        float s = settings.mouseSensitivity;
        sensitivity.GetComponent<Slider>().value = s > 100f ? Mathf.Sqrt(s)*10f : s;
        soundVolume.GetComponent<Slider>().value = settings.soundVolume;
        musicVolume.GetComponent<Slider>().value = settings.musicVolume;
    }


    public void SetGameScene()
    {
        menuPanel.gameObject.SetActive(false);
        loadingPanel.gameObject.SetActive(true);
        SetScene("Game");
    }
    public void SetMenuScene()
    {
        SetScene("Menu");
    }
    public void SetScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void ShowMenuPanel()
    {
        Settings.previous = null;
        settingsPanel.gameObject.SetActive(false);
        controlsPanel.gameObject.SetActive(false);
        menuPanel.gameObject.SetActive(true);
    }
    public void ShowSettingsPanel()
    {
        Settings.previous = Settings.current.Copy();
        SetSettings(Settings.current);
        menuPanel.gameObject.SetActive(false);
        settingsPanel.gameObject.SetActive(true);
    }
    public void ShowControlsPanel()
    {
        if (GameInfo.isDeviceComputer)
        {
            controlsPanel.gameObject.SetActive(true);
            menuPanel.gameObject.SetActive(false);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OpenWebpage()
    {
        Application.OpenURL("https://petershorrorhouse.com");
    }


    public void DetectDifficulty()
    {
        int lastIndex = Settings.difficulties.Length - 1;
        for (int i = lastIndex; i >= 0; i--)
        {
            Settings diff = Settings.difficulties[i];
            if (diff == null) continue;
            if (diff.IsMoreDifficultOrEqual(Settings.current))
            {
                programIsChangingDifficulty = true;
                difficulty.GetComponent<Dropdown>().value = diff.difficulty;
                programIsChangingDifficulty = false;
                return;
            }
        }
        difficulty.GetComponent<Dropdown>().value = lastIndex;
    }

    public void OnDifficultyReset()
    {
        if(difficulty.GetComponent<Dropdown>().value == 4) difficulty.GetComponent<Dropdown>().value = 1;
        OnDifficultyChange();
    }

    public void OnDifficultyChange()
    {
        if (programIsChangingDifficulty) return;
        int value = difficulty.GetComponent<Dropdown>().value;
        Settings settings = Settings.GetDifficulty(value);
        if (settings != null)
        {
            Settings.current = settings;
            SetSettings(settings);
        }
        else { 
            Settings.current.difficulty = 4;
            DetectDifficulty();
        }
    }
    public void OnSpeedChange()
    {
        float value = speed.GetComponent<Slider>().value;
        Settings.current.speed = value;
        speedInfo.GetComponent<Text>().text = Round(value, 2) + " m/s";
        DetectDifficulty();
    }
    public void OnMovementStyleChange()
    {
        int value = movement.GetComponent<Dropdown>().value;
        Settings.current.movementStyle = value;
        DetectDifficulty();
    }
    public void OnBatteryFlickeringChange()
    {
        bool value = batteryFlickering.GetComponent<Toggle>().isOn;
        Settings.current.batteryFlickering = value;
        DetectDifficulty();
    }
    public void OnVisibilityChange()
    {
        float value = visibility.GetComponent<Slider>().value;
        Settings.current.visibility = value;
        visibilityInfo.GetComponent<Text>().text = Round(value, 2) + " m";
        DetectDifficulty();
    }
    public void OnSurveillanceChange()
    {
        float value = petrSurveillance.GetComponent<Slider>().value;
        Settings.current.petrSurveillance = value;
        petrSurveillanceInfo.GetComponent<Text>().text = Round(value, 1) + " m";
        DetectDifficulty();
    }
    public void OnIdleTimeChange()
    {
        float value = idleTime.GetComponent<Slider>().value;
        Settings.current.idleTime = value;
        idleTimeInfo.GetComponent<Text>().text = Round(value, 1) + " s";
        DetectDifficulty();
    }
    public void OnHiddenKeysChange()
    {
        try
        {
            int value = int.Parse(hiddenKeys.GetComponent<InputField>().text);
            if (value < 1) value = 1;
            else if (value > 100) value = 100;
            if (value < Settings.current.requiredKeys)
            {
                Settings.current.requiredKeys = value;
                requiredKeys.GetComponent<InputField>().text = value + "";
            }
            Settings.current.hiddenKeys = value;
            hiddenKeys.GetComponent<InputField>().text = value + "";
            DetectDifficulty();
        } catch (Exception) { }
    }
    public void OnRequiredKeysChange()
    {
        try
        {
            int value = int.Parse(requiredKeys.GetComponent<InputField>().text);
            if (value < 1) value = 1;
            else if (value > 100) value = 100;
            if (value > Settings.current.hiddenKeys) value = Settings.current.hiddenKeys;
            Settings.current.requiredKeys = value;
            requiredKeys.GetComponent<InputField>().text = value + "";
            DetectDifficulty();
        } catch (Exception) { }
    }
    public void OnHearthbeatChange()
    {
        bool value = hearthbeat.GetComponent<Toggle>().isOn;
        Settings.current.hearthbeat = value;
        DetectDifficulty();
    }
    public void OnSpeakingChange()
    {
        bool value = speaking.GetComponent<Toggle>().isOn;
        Settings.current.speaking = value;
        DetectDifficulty();
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

    public void OnSave()
    {
        Settings.Save(Settings.current);
        ShowMenuPanel();
    }
    public void OnCancel()
    {
        if (Settings.previous != null) Settings.current = Settings.previous;
        ShowMenuPanel();
    }
}
