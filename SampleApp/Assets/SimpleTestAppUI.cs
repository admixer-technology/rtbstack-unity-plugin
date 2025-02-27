using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Plugins.RtbStackSDK;
using TMPro;
using UnityEngine.SceneManagement;

enum TAB_ORDER {
    BANNER,
    INTERSTITIAL,
    REWARDED,
    NATIVE
}
public class SimpleTestAppUI : MonoBehaviour
{
    private Button _sendButton, _removeBannerButton, _removeNativeButton, _copyLastRequest, _copyLogs;
    private TextField _requestURL;
    private TextField _placementID;
    private TabView _tabView;
    private Tab _bannerTab, _interstitialTab, _rewardedTab;
    private IntegerField _widthBannerEdit, _heightBannerEdit;
    private RadioButtonGroup _positionBanner, _positionNative, _templateNative;
    private UIDocument uiDocument;
    private void Update() {
        if (Input.GetKey(KeyCode.Escape)) {
            uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            RtbStackSDK.DestroyNative(_placementID.text.ToString());
            RtbStackSDK.DestroyBanner(_placementID.text.ToString());
        }
    }
    private void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();

        _sendButton = uiDocument.rootVisualElement.Q("sendRequestButton") as Button;
        _removeBannerButton = uiDocument.rootVisualElement.Q("removeBanner") as Button;
        _removeNativeButton = uiDocument.rootVisualElement.Q("removeNative") as Button;
        _copyLastRequest = uiDocument.rootVisualElement.Q("getLastRequest") as Button;
        _copyLogs = uiDocument.rootVisualElement.Q("copyLogs") as Button;

        _requestURL = uiDocument.rootVisualElement.Q("requestUrlEdit") as TextField;
        _placementID = uiDocument.rootVisualElement.Q("placementIdEdit") as TextField;
        _tabView = uiDocument.rootVisualElement.Q("tabView") as TabView;
        _bannerTab = uiDocument.rootVisualElement.Q("bannerTab") as Tab;
        _interstitialTab = uiDocument.rootVisualElement.Q("interstitialTab") as Tab;
        _rewardedTab = uiDocument.rootVisualElement.Q("rewardedTab") as Tab;
        _widthBannerEdit = uiDocument.rootVisualElement.Q("widthEdit") as IntegerField;
        _heightBannerEdit = uiDocument.rootVisualElement.Q("heightEdit") as IntegerField;
        _positionBanner = uiDocument.rootVisualElement.Q("positionGroup") as RadioButtonGroup;
        _positionNative = uiDocument.rootVisualElement.Q("positionGroupNative") as RadioButtonGroup;
        _templateNative = uiDocument.rootVisualElement.Q("templateGroupNative") as RadioButtonGroup;

        _sendButton.RegisterCallback<ClickEvent>(SendRequest);
        _removeBannerButton.RegisterCallback<ClickEvent>(DestroyBanner);
        _removeNativeButton.RegisterCallback<ClickEvent>(DestroyNative);
        _copyLastRequest.RegisterCallback<ClickEvent>(CopyLastRequest);
        _copyLogs.RegisterCallback<ClickEvent>(CopyLogs);
    }

    private void SendRequest(ClickEvent evt) {
        if (_requestURL.text.ToString() == "") {

        } else {
            switch ((TAB_ORDER)_tabView.selectedTabIndex) {
                case TAB_ORDER.BANNER:
                    LoadBanner();
                break;
                case TAB_ORDER.INTERSTITIAL:
                    LoadInterstitial();
                break;
                case TAB_ORDER.REWARDED:
                    LoadRewarded();
                break;
                case TAB_ORDER.NATIVE:
                    LoadNativeAd();
                break;
            }
            uiDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
    }

    private void CopyLastRequest(ClickEvent evt) {
        GUIUtility.systemCopyBuffer = RtbStackSDK.GetLastRequest();
    }

    private void CopyLogs(ClickEvent evt) {
        GUIUtility.systemCopyBuffer = RtbStackSDK.GetLogs();
    }

    private void LoadBanner() {
        int width = _widthBannerEdit.value;
        int height = _heightBannerEdit.value;

        int [,] sizes = {{width, height}};

        RtbStackSDK.Position position = RtbStackSDK.Position.BOTTOM;

        if (_positionBanner.value >= 0 && _positionBanner.value < 3) {
            position = (RtbStackSDK.Position) _positionBanner.value;
        }

        RtbStackSDK.LoadBanner(_requestURL.text.ToString(), _placementID.text.ToString(), sizes, position);

        RtbStackSDK.onBannerAdLoadedEvent += onBannerAdLoaded;
        RtbStackSDK.onBannerAdFailedToLoadEvent += onBannerAdFailedToLoad;
        RtbStackSDK.onBannerAdClickedEvent += onBannerAdClicked;
    }

    private void DestroyBanner(ClickEvent evt) {
        RtbStackSDK.DestroyBanner(_placementID.text.ToString());
    }

    private void LoadInterstitial() {
        // Load Interstitial Ad
        RtbStackSDK.LoadInterstitial(_requestURL.text.ToString(), _placementID.text.ToString());
        RtbStackSDK.SetInterstitialClickThroughAction(RtbStackSDK.ClickThroughAction.OPEN_DEVICE_BROWSER);
        RtbStackSDK.onInterstitialAdLoadedEvent += onInterstitialAdLoaded;
        RtbStackSDK.onInterstitialAdFailedToLoadEvent += onInterstitialAdFailedToLoad;
        RtbStackSDK.onInterstitialAdClickedEvent += onInterstitialAdClicked;
    }

    private void LoadRewarded() {
        // Load Rewarded Ad
        RtbStackSDK.LoadRewarded(_requestURL.text.ToString(), _placementID.text.ToString());
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

    private void LoadNativeAd() {
        RtbStackSDK.Position position = RtbStackSDK.Position.BOTTOM;
        if (_positionNative.value >= 0 && _positionNative.value < 3) {
            position = (RtbStackSDK.Position) _positionNative.value;
        }

        RtbStackSDK.NativeTemplate template = RtbStackSDK.NativeTemplate.SMALL;
        if (_templateNative.value >= 0 && _templateNative.value < 3) {
            template = (RtbStackSDK.NativeTemplate) _templateNative.value;
        }

        RtbStackSDK.LoadNative(_requestURL.text.ToString(), _placementID.text.ToString(), position, template, "#FFFFFF");
        RtbStackSDK.onNativeAdLoadedEvent += onNativeAdLoaded;
        RtbStackSDK.onNativeAdFailedToLoadEvent += onNativeAdFailedToLoad;
        RtbStackSDK.onNativeAdClickedEvent += onNativeAdClicked;
    }

    private void DestroyNative(ClickEvent evt) {
        RtbStackSDK.DestroyNative(_placementID.text.ToString());
    }
    // Ad events

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