using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class BatteryFlicker : MonoBehaviour
{
    public static BatteryFlicker instance;
    public static Light secondLight = null;
    public static float intensityFactor = 1f;

    public float speed = 9f;
    public float normalIntensity = 2f;

    private Light light;
    private Random random;
    private float noiseX = 0f;
    private float randomY;

    void Start()
    {
        instance = this;
        intensityFactor = 1f;
        secondLight = null;

        light = GetComponent<Light>();
        random = new Random();
        randomY = (float) (random.NextDouble()*100000d);

        light.gameObject.SetActive(true);
        Petr.playerLight = light;
    }

    void Update()
    {
        Flicker();
    }

    public void Flicker()
    {
        float intensity = Mathf.Pow(Mathf.PerlinNoise(noiseX, randomY), 3.5f) * intensityFactor * normalIntensity * 2.5f;
        intensity = intensity < 0.1f ? 0 : intensity;
        light.intensity = Settings.current.batteryFlickering ? intensity : normalIntensity;
        if (secondLight != null) secondLight.intensity = intensity;
        noiseX += speed * Time.deltaTime;
    }
}
