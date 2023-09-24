using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using SFB;

public class CustomMenuMusic : MonoBehaviour
{
    private static readonly string[] supportedExtensions = new string[] { "mp3", "wav", "ogg" };

    private Button button;

    public AudioSource musicSource;

    void Awake()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(OpenFileSelection);
    }

    void OpenFileSelection()
    {
        #if UNITY_IOS
        string mobilePlatformFormat = "public.audio";
        #else
        string mobilePlatformFormat = "audio/*";
        #endif

        #if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
        ExtensionFilter[] extensions = new ExtensionFilter[] { new ExtensionFilter("Audio Files", "mp3", "wav", "ogg" ) };
        string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel("Select Audio File", "", extensions, false);
        if(paths.Length > 0) StartCoroutine(LoadAudioClip(paths[0]));
        #else
        NativeFilePicker.Permission permission = NativeFilePicker.PickFile((path) =>
        {
            if (path != null && path != "")
            {
                StartCoroutine(LoadAudioClip(path));
            }
        }, new string[] { mobilePlatformFormat });
        #endif
    }

    private IEnumerator LoadAudioClip(string filePath)
    {
        var audioClipRequest = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.UNKNOWN);
        yield return audioClipRequest.SendWebRequest();

        AudioClip audioClip = DownloadHandlerAudioClip.GetContent(audioClipRequest);
        musicSource.clip = audioClip;
        musicSource.Play();
    }
}
