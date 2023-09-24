using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Warning : MonoBehaviour
{
    private static readonly string showWarningFilePath = "show-warning.txt";
    public static bool showWarning;

    private void Start()
    {
        showWarning = ReadShowWarning();
        if (!showWarning) Continue(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Exit();
        else if (Input.GetKeyDown(KeyCode.Return)) Continue(true);
    }

    public void Exit()
    {
        gameObject.SetActive(false);
        Application.Quit();
    }

    public void Continue(bool showAgain)
    {
        showWarning = showAgain;
        SaveShowWarning();
        gameObject.SetActive(false);
        SceneManager.LoadScene("Menu");
    }

    private static bool ReadShowWarning()
    {
        string content = FileManager.Read(showWarningFilePath);
        if (content == null) return true;
        return content.ToLower() != "no";
    }

    private static void SaveShowWarning()
    {
        string content = showWarning ? "yes" : "no";
        FileManager.Write(showWarningFilePath, content);
    }
}
