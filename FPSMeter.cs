using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSMeter : MonoBehaviour
{
    private static readonly string fileName = "show-fps.txt";

    private static FPSMeter instance;
    public Toggle toggle;

    private bool measureFPS = false;
    private Text text;
    private float time = 0f;
    private List<float> timeRecords;

    void Awake()
    {
        instance = this;

        measureFPS = Read();
        toggle.isOn = measureFPS;

        text = GetComponent<Text>();
        timeRecords = new List<float>();

        if (!measureFPS) text.text = "";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F4))
        {
            measureFPS = !measureFPS;
            toggle.isOn = measureFPS;
            Save(measureFPS);
            if (!measureFPS) text.text = "";
        }
        if (!measureFPS || Time.timeScale == 0f) return;

        time += Time.deltaTime;

        timeRecords.Add(time);

        float minTime = time - 1f;
        while (timeRecords[0] < minTime) timeRecords.RemoveAt(0);

        int fps = timeRecords.Count;
        text.text = fps + " FPS";
    }

    public static void OnToggle()
    {
        instance.measureFPS = instance.toggle.isOn;
        Save(instance.measureFPS);
        if (!instance.measureFPS) instance.text.text = "";
    }

    private static void Save(bool b)
    {
        FileManager.Write(fileName, b ? "yes" : "no");
    }

    private static bool Read()
    {
        string content = FileManager.Read(fileName);
        return content != null && content == "yes";
    }
}
