using UnityEngine;
using System;
using System.IO;

public class FileManager : MonoBehaviour
{
    public static readonly string dataPath = Application.persistentDataPath + "/";

    public static void Write(string fileName, string text)
    {
        try
        {
            File.WriteAllText(dataPath + fileName, text);
        }
        catch (Exception e)
        {
            Debug.Log("Cannot write file '" + fileName + "' : " + e);
        }
    }

    public static string Read(string fileName)
    {
        try
        {
            return File.ReadAllText(dataPath + fileName);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
