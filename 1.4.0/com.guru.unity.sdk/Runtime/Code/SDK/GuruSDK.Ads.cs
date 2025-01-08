
namespace Guru
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Guru.Notification;
    using Guru.Ads;
    
    public partial class GuruSDK
    {
        private const float CONSENT_FLOW_TIMEOUT = 10; // Consent Flow 超时时间（秒）   
        private string _attType;

        private static readonly HashSet<string> BlockedAdEvents = new HashSet<string>()
        {
            Analytics.EventBadsLoaded,
            Analytics.EventBadsFailed,
            Analytics.EventBadsPaid,
        };
        
        /// <summary>
        /// 启动广告服务
        /// </summary>
        public static void StartAds(bool buyNoAds = false)
        {
            if (InitConfig.UseCustomConsent)
            {
                LogE($"{LOG_TAG} --- Call <color=orange>StartAdsWithCustomConsent</color> when you use custom consent, and pass the result (boolean) to the method.");
                return;
            }
            
            // 默认的启动顺序是先启动Consent后, 根据用户回调的结果来启动广告
            Instance.StartConsentFlow();
        }

        /// <summary>
        /// 使用自定义的Consent, 获取用户授权后, 调用此方法
        /// </summary>
        /// <param name="userAllow"></param>
        /// <param name="consentName">Consent 引导的类型, 如果使用了 MAX 的 consent 请填写 max </param>
        /// <param name="spec">广告启动配置</param>
        public static void StartAdsWithCustomConsent(bool userAllow = true, 
            string consentName = "custom", AdsInitSpec spec = null)
        {
#if UNITY_IOS
            InitAttStatus(consentName);
#endif
            if (userAllow)
            {
#if UNITY_IOS
                Instance.CheckAttStatus();
#else
                StartAdService(spec);
#endif
            }
            else
            {
                LogI($"{LOG_TAG} --- User refuse to provide ads Id, Ads Service will be cancelled");
            }
        }

        /// <summary>
        /// 使用自定义的Consent, 获取用户授权后, 调用此方法
        /// </summary>
        /// <param name="userAllow">自定义 Consent 的用户授权结果</param>
        /// <param name="consentName">Consent引导名称</param>
        /// <param name="buyNoAds">是否已经购买了去广告</param>
        public static void StartAdsWithCustomConsent(bool userAllow = true, string consentName = "custom",
            bool buyNoAds = false)
        {
            StartAdsWithCustomConsent(userAllow, consentName, AdsInitSpec.BuildWithNoAds());
        }


        #region Guru Consent

        private bool _hasConsentCalled = false;
        private bool _adServiceHasStarted = false;
        private string _notiStatue = "";
        private bool _hasNotiGranted = false;

        /// <summary>
        /// 启动Consent流程
        /// 因为之后规划广告流程会放在 Consent 初始化之后, 因此请求广告的时候会需要先请求 Consent
        /// </summary>
        private void StartConsentFlow()
        {
            LogI($"#4.5 ---  StartConsentFlow ---");
            
            float time = 1;
            if (!_adServiceHasStarted && _appServicesConfig != null)
            {
                time = _appServicesConfig.GetIsAdsCompliance() ? CONSENT_FLOW_TIMEOUT : 1f; // 启动合规判定后, 延迟最多 10 秒后启动广告
            }
            Delay(time, AdServiceHandler); // 广告延迟启动
            
            if (_hasConsentCalled) return;
            _hasConsentCalled = true;

            bool enableCountryCheck = false;
            string dmaMapRule = "";
            
            if (_appServicesConfig != null && _appServicesConfig.parameters != null)
            {
                enableCountryCheck = _appServicesConfig.GetDMACountryCheck();
                dmaMapRule = _appServicesConfig.GetDMAMapRule();
            }

#if UNITY_IOS
            InitAttStatus(); // Consent 启动前记录 ATT 初始值
#endif
            UnityEngine.Debug.Log($"{LOG_TAG}  --- Call:StartConsentFlow ---");
            GuruConsent.StartConsent(OnGuruConsentOver, dmaMapRule:dmaMapRule, enableCountryCheck:enableCountryCheck);
        }

        /// <summary>
        /// Guru Consent flow is Over
        /// </summary>
        /// <param name="code"></param>
        private void OnGuruConsentOver(int code)
        {
            
            // 无论状态如何, 都在回调内启动广告初始化
            AdServiceHandler();

            // 调用回调
            Callbacks.ConsentFlow.InvokeOnConsentResult(code);
            
#if UNITY_IOS
            CheckAttStatus();  // [iOS] Consent 启动后检查 ATT 初始值
#elif UNITY_ANDROID
            CheckNotiPermission(); // Consent 回调后检查 Notification 权限
#endif
            
            // 内部处理后继逻辑
            switch(code)
            {
                case GuruConsent.StatusCode.OBTAINED:
                case GuruConsent.StatusCode.NOT_AVAILABLE:
                    // 已获取授权, 或者地区不可用, ATT 尚未启动
                    // TODO: 添加后继处理逻辑
                    break;
            }
        }

        /// <summary>
        /// 启动广告服务
        /// </summary>
        private void AdServiceHandler()
        {
            if (_adServiceHasStarted) return;
            _adServiceHasStarted = true;
            StartAdService();
        }

        
        #endregion
        
        #region IOS ATT 广告授权流程
        
#if UNITY_IOS
        
        /// <summary>
        /// 显示系统的 ATT 弹窗
        /// </summary>
        public void RequestAttDialog(Action<string> onAttResult = null)
        {
            LogI($"RequestATTDialog");
            ATTManager.Instance.RequestATTDialog(onAttResult);
        }
        
        
        /// <summary>
        /// 初始化 ATT 状态
        /// </summary>
        private static void InitAttStatus(string attGuideType = "")
        {
            if (string.IsNullOrEmpty(attGuideType))
                attGuideType = InitConfig.UseCustomConsent ? ATTManager.GUIDE_TYPE_CUSTOM : ATTManager.GUIDE_TYPE_ADMOB; // 点位属性确定
            
            ATTManager.Instance.SetInitAttGuidType(attGuideType);
        }
        
        /// <summary>
        /// iOS 平台检查 ATT 状态
        /// </summary>
        private void CheckAttStatus(bool autoReCall = false)
        {
            ATTManager.Instance.CheckStatus(OnCheckAttStatusComplete, autoReCall);
        }
        
        private void OnCheckAttStatusComplete()
        {
            var status = ATTManager.Instance.GetAttStatusString();
            LogI($"[SDK] ---- OnCheckAttStatusComplete");
            AdjustService.Instance.CheckNewAttStatus(); // 通知 Adjust 更新一下 ATT 状态
            Callbacks.SDK.InvokeOnAttResult(status); // 返回 ATT 状态回调
            CheckNotiPermission();
        }
        
#endif

        #endregion

        #region Notification Permission Check
        
        /// <summary>
        /// 初始化 Noti Service
        /// </summary>
        private void InitNotiPermission()
        {
            // bool hasNotiGranted = false;
            _notiStatue = "no_determined";
            NotificationService.Initialize(); // 初始化 Noti 服务
            Analytics.SetNotiPerm(NotificationService.GetStatus());
        }
        
        /// <summary>
        /// 检查 Noti 状态
        /// </summary>
        private void CheckNotiPermission()
        {
            bool isGranted = NotificationService.IsPermissionGranted();
            LogI($"[SDK] ---- Check Noti Permission: {isGranted}");
            if (isGranted)
            {
                var status = NotificationService.GetStatus();
                LogI($"[SDK] ---- Set Notification Permission: {status}");
                Analytics.SetNotiPerm(status);
                NotificationService.CreatePushChannels();
            }
            else
            {
                // 如果未启用自动 Noti 授权，则直接上报状态
                if (!_initConfig.AutoNotificationPermission)
                {
                    LogW($"[SDK] ---- AutoNotificationPermission is OFF, Project should request permission own.");
                    return;
                }
                RequestNotificationPermission(); // 请求授权
            }
        }
        
        /// <summary>
        /// 请求推送授权
        /// </summary>
        /// <param name="callback"></param>
        public static void RequestNotificationPermission(Action<string> callback = null)
        {
            FirebaseUtil.StartFetchFcmToken();
            
            LogI($"[SDK] ---- RequestNotificationPermission");
            NotificationService.RequestPermission(status =>
            {
                LogI($"[SDK] ---- Set Notification Permission: {status}");
                if(!string.IsNullOrEmpty(status)) Analytics.SetNotiPerm(status);
                // 创建 Push 渠道
                if (NotificationService.IsPermissionGranted())
                {
                    NotificationService.CreatePushChannels();
                }
                callback?.Invoke(status);
            });
        }
        
        /// <summary>
        /// 获取 Notification 状态值
        /// </summary>
        /// <returns></returns>
        public static string GetNotificationStatus()
        {
            return NotificationService.GetStatus();
        }
        
        /// <summary>
        /// 用户是否已经获取了 Notification 授权了
        /// </summary>
        /// <returns></returns>
        public static bool IsNotificationPermissionGranted()
        {
            return NotificationService.IsPermissionGranted();
        }

        #endregion
        
        #region Ad Services

        private static bool _initAdsCompleted = false;
        public static bool IsAdsReady => _initAdsCompleted;
        private static int _preBannerAction = 0;
        
        public static AdsInitSpec GetDefaultAdsSpec()
        {
            return AdsInitSpec.BuildDefault(InitConfig.AutoLoadWhenAdsReady, DebugModeEnabled);
        }


        /// <summary>
        /// 启动广告服务
        /// </summary>
        private static void StartAdService(AdsInitSpec spec = null)
        {
            //---------- Using InitConfig ----------
            if (InitConfig != null && InitConfig.IsBuyNoAds)
            {
                SetBuyNoAds(true);
            }

            LogI($"StartAdService");
            // if (spec == null)
            // {
            //     spec = AdsInitSpec.BuildDefault(InitConfig.AutoLoadWhenAdsReady, DebugModeEnabled);
            //     if (Model.IsNoAds)
            //     {
            //         spec = AdsInitSpec.BuildWithNoAds(InitConfig.AutoLoadWhenAdsReady, DebugModeEnabled);
            //     }
            // }
            //
            // if(InitConfig != null && !string.IsNullOrEmpty(InitConfig.BannerBackgroundColor)) 
            //     spec.bannerColorHex = InitConfig.BannerBackgroundColor;
            
            //--------- Add Callbacks -----------
            // 挂载事件监听，公布给项目组进行对应的事件处理
            // BANNER
            AdService.Instance.OnBadsLoad += OnBannerStartLoad;
            AdService.Instance.OnBadsLoaded += OnBannerLoaded;
            // INTER
            AdService.Instance.OnIadsLoad += OnInterstitialStartLoad;
            AdService.Instance.OnIadsLoaded += OnInterstitialLoaded;
            AdService.Instance.OnIadsFailed += OnInterstitialFailed;
            AdService.Instance.OnIadsClose += OnInterstitialClose;
            // REWARDED
            AdService.Instance.OnRadsLoad += OnRewardStartLoad;
            AdService.Instance.OnRadsLoaded += OnRewardLoaded;
            AdService.Instance.OnRadsFailed += OnRewardFailed;
            AdService.Instance.OnRadsClose += OnRewardClose;
            
            // ---------- Start Services ----------
            UnityEngine.Debug.Log($"--- StartAdService ---");
            
            try
            {
                // 创建广告管理器
                var mediationProfile = GetAdsMediationProfile();
                IAdManager adManager = null;
                switch (mediationProfile.mediationType)
                {
                    case AdMediationType.IronSource:
                        //TODO: 创建 IS 广告管理器
                        break;
                    default:
                        adManager = new Guru.Ads.Max.MaxAdManager(AdService.Instance, mediationProfile, OnAdsInitComplete);
                        break;
                }
                
                // 启动广告服务
                AdService.Instance.StartService(adManager, BlockedAdEvents); // 传入广告管理器以及事件过滤列表
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"--- StartAdService Failed: {e.Message}");
            }
            
            // ---------- Life Cycle ----------
            Callbacks.App.OnAppPaused += OnAppPaused;
        }


        public static double TchADRev001Value => AdService.Instance.Model?.TchAD001RevValue ?? 0;
        public static double TchADRev02Value => AdService.Instance.Model?.TchAD02RevValue ?? 0; 
        
        
        #endregion

        #region 广告初始化参数
        
        // 创建初始化配置
        private static AdMediationProfile GetAdsMediationProfile()
        {
            if(DebugModeEnabled) UnityEngine.Debug.Log($"--- GetAdsMediationProfile ---");
            
            // 获取广告 ID   
            _appServicesConfig.ad_settings.TryGetMaxUnitIds(
                out var bannerId,
                out var interstitialId, 
                out var rewardedId);
            if(DebugModeEnabled) UnityEngine.Debug.Log($"\t Ads Unit IDs: \nbads:{bannerId}\niads:{interstitialId}\nrads:{rewardedId}");
            
            _appServicesConfig.ad_settings.TryGetAmazonUnitIds(
                out var amazonAppId, 
                out var amazonBannerId, 
                out var amazonInterstitialId, 
                out var amazonRewardedId);
            if(DebugModeEnabled) UnityEngine.Debug.Log($"\t Amazon Unit IDs: \n amazonAppId:{amazonAppId}\n amazonBannerId:{amazonBannerId}\n amazonInterstitialId:{amazonInterstitialId}\n amazonRewardedId:{amazonRewardedId}");
            
            // 商店链接
            if(DebugModeEnabled) UnityEngine.Debug.Log($"\t Model: {Model}");
            if(DebugModeEnabled) UnityEngine.Debug.Log($"\t InitConfig: {InitConfig}");
            
            // Mediator 类型
            var debugModel = DebugModeEnabled;
            if(DebugModeEnabled) UnityEngine.Debug.Log($"\t debugModel: {debugModel}");
            var mediatorType = AdMediationType.Max; // TODO：改为用云控获取创建广告聚合的类型
            if(DebugModeEnabled) UnityEngine.Debug.Log($"\t Mediator Type: {mediatorType}");
            var storeUrl = _appServicesConfig.GetStoreUrl();
            if(DebugModeEnabled) UnityEngine.Debug.Log($"\t storeUrl: {storeUrl}");
            bool isNoAds = Model.IsNoAds;
            if(DebugModeEnabled) UnityEngine.Debug.Log($"\t isNoAds: {isNoAds}");
            var uid = UID;
            if(DebugModeEnabled) UnityEngine.Debug.Log($"\t UID: {uid}");
            var bannerColor = InitConfig.BannerBgColor;
            if(DebugModeEnabled) UnityEngine.Debug.Log($"\t bannerColor: {bannerColor}");
            var bannerWidth = InitConfig.BannerWidth;
            if(DebugModeEnabled) UnityEngine.Debug.Log($"\t bannerWidth: {bannerWidth}");
            var osVersion = Instance.GetOSVersionStr();
            if(DebugModeEnabled) UnityEngine.Debug.Log($"\t osVersion: {osVersion}");
            var isIapUser = Model.IsIapUser;
            if(DebugModeEnabled) UnityEngine.Debug.Log($"\t isIapUser: {isIapUser}");
            var firstOpenTime = IPMConfig.GetFirstOpenDate();
            if(DebugModeEnabled) UnityEngine.Debug.Log($"\t firstOpenTime: {firstOpenTime}");
            var appVersionCode = Instance.GetVersionCodeStr();
            if(DebugModeEnabled) UnityEngine.Debug.Log($"\t appVersionCode: {appVersionCode}");
            var networkStatus = Instance.GetNetworkStatus();
            if(DebugModeEnabled) UnityEngine.Debug.Log($"\t networkStatus: {networkStatus}");
            var previousDate = AdService.Instance.Model.PreviousFBAdRevenueDate;
            if(DebugModeEnabled) UnityEngine.Debug.Log($"\t previousDate: {previousDate}");
            
            // 构建初始化配置
            var config = new AdInitConfigBuilder()
                .SetMediationType(mediatorType)
                .SetBannerUnitId(bannerId)
                .SetInterstitialUnitId(interstitialId)
                .SetRewardedUnitId(rewardedId)
                .SetAmazonAppId(amazonAppId)
                .SetAmazonBannerId(amazonBannerId)
                .SetAmazonInterstitialId(amazonInterstitialId)
                .SetAmazonRewardedId(amazonRewardedId)
                .SetStoreUrl(storeUrl)
                .SetIsNoAds(isNoAds)
                .SetUserId(uid)
                .SetBannerBackColorHex(bannerColor)
                .SetBannerWidth(bannerWidth)
                .SetDebugModeEnabled(debugModel)
                .SetOSVersionStr(osVersion)
                .SetIsIapUser(isIapUser)
                .SetFirstInstallDate(firstOpenTime)
                .SetPreviousFBAdRevenueDate(previousDate)
                .SetVersionCodeStr(appVersionCode)
                .SetNetworkStatus(networkStatus)
                .Build();
            return config;
        }

        
        /// <summary>
        /// 获取系统 OS 版本号字符串
        /// </summary>
        /// <returns></returns>
        private string GetOSVersionStr()
        {
#if UNITY_ANDROID
            return $"{DeviceUtil.GetAndroidOSVersionInt()}";
#elif UNITY_IOS
            return DeviceUtil.GetIOSOSVersionString();
#endif
            return "0";
        }
        
        /// <summary>
        /// 获取应用构建 Code 号
        /// </summary>
        /// <returns></returns>
        private string GetVersionCodeStr()
        {
            return GuruAppVersion.Load()?.buildNumber ?? "1";
        }
        
        #endregion

        #region 对外暴露的广告行为回调

        private static void OnBannerStartLoad(BadsLoadEvent evt)
            => Callbacks.Ads.InvokeOnBannerADStartLoad(evt.adUnitId); 
        private static void OnBannerLoaded(BadsLoadedEvent evt) 
            => Callbacks.Ads.InvokeOnBannerADLoaded();
        private static void OnInterstitialStartLoad(IadsLoadEvent evt) 
            => Callbacks.Ads.InvokeOnInterstitialADStartLoad(evt.adUnitId);
        private static void OnInterstitialLoaded(IadsLoadedEvent evt) 
            => Callbacks.Ads.InvokeOnInterstitialADLoaded();
        private static void OnInterstitialFailed(IadsFailedEvent evt)
            => Callbacks.Ads.InvokeOnInterstitialADFailed();
        private static void OnInterstitialClose(IadsCloseEvent evt)
            => Callbacks.Ads.InvokeOnInterstitialADClosed();
        private static void OnRewardStartLoad(RadsLoadEvent evt)
            => Callbacks.Ads.InvokeOnRewardedADStartLoad(evt.adUnitId); 
        private static void OnRewardLoaded(RadsLoadedEvent evt)
            => Callbacks.Ads.InvokeOnRewardedADLoaded(); 
        private static void OnRewardFailed(RadsFailedEvent evt)
            => Callbacks.Ads.InvokeOnRewardADFailed();
        private static void OnRewardClose(RadsCloseEvent evt)
            => Callbacks.Ads.InvokeOnRewardADClosed();
        
        #endregion

        #region 生命周期

        /// <summary>
        /// 生命周期回调
        /// </summary>
        /// <param name="paused"></param>
        private static void OnAppPaused(bool paused)
        {
            AdService.Instance.SetAppPause(paused);
        }
        
        /// <summary>
        /// 广告初始化回调
        /// </summary>
        private static void OnAdsInitComplete()
        {
            
            UnityEngine.Debug.Log($"[SDK] get AdManager callback: OnAdsInitComplete");
            
            _initAdsCompleted = true;

            if (!InitConfig.IsBuyNoAds)
            {
                // 预制动作处理
                if (_preBannerAction == 1)
                {
                    _preBannerAction = 0;
                    ShowBanner();
                }
                else if (_preBannerAction == -1)
                {
                    _preBannerAction = 0;
                    HideBanner();
                }
            }
            Callbacks.Ads.InvokeOnAdsInitComplete();
        }

        #endregion
        
        #region 广告接口
        private static bool CheckAdsReady()
        {
            if (!IsAdsReady)
            {
                LogW("[SDK] Ads is not ready. Call <GuruSDk.StartAdService> first.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 显示Banner广告
        /// </summary>
        /// <param name="placement"></param>
        public static void ShowBanner(string placement = "")
        {
            if (!CheckAdsReady())
            {
                _preBannerAction = 1;
                return;
            }
            AdService.Instance.ShowBanner(placement);
        }
        
        /// <summary>
        /// 隐藏Banner广告
        /// </summary>
        public static void HideBanner()
        {
            if (!CheckAdsReady())
            {
                _preBannerAction = -1;
                return;
            }
            AdService.Instance.HideBanner();
        }

        public static Rect GetBannerLayout()
        {
            if (!CheckAdsReady()) return new Rect();
            return AdService.Instance.GetBannerLayout();
        }
        
        public static bool IsBannerVisible => AdService.Instance.IsBannerVisible();
        
        public static void LoadInterstitialAd()
        {
            if (!CheckAdsReady()) return;
            AdService.Instance.LoadInterstitial();
        }

        public static bool IsInterstitialAdReady => AdService.Instance.IsInterstitialReady();
        
        /// <summary>
        /// 显示插屏广告
        /// </summary>
        /// <param name="placement"></param>
        /// <param name="onDismissed"></param>
        public static void ShowInterstitialAd(string placement = "", Action onDismissed = null)
        {
            if (!CheckAdsReady()) return;
            if (!AdService.Instance.IsInterstitialReady())
            {
                LogE("Interstitial is not ready. Call <GuruSDk.ShowInterstitialAd> again.");
                LoadInterstitialAd();
                return;
            }
            AdService.Instance.ShowInterstitial(placement, onDismissed);
        }

        public static void LoadRewardAd()
        {
            if (!CheckAdsReady()) return;
            AdService.Instance.LoadRewarded();
        }
        
        public static bool IsRewardAdReady => AdService.Instance.IsRewardedReady();
        
        /// <summary>
        /// 显示激励视频广告
        /// </summary>
        /// <param name="placement"></param>
        /// <param name="onRewarded"></param>
        /// <param name="onClose"></param>
        public static void ShowRewardAd(string placement = "", Action onRewarded = null, Action onClose = null)
        {
            if (!CheckAdsReady()) return;
            if (!AdService.Instance.IsRewardedReady())
            {
                LogE("RewardAd is not ready. Call <GuruSDk.LoadRewardAd> again.");
                LoadRewardAd();
                return;
            }
            AdService.Instance.ShowRewarded(placement, onRewarded, onClose);
        }


        #endregion

        #region MaxDebugPannel

        /// <summary>
        /// 显示Max调试菜单
        /// </summary>
        public static void ShowMaxMediationDebugger()
        {
#if UNITY_EDITOR

            LogI($"Call show Max Debug Panel in Editor.");
            return;
#endif
            if (!AdService.Instance.IsReady())
            {
                LogI($"ADService is not initialized, call <GuruSDK.StartAds> first.");
                return;
            }
            AdService.Instance.ShowMaxMediationDebugger();
        }
        

        public static void ShowMaxCreativeDebugger()
        {
#if UNITY_EDITOR

            LogI($"Call show Max Debug Panel in Editor.");
            return;
#endif
            if (!AdService.Instance.IsReady())
            {
                LogI($"ADService is not initialized, call <GuruSDK.StartAds> first.");
                return;
            }
            AdService.Instance.ShowMaxCreativeDebugger();
        }


        #endregion
        
    }


    

}