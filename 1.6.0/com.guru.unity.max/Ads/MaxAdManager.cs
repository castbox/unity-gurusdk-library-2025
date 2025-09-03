
using Cysharp.Threading.Tasks;

namespace Guru.Ads.Max
{
    using System;
    using UnityEngine;
    using Guru.Ads;
    
    /// <summary>
    /// Max 广告管理器
    /// </summary>
    public class MaxAdManager: IAdManager, IMaxDebugger
    {
        private const string LOG_TAG = "[Max]";
        private readonly MaxBannerLoader _bannerLoader;
        private readonly MaxInterstitialLoader _interstitialLoader;
        private readonly MaxRewardedLoader _rewardedLoader;
        private readonly Action _onInitComplete;
        private readonly bool _isDebug;
        private readonly bool? _invokeEventOnMainThread;
        
        private bool _isNoAds;
        private bool _isReady;
        
        public MaxAdManager(IAdEventObserver eventObserver, AdMediationProfile mediationProfile, Action OnInitComplete, bool? invokeEventOnMainThread = null)
        {
            _isReady = false;
            _onInitComplete = OnInitComplete;
            _isNoAds = mediationProfile.isNoAds;
            _isDebug = mediationProfile.debugModeEnabled;
            _invokeEventOnMainThread = invokeEventOnMainThread;
            
            
            // --- preloader ---
            var amazonLoader = MaxCustomAmazonLoader.GetLoader(
                mediationProfile.amazonAppId, 
                mediationProfile.amazonBannerId, mediationProfile.amazonInterstitialId, mediationProfile.amazonRewardedId, 
                mediationProfile.bannerUnitId, mediationProfile.interstitialUnitId, mediationProfile.rewardedUnitId,
                mediationProfile.debugModeEnabled);
            
            // Pubmatic
            var pubmaticLoader = new MaxCustomLoaderPubmatic(mediationProfile.storeUrl); // Pubmatic 初始化即可
            
            // --- proxies ----
            _bannerLoader = new MaxBannerLoader(mediationProfile.bannerUnitId, mediationProfile.bannerWidth,
                mediationProfile.bannerBgColorHex, amazonLoader, eventObserver, mediationProfile.enableAdaptiveBanner);
            _interstitialLoader = new MaxInterstitialLoader(mediationProfile.interstitialUnitId, amazonLoader, eventObserver);
            _rewardedLoader = new MaxRewardedLoader(mediationProfile.rewardedUnitId, amazonLoader, eventObserver);

            if (mediationProfile.debugModeEnabled)
            {
                Debug.Log($"{LOG_TAG} --- Debug is enabled ---");
                Debug.Log(mediationProfile.ToString());
            }

            // --- Segments ---
            CreateMaxSegment(mediationProfile);
            
            // SDK 初始化
            CallMaxSdkInit();
        }

        public string GetMediationName() => "MAX";

        public string GetLogTag() => LOG_TAG;
        
        /// <summary>
        /// 调用 SDK 的初始化逻辑
        /// </summary>
        private void CallMaxSdkInit()
        {
            if (_isReady) return;
            MaxSdkCallbacks.OnSdkInitializedEvent += OnSdkInitializedEvent;
            Debug.Log($"{LOG_TAG} --- Call MaxSdk.InitializeSdk ---");
            MaxSdk.InitializeSdk();
        }
        
        private void OnSdkInitializedEvent(MaxSdkBase.SdkConfiguration config)
        {
            MaxSdkCallbacks.OnSdkInitializedEvent -= OnSdkInitializedEvent;
         
            _isReady = config.IsSuccessfullyInitialized;
            if (!_isReady)
            {
                Debug.LogWarning($"{LOG_TAG} --- Max init failed, try again");
                CallMaxSdkInit();
                return;
            }
            
            // ---- Init Success ----
            // 设置事件回调是否在主线程执行
            if(_invokeEventOnMainThread != null)
                MaxSdk.InvokeEventsOnUnityMainThread = _invokeEventOnMainThread;
            
            MaxSdk.SetMuted(false);
            MaxSdk.SetVerboseLogging(_isDebug);
            MaxSdk.SetExtraParameter("enable_black_screen_fixes", "true"); // 修复黑屏
            
            // Pangle 在广告初始化完成的时候开启设置
            var pangleLoader = new MaxCustomLoaderPangle(true); // 强制开启日志打印
            
            Debug.Log($"{LOG_TAG} --- Init Success, with noAds: {_isNoAds}");
            // TODO: 提供参数 工项目组可以手动启动加载 Banner, IV 和 RV
            // 未购买去广告， 则自动开始加载 Banner 和 Inter
            if (!_isNoAds) _bannerLoader.Load();
            if (!_isNoAds) _interstitialLoader.Load();
            _rewardedLoader.Load(); // Rewarded 默认都会开始加载

            _onInitComplete?.Invoke();
        }

        #region MaxSegments

        /// <summary>
        /// 创建 MaxSegments 对象
        /// </summary>
        /// <param name="mediationProfile"></param>
        private void CreateMaxSegment(AdMediationProfile mediationProfile)
        {
            var segmentsManager = new MaxSegmentsManager(new MaxSegmentsProfile()
            {
                buildNumberStr = mediationProfile.appVersionCode,
                osVersionStr = mediationProfile.osVersionStr,
                isPaidUser = mediationProfile.isIapUser,
                firstInstallDate = mediationProfile.firstInstallDate,
                networkStatus = mediationProfile.networkStatus
            });
        }

        #endregion

        #region Banner
        
        public void ShowBanner(string placement = "")
        {
            if (_isNoAds)
            {
                Debug.LogWarning($"{LOG_TAG} --- User has already buy no ads. Banner should be hidden!");
                return;
            }

            _bannerLoader.Show(placement).Forget();
        }

        public void HideBanner()
        {
            _bannerLoader.Hide();
        }

        public bool IsBannerVisible()
        {
            return _bannerLoader.IsBannerVisible;
        }

        public Rect GetBannerLayout()
        {
            return _bannerLoader.GetBannerLayout();
        }

        public void SetBannerAdUnitId(string adUnitId)
        {
            _bannerLoader.SetAdUnitId(adUnitId);
        }

        private void DisableBanner()
        {
            _bannerLoader.Enabled = false;
        }
        

        #endregion

        #region InterStitial 插屏广告

        // --- Interstitial ---
        public void LoadInterstitial()
        {
            _interstitialLoader.Load();
        }

        public bool IsInterstitialReady()
        {
            if (!_isReady)
            {
                Debug.LogError($"{LOG_TAG} --- MaxAdManager Is not Ready, init ManagerFirst!");
                return false;
            }
            return _interstitialLoader.IsAdReady();
        }

        public void ShowInterstitial(string placement = "")
        {
            _interstitialLoader.Show(placement);
        }

        public void SetInterstitialAdUnitId(string adUnitId)
        {
            _interstitialLoader.SetAdUnitId(adUnitId);
        }

        #endregion

        #region Rewarded 激励视频
        
        // --- Rewarded ---
        public void LoadRewarded()
        {
            _rewardedLoader.Load();
        }

        public bool IsRewardedReady()
        {
            if (!_isReady)
            {
                Debug.LogError($"{LOG_TAG} --- MaxAdManager Is not Ready, init ManagerFirst!");
                return false;
            }
            return _rewardedLoader.IsAdReady();
        }

        public void ShowRewarded(string placement = "")
        {
            Debug.Log($"{LOG_TAG} --- MaxAdManager Show RV:{placement}");
            _rewardedLoader.Show(placement);
        }

        public void SetRewardedAdUnitId(string adUnitId)
        {
            _rewardedLoader.SetAdUnitId(adUnitId);
        }

        #endregion
        
        #region 生命周期
        
        /// <summary>
        /// 是否 Ready
        /// </summary>
        /// <returns></returns>
        public bool IsReady()
        {
            return _isReady;
        }
        
        /// <summary>
        /// 激活去广告
        /// </summary>
        public void EnableNoAds()
        {
            if (_isNoAds) return; // 已经购买了去广告，不做处理
            _isNoAds = true;

            DisableBanner();
        }


        /// <summary>
        /// 应用切后台
        /// </summary>
        /// <param name="paused"></param>
        public void SetAppPause(bool paused)
        {
            if (paused)
            {
                if (_bannerLoader.IsBannerVisible)
                {
                    _bannerLoader.SetAutoRefresh(false);
                }
            }
            else
            {
                if (_bannerLoader.IsBannerVisible)
                {
                    _bannerLoader.SetAutoRefresh(true);
                }
            }            
        }
        

        #endregion

        #region 调式接口

        /// <summary>
        /// 显示 Mediation Debugger
        /// </summary>
        public void ShowMediationDebugger()
        {
            MaxSdk.ShowMediationDebugger();
        }
        
        /// <summary>
        /// 显示 Creative Debugger
        /// </summary>
        public void ShowCreativeDebugger()
        {
            MaxSdk.ShowCreativeDebugger();
        }
        
        

        #endregion
    }
}