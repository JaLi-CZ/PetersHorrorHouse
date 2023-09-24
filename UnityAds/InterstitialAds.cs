using UnityEngine;
using UnityEngine.Advertisements;

using System.Threading.Tasks;

public class InterstitialAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static InterstitialAds instance = null;

    public string androidAdUnitId = "Interstitial_Android";
    public string iOSAdUnitId = "Interstitial_iOS";
    
    private string adUnitId;
    private bool adLoaded = false;

    void Awake()
    {
        if (instance != null) return;
        instance = this;
        adUnitId = (Application.platform == RuntimePlatform.IPhonePlayer) ? iOSAdUnitId : androidAdUnitId;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadAd()
    {
        if (GameInfo.isProEdition) return;
        Debug.Log("Loading Ad: " + adUnitId + "...");
        Advertisement.Load(adUnitId, this);
    }

    public void ShowAd()
    {
        if (GameInfo.isProEdition) return;
        if (!adLoaded) return;
        Debug.Log("Showing Ad: " + adUnitId + "...");
        Advertisement.Show(adUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        adLoaded = true;
        Debug.Log("Ad #" + adUnitId + " successfully loaded!");
    }

    public void OnUnityAdsFailedToLoad(string _adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit: {_adUnitId} - {error.ToString()} - {message}");
    }

    public void OnUnityAdsShowFailure(string _adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {_adUnitId}: {error.ToString()} - {message}");
    }

    public void OnUnityAdsShowStart(string _adUnitId) { }
    public void OnUnityAdsShowClick(string _adUnitId) { }
    public void OnUnityAdsShowComplete(string _adUnitId, UnityAdsShowCompletionState showCompletionState) {
        Debug.Log("Ad #" + adUnitId + " successfully shown!");
        adLoaded = false;
        LoadAd();
    }
}