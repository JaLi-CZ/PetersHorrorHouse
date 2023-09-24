using UnityEngine;
using UnityEngine.Advertisements;

public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
{
    public static AdsInitializer instance = null;

    public string androidGameId, iOSGameId;
    public bool testMode = true;
    private string gameId;

    void Awake()
    {
        if (instance != null || GameInfo.isProEdition) return;
        instance = this;
        InitializeAds();
    }

    public void InitializeAds()
    {
        if (GameInfo.isProEdition) return;
        Debug.Log("Initializing Unity Ads...");
        gameId = (Application.platform == RuntimePlatform.IPhonePlayer) ? iOSGameId : androidGameId;
        Advertisement.Initialize(gameId, testMode, this);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads successfully initialized!");
        InterstitialAds.instance.LoadAd();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads initialization failed! Error: {error.ToString()} -> {message}");
    }
}
