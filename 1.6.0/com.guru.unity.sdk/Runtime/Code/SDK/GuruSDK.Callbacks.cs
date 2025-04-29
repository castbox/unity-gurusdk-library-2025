namespace Guru
{
    using System;
    
    
    public partial class GuruSDK
    {
        /// <summary>
        /// 回调参数类
        /// </summary>
        public class Callbacks
        {
            /// <summary>
            /// APP 事件
            /// </summary>
            public static class App
            {
                private static Action<bool> _onAppPaused;
                public static event Action<bool> OnAppPaused
                {
                    add => _onAppPaused += value;
                    remove => _onAppPaused -= value;
                }
                internal static void InvokeOnAppPaused(bool isPaused)
                {
                    _onAppPaused?.Invoke(isPaused);
                }
                
                private static Action _onAppQuit;
                public static event Action OnAppQuit
                {
                    add => _onAppQuit += value;
                    remove => _onAppQuit -= value;
                }
                internal static void InvokeOnAppQuit()
                {
                    _onAppQuit?.Invoke();
                }
            }
            
            /// <summary>
            /// GDPR Consent
            /// </summary>
            public static class ConsentFlow
            {
                /// <summary>
                /// 当Consent启动结束后返回状态码
                /// </summary>
                public static event Action<int> OnConsentResult
                {
                    add => _onConsentResult += value;
                    remove => _onConsentResult -= value;
                }
                private static Action<int> _onConsentResult;
                internal static void InvokeOnConsentResult(int code)
                {
                    _onConsentResult?.Invoke(code);
                }
                
                /// <summary>
                /// ATT 状态返回
                /// </summary>
                public static event Action<int> OnAttResult
                {
                    add => _onAttResult += value;
                    remove => _onAttResult -= value;
                }
                private static Action<int> _onAttResult;
                internal static void InvokeOnAttResultCallback(int code)
                {
                    _onAttResult?.Invoke(code);
                }
            }

            /// <summary>
            /// 广告回调
            /// </summary>
            public static class Ads
            {
                private static Action _onAdsInitComplete;
                public static event Action OnAdsInitComplete
                {
                    add => _onAdsInitComplete += value;
                    remove => _onAdsInitComplete -= value;
                }
                internal static void InvokeOnAdsInitComplete()
                {
                    _onAdsInitComplete?.Invoke();
                }
                
                //------------ BANNER -----------------
                private static Action<string> _onBannerADStartLoad;
                public static event Action<string> OnBannerADStartLoad
                {
                    add => _onBannerADStartLoad += value;
                    remove => _onBannerADStartLoad -= value;
                }
                internal static void InvokeOnBannerADStartLoad(string adUnitId)
                {
                    _onBannerADStartLoad?.Invoke(adUnitId);
                }

                private static Action _onBannerADLoaded;
                public static event Action OnBannerADLoaded
                {
                    add => _onBannerADLoaded += value;
                    remove => _onBannerADLoaded -= value;
                }
                internal static void InvokeOnBannerADLoaded()
                {
                    _onBannerADLoaded?.Invoke();
                }
                
                //------------ INTER -----------------
                private static Action<string> _onInterstitialADStartLoad;
                public static event Action<string> OnInterstitialADStartLoad
                {
                    add => _onInterstitialADStartLoad += value;
                    remove => _onInterstitialADStartLoad -= value;
                }
                internal static void InvokeOnInterstitialADStartLoad(string adUnitId)
                {
                    _onInterstitialADStartLoad?.Invoke(adUnitId);
                }
                
                private static Action _onInterstitialADLoaded;
                public static event Action OnInterstitialADLoaded
                {
                    add => _onInterstitialADLoaded += value;
                    remove => _onInterstitialADLoaded -= value;
                }
                internal static void InvokeOnInterstitialADLoaded()
                {
                    _onInterstitialADLoaded?.Invoke();
                }
                
                private static Action _onInterstitialADFailed;
                public static event Action OnInterstitialADFailed
                {
                    add => _onInterstitialADFailed += value;
                    remove => _onInterstitialADFailed -= value;
                }
                internal static void InvokeOnInterstitialADFailed()
                {
                    _onInterstitialADFailed?.Invoke();
                }
                
                private static Action _onInterstitialADClosed;
                public static event Action OnInterstitialADClosed
                {
                    add => _onInterstitialADClosed += value;
                    remove => _onInterstitialADClosed -= value;
                }
                internal static void InvokeOnInterstitialADClosed()
                {
                    _onInterstitialADClosed?.Invoke();
                }

                //------------ REWARD -----------------
                private static Action<string> _onRewardedADStartLoad;
                public static event Action<string> OnRewardedADStartLoad
                {
                    add => _onRewardedADStartLoad += value;
                    remove => _onRewardedADStartLoad -= value;
                }
                internal static void InvokeOnRewardedADStartLoad(string adUnitId)
                {
                    _onRewardedADStartLoad?.Invoke(adUnitId);
                }
                
                private static Action _onRewardedADLoaded;
                public static event Action OnRewardedADLoaded
                {
                    add => _onRewardedADLoaded += value;
                    remove => _onRewardedADLoaded -= value;
                }
                internal static void InvokeOnRewardedADLoaded()
                {
                    _onRewardedADLoaded?.Invoke();
                }
                
                private static Action _onRewardADClosed;
                public static event Action OnRewardedADClosed
                {
                    add => _onRewardADClosed += value;
                    remove => _onRewardADClosed -= value;
                }
                internal static void InvokeOnRewardADClosed()
                {
                    _onRewardADClosed?.Invoke();
                }
                
                private static Action _onRewardADFailed;
                public static event Action OnRewardADFailed
                {
                    add => _onRewardADFailed += value;
                    remove => _onRewardADFailed -= value;
                }
                internal static void InvokeOnRewardADFailed()
                {
                    _onRewardADFailed?.Invoke();
                }
            }

            /// <summary>
            /// 云控参数
            /// </summary>
            public static class Remote
            {
                private static Action<bool> _onRemoteFetchComplete;
                public static event Action<bool> OnRemoteFetchComplete
                {
                    add => _onRemoteFetchComplete += value;
                    remove => _onRemoteFetchComplete -= value;
                }
                internal static void InvokeOnRemoteFetchComplete(bool success)
                {
                    _onRemoteFetchComplete?.Invoke(success);
                }
            }

            /// <summary>
            /// 支付回调
            /// </summary>
            public static class IAP
            {
                private static Action _onIAPInitStart;
                public static event Action OnIAPInitStart
                {
                    add => _onIAPInitStart += value;
                    remove => _onIAPInitStart -= value;
                }
                internal static void InvokeOnIAPInitStart()
                {
                    _onIAPInitStart?.Invoke();
                }
                
                
                private static Action<bool> _onIAPInitComplete;
                public static event Action<bool> OnIAPInitComplete
                {
                    add => _onIAPInitComplete += value;
                    remove => _onIAPInitComplete -= value;
                }
                internal static void InvokeOnIAPInitComplete(bool success)
                {
                    _onIAPInitComplete?.Invoke(success);
                }
                
                
                private static Action<string> _onPurchaseStart;
                public static event Action<string> OnPurchaseStart
                {
                    add => _onPurchaseStart += value;
                    remove => _onPurchaseStart -= value;
                }
                internal static void InvokeOnPurchaseStart(string productId)
                {
                    _onPurchaseStart?.Invoke(productId);
                }
                
                
                private static Action<string, bool> _onPurchaseEnd;
                public static event Action<string, bool> OnPurchaseEnd
                {
                    add => _onPurchaseEnd += value;
                    remove => _onPurchaseEnd -= value;
                }
                internal static void InvokeOnPurchaseEnd(string productId, bool success)
                {
                    _onPurchaseEnd?.Invoke(productId, success);
                }
                
                
                private static Action<string, string> _onPurchaseFailed;
                public static event Action<string, string> OnPurchaseFailed
                {
                    add => _onPurchaseFailed += value;
                    remove => _onPurchaseFailed -= value;
                }
                internal static void InvokeOnPurchaseFailed(string productId, string error)
                {
                    _onPurchaseFailed?.Invoke(productId, error);
                }
                
       
                private static Action<bool, string> _onIAPRestored;
                public static event Action<bool, string> OnIAPRestored
                {
                    add => _onIAPRestored += value;
                    remove => _onIAPRestored -= value;
                }
                internal static void InvokeOnIAPRestored(bool success, string productId)
                {
                    _onIAPRestored?.Invoke(success, productId);
                }
            }


            public static class SDK
            {
                private static Action<bool> _onFirebaseReady;
                public static event Action<bool> OnFirebaseReady
                {
                    add => _onFirebaseReady += value;
                    remove => _onFirebaseReady -= value;
                }
                internal static void InvokeOnFirebaseReady(bool success)
                {
                    _onFirebaseReady?.Invoke(success);
                }
                
                private static Action _onGuruServiceReady;
                public static event Action OnGuruServiceReady
                {
                    add => _onGuruServiceReady += value;
                    remove => _onGuruServiceReady -= value;
                }
                internal static void InvokeOnGuruServiceReady()
                {
                    _onGuruServiceReady?.Invoke();
                }

                private static Action<bool> _onDebuggerDisplayed;
                public static event Action<bool> OnDisplayDebugger
                {
                    add => _onDebuggerDisplayed += value;
                    remove => _onDebuggerDisplayed -= value;
                }
                internal static void InvokeOnDebuggerDisplayed(bool success)
                {
                    _onDebuggerDisplayed?.Invoke(success);
                }
                
                private static Action<bool> _onUserAuthResult;
                public static event Action<bool> OnGuruUserAuthResult
                {
                    add => _onUserAuthResult += value;
                    remove => _onUserAuthResult -= value;
                }
                internal static void InvokeOnGuruUserAuthResult(bool success)
                {
                    _onUserAuthResult?.Invoke(success);
                }
                
                private static Action<string> _onAttResult;
                public static event Action<string> OnAttResult
                {
                    add => _onAttResult += value;
                    remove => _onAttResult -= value;
                }
                internal static void InvokeOnAttResult(string status)
                {
                    _onAttResult?.Invoke(status);
                }
                
                // DeepLink 回调 
                private static Action<string> _onDeeplinkCallback;
                public static event Action<string> OnDeeplinkCallback
                {
                    add => _onDeeplinkCallback += value;
                    remove => _onDeeplinkCallback -= value;
                }
                internal static void InvokeDeeplinkCallback(string deeplink)
                {
                    _onDeeplinkCallback?.Invoke(deeplink);
                }
                
                // TODO: 之后需要添加 define 宏来控制是否可用
                // Firebase Auth 回调
                private static Action<bool, Firebase.Auth.FirebaseUser> _onFirebaseUserAuthResult;
                public static event Action<bool, Firebase.Auth.FirebaseUser> OnFirebaseUserAuthResult
                {
                    add => _onFirebaseUserAuthResult += value;
                    remove => _onFirebaseUserAuthResult -= value;
                }
                internal static void InvokeOnFirebaseAuthResult(bool success, Firebase.Auth.FirebaseUser firebaseUser = null)
                {
                    _onFirebaseUserAuthResult?.Invoke(success, firebaseUser);
                }
                
            }

        }


    }
}