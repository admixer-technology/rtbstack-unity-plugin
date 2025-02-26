using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

using Plugins.RtbStackSDK;

public class MenuScript : MonoBehaviour
{
    private UIDocument uiDocument;
    private Button _bannerButton, _interstitialButton, _rewardedButton, _nativeButton;
    // You need to retreive request URL from RtbStack UI
    private string _requestURL = "https://us-adx-qa.rtb-stack.com/test?client=eee6d3c9-34cf-4b5c-9033-fb3bb8093bb8";
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnEnable() {
        uiDocument = GetComponent<UIDocument>();

        _bannerButton = uiDocument.rootVisualElement.Q("showBanner") as Button;
        _interstitialButton = uiDocument.rootVisualElement.Q("showInterstitial") as Button;
        _rewardedButton = uiDocument.rootVisualElement.Q("showRewarded") as Button;
        _nativeButton = uiDocument.rootVisualElement.Q("showNative") as Button;

        _bannerButton.RegisterCallback<ClickEvent>(ShowBanner);
        _interstitialButton.RegisterCallback<ClickEvent>(ShowInterstitial);
        _rewardedButton.RegisterCallback<ClickEvent>(ShowRewarded);
        _nativeButton.RegisterCallback<ClickEvent>(ShowNative);
    }

    private void ShowBanner(ClickEvent evt) {
        if (_bannerButton.text == "Show Banner") {
            _bannerButton.text = "Remove Banner";
            LoadBanner();
        } else {
            _bannerButton.text = "Show Banner";
            DestroyBanner();
        }
    }

    private void ShowInterstitial(ClickEvent evt) {
        LoadInterstitial();
    }

    private void ShowRewarded(ClickEvent evt) {
        LoadRewarded();
    }

    private void ShowNative(ClickEvent evt) {
        if (_nativeButton.text == "Show Native") {
            _nativeButton.text = "Remove Native";
            LoadNative();
        } else {
            _nativeButton.text = "Show Native";
            DestroyNative();
        }
    }

    private void LoadBanner() {
        int width = 300;
        int height = 250;

        int [,] sizes = {{width, height}};

        RtbStackSDK.Position position = RtbStackSDK.Position.BOTTOM;
        RtbStackSDK.LoadBanner(_requestURL, "BannerTagID", sizes, position);

        RtbStackSDK.onBannerAdLoadedEvent += onBannerAdLoaded;
        RtbStackSDK.onBannerAdFailedToLoadEvent += onBannerAdFailedToLoad;
        RtbStackSDK.onBannerAdClickedEvent += onBannerAdClicked;
    }

    private void DestroyBanner() {
        RtbStackSDK.DestroyBanner("BannerTagID");
    }

    private void LoadInterstitial() {
        RtbStackSDK.LoadInterstitial(_requestURL, "InterstitialTagID");
        RtbStackSDK.SetInterstitialClickThroughAction(RtbStackSDK.ClickThroughAction.OPEN_DEVICE_BROWSER);
        RtbStackSDK.onInterstitialAdLoadedEvent += onInterstitialAdLoaded;
        RtbStackSDK.onInterstitialAdFailedToLoadEvent += onInterstitialAdFailedToLoad;
        RtbStackSDK.onInterstitialAdClickedEvent += onInterstitialAdClicked;
    }

    private void LoadRewarded() {
        RtbStackSDK.LoadRewarded(_requestURL, "RewardedTagID");
        RtbStackSDK.SetRewardedClickThroughAction(RtbStackSDK.ClickThroughAction.OPEN_DEVICE_BROWSER);
        RtbStackSDK.onRewardedAdLoadedEvent += onRewardedAdLoaded;
        RtbStackSDK.onRewardedAdFailedToLoadEvent += onRewardedAdFailedToLoad;
        RtbStackSDK.onRewardedAdClickedEvent += onRewardedAdClicked;
        RtbStackSDK.onUserEarnedRewardEvent += onUserEarnedReward;
    }

    public void onInterstitialDestroyClicked(){
        RtbStackSDK.DestroyInterstitial();
    }

    public void onRewardedDestroyClicked(){
        RtbStackSDK.DestroyRewarded();
    }

    private void LoadNative() {
        RtbStackSDK.Position position = RtbStackSDK.Position.BOTTOM;
        RtbStackSDK.NativeTemplate template = RtbStackSDK.NativeTemplate.SMALL;

        RtbStackSDK.LoadNative(_requestURL, "NativeTagID", position, template);
        RtbStackSDK.onNativeAdLoadedEvent += onNativeAdLoaded;
        RtbStackSDK.onNativeAdFailedToLoadEvent += onNativeAdFailedToLoad;
        RtbStackSDK.onNativeAdClickedEvent += onNativeAdClicked;
    }

    private void DestroyNative() {
        RtbStackSDK.DestroyNative("NativeTagID");
    }

    // Ad events mapping
    void onBannerAdLoaded(string placementId) {
        Debug.Log("MyCustomLog onBannerAdLoaded "+placementId);
    }

    void onBannerAdFailedToLoad(string placementId, string error) {
        Debug.Log("MyCustomLog onBannerAdFailedToLoad " + placementId + " " + error);
    }

    void onBannerAdClicked(string placementId, string url) {
        Debug.Log("MyCustomLog onBannerAdClicked "+placementId+" "+url);
    }

    void onInterstitialAdLoaded() {
        Debug.Log("MyCustomLog onInterstitialAdLoaded");
        RtbStackSDK.ShowInterstitial();
    }

    void onInterstitialAdFailedToLoad(string error) {
        Debug.Log("MyCustomLog onInterstitialAdFailedToLoad "+error);
    }

    void onInterstitialAdClicked(string url) {
        Debug.Log("MyCustomLog onInterstitialAdClicked "+url);
    }

    void onRewardedAdLoaded() {
        Debug.Log("MyCustomLog onRewardedAdLoaded");
        RtbStackSDK.ShowRewarded();
    }

    void onRewardedAdFailedToLoad(string error) {
        Debug.Log("MyCustomLog onRewardedAdFailedToLoad "+error);
    }

    void onRewardedAdClicked(string url) {
        Debug.Log("MyCustomLog onRewardedAdClicked "+url);
    }

    void onUserEarnedReward(RtbStackSDK.RewardItem item) {
        Debug.Log("MyCustomLog user earned reward "+item.Amount);
    }

    void onNativeAdLoaded() {
        Debug.Log("MyCustomLog onNativeAdLoaded");
        RtbStackSDK.ShowRewarded();
    }

    void onNativeAdFailedToLoad(string error) {
        Debug.Log("MyCustomLog onNativeAdFailedToLoad "+error);
    }

    void onNativeAdClicked(string placementId, string url) {
        Debug.Log("MyCustomLog onNativeAdClicked "+placementId+" "+url);
    }
}
