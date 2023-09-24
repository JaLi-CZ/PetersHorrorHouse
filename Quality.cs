using System;
using UnityEngine;
using UnityEngine.UI;

public class Quality : MonoBehaviour
{
    private static readonly int HIGH = 0, MEDIUM = 1, LOW = 2, VERY_LOW = 3;
    private static string fileName = "quality.txt";
    private static Dropdown dropdown;
    private static int quality = -1;

    void Awake()
    {
        dropdown = GetComponent<Dropdown>();

        quality = ReadQuality();
        Switch(quality);
    }

    public static void Switch()
    {
        Switch(dropdown.value);
    }

    public static void Switch(int quality)
    {
        if (quality < HIGH || quality > VERY_LOW) return;
        Quality.quality = quality;
        QualitySettings.SetQualityLevel(quality, applyExpensiveChanges: true);
        SaveQuality();
        dropdown.value = quality;
    }

    public static void SaveQuality()
    {
        FileManager.Write(fileName, quality+"");
    }

    public static int ReadQuality()
    {
        try
        {
            return int.Parse(FileManager.Read(fileName));
        } catch (Exception)
        {
            return -1;
        }
    }
}
