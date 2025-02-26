using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

namespace Plugins.RtbStackSDK {
    public static class RtbStackSDK {

        private const string GAME_OBJECT_NAME = "RtbStackSDKToUnityBridge";
        private static GameObject gameObject;

        private static string lastRequest;

        private static string lastLog;

        public enum ClickThroughAction {
            OPEN_SDK_BROWSER,
            OPEN_DEVICE_BROWSER,
            RETURN_URL
        }

        public enum Position {
            TOP,
            CENTER,
            BOTTOM
        }

        public enum NativeTemplate {
            SMALL,
            MEDIUM
        }


        public class RewardItem {
            public int Amount { get; set;}
            public string Type { get; set;}

        }

        private class OnBannerAdLoaded {
            public string PlacementId {get; set;}
        }

        private class OnBannerAdFailedToLoad {
            public string PlacementId {get; set;}
            public string Error {get; set;}
        }

        private class OnBannerAdClicked {
            public string PlacementId {get; set;}
            public string ClickURL {get; set;}
        }

        /*********************************************************************************
        * Anroid only variables
        **********************************************************************************/
        private const string JAVA_BANNER_CLASS_NAME = "com.rtbstack.sdk.unityplugin.BannerAdViewUnityPlugin";
        private const string JAVA_INTERSTITIAL_CLASS_NAME = "com.rtbstack.sdk.unityplugin.InterstitialAdViewUnityPlugin";
        private const string JAVA_REWARDED_CLASS_NAME = "com.rtbstack.sdk.unityplugin.RewardedAdViewUnityPlugin";
        private const string JAVA_NATIVE_CLASS_NAME = "com.rtbstack.sdk.unityplugin.NativeAdViewUnityPlugin";
        private static Dictionary<string, AndroidJavaObject> banners;
        private static AndroidJavaObject interstitialAdView;
        private static AndroidJavaObject rewardedAdView;
        private static AndroidJavaObject nativeAdView;

        /*********************************************************************************
        * iOS only variables
        **********************************************************************************/
        #pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
        #if UNITY_IOS
        [DllImport("__Internal")]
        #endif
        private static extern void loadBannerIOS(string tagID, string sizes, string position);
        
        #if UNITY_IOS
        [DllImport("__Internal")]
        #endif
        private static extern void destroyBannerIOS(string tagID);
        
        #if UNITY_IOS
        [DllImport("__Internal")]
        #endif
        private static extern void setBannerClickThroughActionIOS(string tagID, string action);

        #if UNITY_IOS
        [DllImport("__Internal")]
        #endif
        private static extern void setBannerAutoRefreshEnabledIOS(string tagID, string autoRefresh);

        #if UNITY_IOS
        [DllImport("__Internal")]
        #endif
        private static extern void loadInterstitialIOS(string tagID);
        
        #if UNITY_IOS
        [DllImport("__Internal")]
        #endif
        private static extern void showInterstitialIOS();
        
        #if UNITY_IOS
        [DllImport("__Internal")]
        #endif
        private static extern void setInterstitialClickThroughActionIOS(string action);
        
        #if UNITY_IOS
        [DllImport("__Internal")]
        #endif
        private static extern void destroyInterstitialIOS();

        #if UNITY_IOS
        [DllImport("__Internal")]
        #endif
        private static extern void loadRewardedIOS(string tagID);
        
        #if UNITY_IOS
        [DllImport("__Internal")]
        #endif
        private static extern void showRewardedIOS();

        #if UNITY_IOS
        [DllImport("__Internal")]
        #endif
        private static extern void setRewardedClickThroughActionIOS(string action);

        #if UNITY_IOS
        [DllImport("__Internal")]
        #endif
        private static extern void destroyRewardedIOS();
        #pragma warning restore CS0626 // Method, operator, or accessor is marked external and has no attributes on it

        /*********************************************************************************
        * Constructor
        **********************************************************************************/

        static RtbStackSDK() {
            gameObject = new GameObject
            {
                name = GAME_OBJECT_NAME
            };
            gameObject.AddComponent<NativeEventsHandler>();
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
        }

        static void InitBannerList() {
            banners = new Dictionary<string, AndroidJavaObject>();
        }

        static void InitInterstitial() {
            interstitialAdView = new AndroidJavaObject(JAVA_INTERSTITIAL_CLASS_NAME);
            interstitialAdView.Call("setBackgroundEventListener", InterstitialBackgroundEventListener.Instance);
        }

        static void InitRewarded() {
            rewardedAdView = new AndroidJavaObject(JAVA_REWARDED_CLASS_NAME);
            rewardedAdView.Call("setBackgroundEventListener", RewardedBackgroundEventListener.Instance);
        }

        static void InitNative() {
            nativeAdView = new AndroidJavaObject(JAVA_NATIVE_CLASS_NAME);
            nativeAdView.Call("setBackgroundEventListener", NativeBackgroundEventListener.Instance);
        }

        /*********************************************************************************
        * Banner methods for C# side
        **********************************************************************************/

        public static void LoadBanner(string requestURL, string tagID, int[,] sizes, Position position) {
            var json = JsonConvert.SerializeObject(sizes);
            switch(Application.platform) {
                case RuntimePlatform.Android: 
                case RuntimePlatform.WindowsEditor:
                    if(banners == null) {
                        InitBannerList();
                    }

                    AndroidJavaObject bannerAdView;
                    if(banners.ContainsKey(tagID) && banners[tagID] != null) {
                        bannerAdView = banners[tagID];
                    } else {
                        bannerAdView = new AndroidJavaObject(JAVA_BANNER_CLASS_NAME);
                        banners[tagID] = bannerAdView;
                    }
                    
                    bannerAdView.Call("setBackgroundEventListener", BannerBackgroundEventListener.Instance);
                    bannerAdView.Call("setAdSizes", json.ToString());
                    bannerAdView.Call("setBannerPosition", position.ToString());
                    bannerAdView.Call("setRequestURL", requestURL);
                    bannerAdView.Call("setTagID", tagID);
                    bannerAdView.Call("loadAd");
                    lastRequest = bannerAdView.Call<string>("getLastRequest");
                    lastLog = bannerAdView.Call<string>("getLogCat");
                    break;
                case RuntimePlatform.IPhonePlayer:
                    loadBannerIOS(tagID, json.ToString(), position.ToString());
                    break;    
            }
        }

        public static void SetBannerClickThroughAction(string tagID, ClickThroughAction action) {
            switch(Application.platform) {
                case RuntimePlatform.Android: 
                    if(banners == null) {
                        InitBannerList();
                    }

                    AndroidJavaObject bannerAdView;
                    if (banners.ContainsKey(tagID) && banners[tagID] != null) {
                        bannerAdView = banners[tagID];
                    } else {
                        bannerAdView = new AndroidJavaObject(JAVA_BANNER_CLASS_NAME);
                        banners[tagID] = bannerAdView;
                    }

                    bannerAdView.Call("setClickThroughAction", action.ToString());
                    break;
                case RuntimePlatform.IPhonePlayer:
                    setBannerClickThroughActionIOS(tagID, action.ToString());
                    break;
            }
        }

        public static void SetBannerAutoRefreshEnabled(string tagID, bool autoRefresh) {
            switch(Application.platform) {
                case RuntimePlatform.Android:
                    if(banners == null) {
                        InitBannerList();
                    }
                    
                    AndroidJavaObject bannerAdView;
                    if(banners.ContainsKey(tagID) && banners[tagID] != null) {
                        bannerAdView = banners[tagID];
                    } else {
                        bannerAdView = new AndroidJavaObject(JAVA_BANNER_CLASS_NAME);
                        banners[tagID] = bannerAdView;
                    }

                    bannerAdView.Call("setAutoRefreshEnabled", autoRefresh);
                    break;
                case RuntimePlatform.IPhonePlayer:
                    if(autoRefresh) {
                        setBannerAutoRefreshEnabledIOS(tagID, "1");
                    } else {
                        setBannerAutoRefreshEnabledIOS(tagID, "0");
                    }
                    break;
            }
        }

        public static void DestroyBanner(string tagID){
            switch(Application.platform) {
                case RuntimePlatform.Android:
                    if(banners == null) {
                        InitBannerList();
                    }
                    if (banners.ContainsKey(tagID) && banners[tagID] != null) {
                        AndroidJavaObject bannerAdView = banners[tagID];
                        bannerAdView.Call("destroyAd");
                        bannerAdView = null;
                        banners.Remove(tagID);
                    }
                    break;
                case RuntimePlatform.IPhonePlayer:{
                    destroyBannerIOS(tagID);
                    break;
                }
            }
        }

        public static string GetLogs() {
            return lastLog;
        }

        /*********************************************************************************
        * Interstitial methods for C# side
        **********************************************************************************/

        public static void LoadInterstitial(string requestURL, string tagID) {
            switch(Application.platform) {
                case RuntimePlatform.Android:
                    if(interstitialAdView == null) {
                        InitInterstitial();
                    }
                    interstitialAdView.Call("setRequestURL", requestURL);
                    interstitialAdView.Call("setTagID", tagID);
                    interstitialAdView.Call("loadAd");
                    lastRequest = interstitialAdView.Call<string>("getLastRequest");
                    break;
                case RuntimePlatform.IPhonePlayer:
                    loadInterstitialIOS(tagID);
                    break;
            }
        }

        public static void ShowInterstitial() {
            switch(Application.platform) {
                case RuntimePlatform.Android:
                    if(interstitialAdView == null) {
                        InitInterstitial();
                    }
                    interstitialAdView.Call("showAd");
                    break;
                case RuntimePlatform.IPhonePlayer:
                    showInterstitialIOS();
                    break;
            }
        }

        public static void SetInterstitialClickThroughAction(ClickThroughAction action) {
            switch(Application.platform) {
                case RuntimePlatform.Android:
                    if(interstitialAdView == null) {
                        InitInterstitial();
                    }
                    interstitialAdView.Call("setClickThroughAction", action.ToString());
                    break;
                case RuntimePlatform.IPhonePlayer:
                    setInterstitialClickThroughActionIOS(action.ToString());
                    break;
            }
        }

        public static void DestroyInterstitial() {
            switch(Application.platform) {
                case RuntimePlatform.Android:
                    if(interstitialAdView != null) {
                        interstitialAdView.Call("destroyAd");
                        interstitialAdView = null;
                    }
                    break;
                case RuntimePlatform.IPhonePlayer:
                    destroyInterstitialIOS();
                    break;    
            }
        }

        /*********************************************************************************
        * Rewarded methods for C# side
        **********************************************************************************/

        public static void LoadRewarded(string requestURL, string tagID) {
            switch(Application.platform) {
                case RuntimePlatform.Android:
                    if(rewardedAdView == null) {
                        InitRewarded();
                    }
                    rewardedAdView.Call("setRequestURL", requestURL);
                    rewardedAdView.Call("setTagID", tagID);
                    rewardedAdView.Call("loadAd");
                    lastRequest = rewardedAdView.Call<string>("getLastRequest");
                    break;
                case RuntimePlatform.IPhonePlayer:
                    loadRewardedIOS(tagID);
                    break;    
            }
        }

        public static void ShowRewarded() {
            switch(Application.platform) {
                case RuntimePlatform.Android:
                    if(rewardedAdView == null) {
                        InitRewarded();
                    }
                    rewardedAdView.Call("showAd");
                    break;
                case RuntimePlatform.IPhonePlayer:
                    showRewardedIOS();
                    break;
            }
        }

        public static void SetRewardedClickThroughAction(ClickThroughAction action) {
            switch(Application.platform) {
                case RuntimePlatform.Android:
                    if(rewardedAdView == null) {
                        InitRewarded();
                    }
                    rewardedAdView.Call("setClickThroughAction", action.ToString());
                    break;
                case RuntimePlatform.IPhonePlayer:
                    setRewardedClickThroughActionIOS(action.ToString());
                    break;
                
            }
        }

        public static void DestroyRewarded(){
            switch(Application.platform) {
                case RuntimePlatform.Android:
                    if(rewardedAdView != null) {
                        rewardedAdView.Call("destroyAd");
                        rewardedAdView = null;
                    }
                    break;
                case RuntimePlatform.IPhonePlayer:
                    destroyRewardedIOS();
                    break;
            }
        }

        /*********************************************************************************
        * Native methods for C# side
        **********************************************************************************/

        public static void LoadNative(string requestURL, string tagID, Position position, NativeTemplate template, string color = "#FFFFFF") {
            switch(Application.platform) {
                    case RuntimePlatform.Android:
                        if(nativeAdView == null) {
                            InitNative();
                        }
                        nativeAdView.Call("setRequestURL", requestURL);
                        nativeAdView.Call("setPosition", position.ToString());
                        nativeAdView.Call("setTemplate", template.ToString());
                        nativeAdView.Call("setBackgroundColor", color);
                        nativeAdView.Call("setTagID", tagID);
                        nativeAdView.Call("loadAd");
                        lastRequest = nativeAdView.Call<string>("getLastRequest");
                        break;  
                }
        }

        public static void DestroyNative(string tagID){
            switch(Application.platform) {
                case RuntimePlatform.Android:
                    if(nativeAdView != null) {
                        nativeAdView.Call("destroyAd");
                        nativeAdView = null;
                    }
                    break;
            }
        }

        public static void SetNativeClickThroughAction(string tagID, ClickThroughAction action) {
            switch(Application.platform) {
                case RuntimePlatform.Android: 
                    if(nativeAdView == null) {
                        InitNative();
                    }

                    nativeAdView.Call("setClickThroughAction", action.ToString());
                    break;
                case RuntimePlatform.IPhonePlayer:
                    setBannerClickThroughActionIOS(tagID, action.ToString());
                    break;
            }
        }


        public static string GetLastRequest() {
            return lastRequest;
        }

        /*********************************************************************************
        * BackgroundEventListener events listener
        **********************************************************************************/

        internal class BannerBackgroundEventListener : AndroidJavaProxy {

            private BannerBackgroundEventListener(): base("com.rtbstack.sdk.unityplugin.UnityBackgroundEventListener") { }

            public static readonly BannerBackgroundEventListener Instance = new BannerBackgroundEventListener();

            public void onEvent(string name, string value) {
                switch(name) {
                    case "OnBannerAdClicked":
                        if(_onBannerAdClickedEvent != null) {
                            var jsonObj = JsonConvert.DeserializeObject<OnBannerAdClicked>(value);
                            _onBannerAdClickedEvent(jsonObj.PlacementId, jsonObj.ClickURL);
                        }
                        break;
                }
            }
        }

        internal class InterstitialBackgroundEventListener : AndroidJavaProxy {

            private InterstitialBackgroundEventListener(): base("com.rtbstack.sdk.unityplugin.UnityBackgroundEventListener") { }

            public static readonly InterstitialBackgroundEventListener Instance = new InterstitialBackgroundEventListener();

            public void onEvent(string name, string value) {
                switch(name) {
                    case "OnInterstitialAdClicked":
                        if(_onInterstitialAdClickedEvent != null) {
                            _onInterstitialAdClickedEvent(value);
                        }
                        break;
                }
            }
        }

        internal class RewardedBackgroundEventListener: AndroidJavaProxy {
            
            private RewardedBackgroundEventListener(): base("com.rtbstack.sdk.unityplugin.UnityBackgroundEventListener") {}

            public static readonly RewardedBackgroundEventListener Instance = new RewardedBackgroundEventListener();

            public void onEvent(string name, string value) {
                switch(name) {
                    case "OnRewardedAdClicked": 
                        if(_onRewardedAdClickedEvent != null) {
                            _onRewardedAdClickedEvent(value);
                        }
                        break;
                    case "OnUserEarnedReward": 
                        if(_onUserEarnedRewawrdEvent != null) {
                            var item = JsonConvert.DeserializeObject<RewardItem>(value);
                            _onUserEarnedRewawrdEvent(item);
                        }
                        break;
                }
            }

        }

        internal class NativeBackgroundEventListener : AndroidJavaProxy {

            private NativeBackgroundEventListener(): base("com.rtbstack.sdk.unityplugin.UnityBackgroundEventListener") { }

            public static readonly NativeBackgroundEventListener Instance = new NativeBackgroundEventListener();

            public void onEvent(string name, string value) {
                switch(name) {
                    case "OnNativeAdClicked":
                        if(_onNativeAdClickedEvent != null) {
                            var jsonObj = JsonConvert.DeserializeObject<OnBannerAdClicked>(value);
                            _onNativeAdClickedEvent(jsonObj.PlacementId, jsonObj.ClickURL);
                        }
                        break;
                    case "OnNativeAdLoaded":
                        if (_onNativeAdLoadedEvent != null) {
                            _onNativeAdLoadedEvent();
                        }
                        break;
                    case "OnNativeAdFailedToLoad":
                        if (_onNativeAdFailedToLoadEvent != null) {
                            _onNativeAdFailedToLoadEvent(value);
                        }
                        break;
                }
            }

        }

        /*********************************************************************************
        * UnitySendMessage events listener
        **********************************************************************************/

        private class NativeEventsHandler: MonoBehaviour {
            private void HandleException(string exception) {
                throw new Exception(exception);
            }

            private void OnBannerAdLoaded(string param) {
                var jsonObj = JsonConvert.DeserializeObject<OnBannerAdLoaded>(param);
                if(_onBannerAdLoadedEvent != null) {
                    _onBannerAdLoadedEvent(jsonObj.PlacementId);
                }
            }

            private void OnBannerAdFailedToLoad(string param) {
                Debug.Log("MyCustomLog "+param);
                var jsonObj = JsonConvert.DeserializeObject<OnBannerAdFailedToLoad>(param);
                if(_onBannerAdFailedToLoadEvent != null) {
                    _onBannerAdFailedToLoadEvent(jsonObj.PlacementId, jsonObj.Error);
                }
            }

            private void OnBannerAdClicked(string param) {
                var jsonObj = JsonConvert.DeserializeObject<OnBannerAdClicked>(param);
                if(_onBannerAdClickedEvent != null) {
                    _onBannerAdClickedEvent(jsonObj.PlacementId, jsonObj.ClickURL);
                }
            }

            private void OnInterstitialAdLoaded() {
                if(_onInterstitialAdLoadedEvent != null) {
                    _onInterstitialAdLoadedEvent();
                }
            }

            private void OnInterstitialAdFailedToLoad(string param) {
                if(_onInterstitialAdFailedToLoadEvent != null) {
                    _onInterstitialAdFailedToLoadEvent(param);
                }
            }

            private void OnInterstitialAdClicked(string param) {
                if(_onInterstitialAdClickedEvent != null) {
                    _onInterstitialAdClickedEvent(param);
                }
            }

            private void OnRewardedAdLoaded() {
                if(_onRewardedAdLoadedEvent != null) {
                    _onRewardedAdLoadedEvent();
                }
            }

            private void OnRewardedAdFailedToLoad(string param) {
                if(_onRewardedAdFailedToLoadEvent != null) {
                    _onRewardedAdFailedToLoadEvent(param);
                }
            }

            private void OnRewardedAdClicked(string param) {
                if(_onRewardedAdClickedEvent != null) {
                    _onRewardedAdClickedEvent(param);
                }
            }

            private void OnUserEarnedReward(string param) {
                if(_onUserEarnedRewawrdEvent != null) {
                    var item = JsonConvert.DeserializeObject<RewardItem>(param);
                    _onUserEarnedRewawrdEvent(item);
                } 
            }

            private void OnNativeAdLoaded() {
                if(_onNativeAdLoadedEvent != null) {
                    _onNativeAdLoadedEvent();
                }
            }

            private void OnNativedAdFailedToLoad(string param) {
                if(_onNativeAdFailedToLoadEvent != null) {
                    _onNativeAdFailedToLoadEvent(param);
                }
            }

            private void OnNativeAdClicked(string param) {
                var jsonObj = JsonConvert.DeserializeObject<OnBannerAdClicked>(param);
                if(_onNativeAdClickedEvent != null) {
                    _onNativeAdClickedEvent(jsonObj.PlacementId, jsonObj.ClickURL);
                }
            }
        }

        /*********************************************************************************
        * Banner callbacks for C# side
        **********************************************************************************/

        private static event Action<string> _onBannerAdLoadedEvent;
        public static event Action<string> onBannerAdLoadedEvent {
            add {
                if(_onBannerAdLoadedEvent == null || !_onBannerAdLoadedEvent.GetInvocationList().Contains(value)) {
                    _onBannerAdLoadedEvent += value;
                }
            }
            remove {
                if(_onBannerAdLoadedEvent != null || _onBannerAdLoadedEvent.GetInvocationList().Contains(value)) {
                    _onBannerAdLoadedEvent -= value;
                }
            }
        }

        private static event Action<string, string> _onBannerAdFailedToLoadEvent;
        public static event Action<string, string> onBannerAdFailedToLoadEvent {
            add {
                if(_onBannerAdFailedToLoadEvent == null || !_onBannerAdFailedToLoadEvent.GetInvocationList().Contains(value)) {
                    _onBannerAdFailedToLoadEvent += value;
                }
            }
            remove {
                if(_onBannerAdFailedToLoadEvent != null || _onBannerAdFailedToLoadEvent.GetInvocationList().Contains(value)) {
                    _onBannerAdFailedToLoadEvent -= value;
                }
            }
        }

        private static event Action<string, string> _onBannerAdClickedEvent;
        public static event Action<string, string> onBannerAdClickedEvent {
            add {
                if(_onBannerAdClickedEvent == null || !_onBannerAdClickedEvent.GetInvocationList().Contains(value)) {
                    _onBannerAdClickedEvent += value;
                }
            }
            remove {
                if(_onBannerAdClickedEvent != null || _onBannerAdClickedEvent.GetInvocationList().Contains(value)) {
                    _onBannerAdClickedEvent -= value;
                }
            }
        }

        /*********************************************************************************
        * Interstitial callbacks for C# side
        **********************************************************************************/
        private static event Action _onInterstitialAdLoadedEvent;
        public static event Action onInterstitialAdLoadedEvent {
            add {
                if (_onInterstitialAdLoadedEvent == null || !_onInterstitialAdLoadedEvent.GetInvocationList ().Contains (value)) {
                    _onInterstitialAdLoadedEvent += value;
                }
            }
            remove {
                if(_onInterstitialAdLoadedEvent != null || _onInterstitialAdLoadedEvent.GetInvocationList().Contains(value)) {
                    _onInterstitialAdLoadedEvent -= value;
                }
            }
        }  

        private static event Action<string> _onInterstitialAdFailedToLoadEvent;
        public static event Action<string> onInterstitialAdFailedToLoadEvent {
            add {
                if (_onInterstitialAdFailedToLoadEvent == null || !_onInterstitialAdFailedToLoadEvent.GetInvocationList().Contains(value)) {
                    _onInterstitialAdFailedToLoadEvent += value;
                }
            }
            remove {
                if(_onInterstitialAdFailedToLoadEvent != null || _onInterstitialAdFailedToLoadEvent.GetInvocationList().Contains(value)) {
                    _onInterstitialAdFailedToLoadEvent -= value;
                }
            }
        }

        private static event Action<string> _onInterstitialAdClickedEvent;
        public static event Action<string> onInterstitialAdClickedEvent {
            add {
                if(_onInterstitialAdClickedEvent == null || !_onInterstitialAdClickedEvent.GetInvocationList().Contains(value)) {
                    _onInterstitialAdClickedEvent += value;
                }
            }
            remove {
                if(_onInterstitialAdClickedEvent != null || _onInterstitialAdClickedEvent.GetInvocationList().Contains(value)) {
                    _onInterstitialAdClickedEvent -= value;
                }
            }
        }

        /*********************************************************************************
        * Rewarded callbacks for C# side
        **********************************************************************************/
        private static event Action _onRewardedAdLoadedEvent;
        public static event Action onRewardedAdLoadedEvent {
            add {
                if(_onRewardedAdLoadedEvent == null || !_onRewardedAdLoadedEvent.GetInvocationList().Contains(value)) {
                    _onRewardedAdLoadedEvent += value;
                }
            }
            remove {
                if(_onRewardedAdLoadedEvent != null || _onRewardedAdLoadedEvent.GetInvocationList().Contains(value)) {
                    _onRewardedAdLoadedEvent -= value;
                }
            }
        }

        private static event Action<string> _onRewardedAdFailedToLoadEvent;
        public static event Action<string> onRewardedAdFailedToLoadEvent {
            add {
                if(_onRewardedAdFailedToLoadEvent == null || !_onRewardedAdFailedToLoadEvent.GetInvocationList().Contains(value)) {
                    _onRewardedAdFailedToLoadEvent += value;
                }
            }
            remove {
                if(_onRewardedAdFailedToLoadEvent != null || _onRewardedAdFailedToLoadEvent.GetInvocationList().Contains(value)) {
                    _onRewardedAdFailedToLoadEvent -= value;
                }
            }
        }

        private static event Action<string> _onRewardedAdClickedEvent;
        public static event Action<string> onRewardedAdClickedEvent {
            add {
                if(_onRewardedAdClickedEvent == null || !_onRewardedAdClickedEvent.GetInvocationList().Contains(value)) {
                    _onRewardedAdClickedEvent += value;
                }
            }
            remove {
                if(_onRewardedAdClickedEvent != null || _onRewardedAdClickedEvent.GetInvocationList().Contains(value)) {
                    _onRewardedAdClickedEvent -= value;
                }
            }
        }

        private static event Action<RewardItem> _onUserEarnedRewawrdEvent;
        public static event Action<RewardItem> onUserEarnedRewardEvent {
            add {
                if(_onUserEarnedRewawrdEvent == null || !_onUserEarnedRewawrdEvent.GetInvocationList().Contains(value)) {
                    _onUserEarnedRewawrdEvent += value;
                }
            }
            remove {
                if(_onUserEarnedRewawrdEvent != null || _onUserEarnedRewawrdEvent.GetInvocationList().Contains(value)) {
                    _onUserEarnedRewawrdEvent -= value;
                }
            }
        }

        /*********************************************************************************
        * Native callbacks for C# side
        **********************************************************************************/

        private static event Action _onNativeAdLoadedEvent;
        public static event Action onNativeAdLoadedEvent {
            add {
                if(_onNativeAdLoadedEvent == null || !_onNativeAdLoadedEvent.GetInvocationList().Contains(value)) {
                    _onNativeAdLoadedEvent += value;
                }
            }
            remove {
                if(_onNativeAdLoadedEvent != null || _onNativeAdLoadedEvent.GetInvocationList().Contains(value)) {
                    _onNativeAdLoadedEvent -= value;
                }
            }
        }

        private static event Action<string> _onNativeAdFailedToLoadEvent;
        public static event Action<string> onNativeAdFailedToLoadEvent {
            add {
                if(_onNativeAdFailedToLoadEvent == null || !_onNativeAdFailedToLoadEvent.GetInvocationList().Contains(value)) {
                    _onNativeAdFailedToLoadEvent += value;
                }
            }
            remove {
                if(_onNativeAdFailedToLoadEvent != null || _onNativeAdFailedToLoadEvent.GetInvocationList().Contains(value)) {
                    _onNativeAdFailedToLoadEvent -= value;
                }
            }
        }

        private static event Action<string, string> _onNativeAdClickedEvent;
        public static event Action<string, string> onNativeAdClickedEvent {
            add {
                if(_onNativeAdClickedEvent == null || !_onNativeAdClickedEvent.GetInvocationList().Contains(value)) {
                    _onNativeAdClickedEvent += value;
                }
            }
            remove {
                if(_onNativeAdClickedEvent != null || _onNativeAdClickedEvent.GetInvocationList().Contains(value)) {
                    _onNativeAdClickedEvent -= value;
                }
            }
        }
    }

}

