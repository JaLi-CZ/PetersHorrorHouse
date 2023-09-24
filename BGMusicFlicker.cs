using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;

public class BGMusicFlicker : MonoBehaviour
{
    public RectTransform background;

    private Image image;
    private AudioSource audioSource;
    private float wiggleNoiseX = 0f, wiggleNoiseY = 0f, wiggleAmount = 0f, zoom = 1f;

    void Awake()
    {
        Time.timeScale = 1f;
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        image = background.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        float musicVolume = Settings.current.musicVolume/100f;
        audioSource.volume = musicVolume;

        float volume = GetVolume();

        float opacity = Mathf.Pow(volume, 3.2f);
        wiggleNoiseX += Time.deltaTime * 10f * opacity;
        wiggleNoiseY += Time.deltaTime * 10f * opacity;
        wiggleAmount = Mathf.Max(Mathf.Max(0, wiggleAmount - Time.deltaTime * 3f), opacity * 30f);
        zoom = Mathf.Max(1f, zoom - Time.deltaTime * 0.2f);
        zoom = Mathf.Max(zoom, 1f + opacity * 0.25f);

        SetOpacity(opacity);
        Wiggle(wiggleAmount);
        Zoom(zoom);
    }

    public void SetOpacity(float opacity)
    {
        Color c = image.color;
        c.a = opacity;
        image.color = c;
    }

    public void Wiggle(float amount)
    {
        float noiseX = Mathf.PerlinNoise(wiggleNoiseX, 0f) * 2f - 1;
        float noiseY = Mathf.PerlinNoise(100f, wiggleNoiseY) * 2f - 1;
        float x = noiseX * amount, y = noiseY * amount;
        SetBackgroundPosition(x, y);
    }

    public void Zoom(float zoom)
    {
        background.localScale = new Vector3(zoom, zoom, 1);
    }

    private void SetBackgroundPosition(float x, float y)
    {
        background.position = new Vector3(x + Screen.width * 0.5f, y + Screen.height * 0.5f, background.position.z);
    }

    // returns the current loudness of an Audio Clip that is being played by the AudioSource
    public float GetVolume()
    {
        float[] data = new float[1024];
        audioSource.GetOutputData(data, 0);

        float sum = 0f;
        foreach (float value in data) sum += Mathf.Abs(value);

        float rmsValue = Mathf.Sqrt(sum / data.Length);
        float volume = Mathf.Clamp01(rmsValue * 2f);
        return volume;
    }
}
