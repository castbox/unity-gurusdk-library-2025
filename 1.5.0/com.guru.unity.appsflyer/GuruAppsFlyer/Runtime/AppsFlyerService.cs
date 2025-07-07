#nullable enable

using System;
using System.Collections.Generic;
using AppsFlyerSDK;
using Cysharp.Threading.Tasks;
using Firebase.Analytics;
using Guru.Ads;
using UnityEngine;

namespace Guru
{
    public class AppsFlyerConfig
    {
        internal readonly string DevKey;
        internal readonly string AppId;
        internal readonly HashSet<string> ExplicitEvents;
        internal readonly bool DebugMode;
        internal readonly bool AppsflyerForceDataUsage;
        
        public AppsFlyerConfig(string devKey, string appId,  
            HashSet<string> explicitEvents, 
            bool? appsflyerForceDataUsage = null, 
            bool debugMode = false)
        {
            DevKey = devKey;
            AppId = appId;
            ExplicitEvents = explicitEvents;
            DebugMode = debugMode;
            AppsflyerForceDataUsage = appsflyerForceDataUsage ?? true;
        }
    }
    
    
    /// <summary>
    /// AppsFlyer 归因服务
    /// </summary>
    public class AppsFlyerService
    {
        private IGuruSDKApiProxy _guruSDK;
        private readonly AppsFlyerConfig _config;
        private static volatile AppsflyerConversionData? _appsflyerConversionData;

        public static AppsflyerConversionData? ConversionData
        {
            get => _appsflyerConversionData;
            private set => _appsflyerConversionData = value;
        }


        public AppsFlyerService(AppsFlyerConfig afConfig, IGuruSDKApiProxy proxy)
        {
            _config = afConfig;
            _guruSDK = proxy;
        }

        public void Prepare()
        {
            if (_config == null)
            {
                throw new NullReferenceException("AppsFlyerConfig is null, Can not create AppsFlyerEventDriver!!");
            }
            
            var userProperties = new Dictionary<string, string>()
            {
                { "device_id", _guruSDK.DeviceId },
                { "user_pseudo_id", _guruSDK.FirebaseId }
            };
            try
            {
                AppsFlyer.initSDK(_config.DevKey, _config.AppId,  GuruAppsflyerManagerV1.Instance);
                AppsFlyer.setAdditionalData(userProperties);
                AppsFlyer.setCustomerUserId(_guruSDK.DeviceId);
                AppsFlyer.setIsDebug(_config.DebugMode);
            }
            catch (Exception exception)
            {
                Log.W($"Failed to get AppsFlyerId {exception}");
            }
        }


        public async UniTask InitializeAsync()
        {
            AppsFlyer.startSDK();
            var appsFlyerId = await RefreshAppsflyerId();
            Log.I($"AppsFlyer Initialized: {appsFlyerId.ToSecretString(maxVisible: 3)}");
            CheckAndListenAppsflyerConversionData();
            // 在外部触发 Flush 所有的缓存事件
        }
        
        #region 事件打点

        
        public void ReportEvent(string eventName, Dictionary<string, object>? evtParams = null)
        {
            // 上报事件
            if (IsValidEvent(eventName))
            {
                // 上报事件
                AppsFlyer.sendEvent(eventName, DictionaryUtil.ToStringDictionary(evtParams));
            }
        }
        
        
        
        public void ReportAdRevenue(AdImpressionEvent adEvent)
        {
            var additionalParams = new Dictionary<string, string>()
            {
                { AdRevenueScheme.COUNTRY, adEvent.countryCode }, // MaxSdk.GetSdkConfiguration().CountryCode 从外部映射获得
                { AdRevenueScheme.AD_UNIT, adEvent.adUnitId },
                { AdRevenueScheme.AD_TYPE, adEvent.adFormat },
                { AdRevenueScheme.PLACEMENT, adEvent.adPlacement },
            };
            
            AppsFlyer.logAdRevenue(
                new AFAdRevenueData(
                    adEvent.adSource,  //adInfo.NetworkName
                    MediationNetwork.ApplovinMax,
                    adEvent.currency,
                    adEvent.value), additionalParams);
        }


        /// <summary>
        /// 设置 Consent
        /// </summary>
        /// <param name="consentData"></param>
        public void SetConsent(ConsentData consentData)
        {
            try
            {
                bool? isUserSubjectToGdpr;
                bool? hasConsentForDataUsage;
                bool? hasConsentForAdsPersonalization;
                
                if (consentData.IsEea)
                {
                    isUserSubjectToGdpr = true;
                    hasConsentForDataUsage = _config.AppsflyerForceDataUsage;
                    hasConsentForAdsPersonalization = 
                        consentData.Consents.TryGetValue(ConsentType.AdPersonalization, out var adPersonalization)
                            ? adPersonalization == ConsentStatus.Granted
                            : null;
                }
                else
                {
                    isUserSubjectToGdpr = false;
                    hasConsentForDataUsage = true;
                    hasConsentForAdsPersonalization = true;
                }
                
                var appsFlyerConsent = new AppsFlyerConsent(isUserSubjectToGdpr, hasConsentForDataUsage,
                    hasConsentForAdsPersonalization);
                AppsFlyer.setConsentData(appsFlyerConsent);
                
                Log.I($"SetAppsflyer complete! GDPR:{isUserSubjectToGdpr} hasConsentForDataUsage:{hasConsentForDataUsage} hasConsentForAdsPersonalization:{hasConsentForAdsPersonalization}");
            }
            catch (Exception e)
            {
                Log.W($"Failed to set consent: {e.Message}");
            }
            
        }
        

        #endregion


        #region 归因转化

        private int _retryFetchAppsflyerIdCount = 0;
        
        
        private async UniTask<string> RefreshAppsflyerId()
        {
            try
            {
                await UniTask.SwitchToMainThread();
                var appsFlyerId = AppsFlyer.getAppsFlyerId();
#if UNITY_EDITOR
                // Editor 中会一直返回空字符串
                appsFlyerId = $"af_fake_id_{_config.DevKey}";
#endif
                if (string.IsNullOrEmpty(appsFlyerId))
                {
                    // GuruAnalytics.Instance.ApplyAppsFlyerId("unknown");
                    ApplyAppsFlyerId("unknown");
                    // retryFetchAppsflyerIdCount++;
                    // retryFetchAppsflyerIdTimer?.Dispose();
                    // retryFetchAppsflyerIdTimer = null;
                    //
                    // Log.W("Failed to get AppsFlyerId, retrying...");
                    // retryFetchAppsflyerIdTimer = UniTimer.Delayed(TimeSpan.FromSeconds(MathUtils.Fibonacci(retryCount).Clamp(3, 600)),
                    //     () => _ = RefreshAppsflyerId());
                    // retryFetchAppsflyerIdTimer.Start();

                    return await RetryFetchAppsflyerId();
                }
                else
                {
                    // GuruAnalytics.Instance.ApplyAppsFlyerId(appsFlyerId);
                    ApplyAppsFlyerId(appsFlyerId);
                    Log.I($"AppsFlyer Id: {appsFlyerId.ToSecretString()}");
                    _retryFetchAppsflyerIdCount = 0;
                }
                return appsFlyerId;
            }
            catch (Exception exception)
            {
                Log.W($"Failed to refresh AppsFlyerId: {exception}");
            }
            return "unknown";
        }


        private int GetRetryTimeNumber(int retryCount)
        {
            return Mathf.Clamp((int)Mathf.Pow(2, retryCount), 3, 600);
        }


        private async UniTask<string> RetryFetchAppsflyerId()
        {
            _retryFetchAppsflyerIdCount++;
            var delayTime = GetRetryTimeNumber(_retryFetchAppsflyerIdCount);
            await UniTask.Delay(TimeSpan.FromSeconds(delayTime));
            return await RefreshAppsflyerId();
        }


        private void ApplyAppsFlyerId(string appsFlyerId)
        {
            _guruSDK.ReportAppsFlyerId(appsFlyerId);
        }


        private void CheckAndListenAppsflyerConversionData()
        {
            var afMgr = GuruAppsflyerManagerV1.Instance;
            if (afMgr == null)
            {
                Log.W("AppsFlyer Manager is not initialized, cannot check conversion data.");
                return;
            }

            try
            {
                // var conversionData = await AppProperty.Instance.GetAppsflyerConversionData();
                var conversionData = GetAppsflyerConversionData();
                if (conversionData == null)
                {
                    Log.I("First Fetch Appsflyer Conversion Data");
                    // _appsflyerConversionDataSubscription?.Dispose();
                    // _appsflyerConversionDataSubscription =
                    //     afMgr.ObservableConversionData.Subscribe(ProcessAppsflyerConversionDataChanged);
                    
                    afMgr.OnGetConversionData += ProcessAppsflyerConversionDataChanged;
                }
                else
                {
                    Log.I($"Appsflyer Conversion Data already exists: {conversionData.RawConversionData}");
                    ConversionData = conversionData;
                    ProcessAppsflyerConversionDataChanged(conversionData);
                }
            }
            catch (Exception ex)
            {
                Log.E($"Unexpected error in CheckAndFetchAppsflyerConversionData: {ex.Message}");
            }
        }
        
        
        private void ProcessAppsflyerConversionDataChanged(AppsflyerConversionData changedConversionData)
        {
            Log.I($"[Appsflyer]: ConversionData changed: {changedConversionData.RawConversionData}");

            UniTask.SwitchToMainThread();

            var isOrganicInstall = changedConversionData.IsOrganicInstall();
            var mediaSource = changedConversionData.GetMediaSource();
            _guruSDK.ReportUserProperty("af_media_source",
                string.IsNullOrEmpty(mediaSource) ? (isOrganicInstall ? "organic" : "unknown") : mediaSource!);

            var campaign = changedConversionData.GetCampaign();
            _guruSDK.ReportUserProperty("af_campaign",
                string.IsNullOrEmpty(campaign) ? "unknown" : campaign!);


            var currentConversionData = ConversionData;
            if (currentConversionData == null ||
                !Equals(currentConversionData, changedConversionData))
            {
                // AppProperty.Instance.SetAppsflyerConversionData(changedConversionData);
                SetAppsflyerConversionData(changedConversionData);

                ConversionData = changedConversionData;
                Log.I("Appsflyer Conversion Data changed!!!");
            }

            changedConversionData.Dump();
        }

        private static void SetAppsflyerConversionData(AppsflyerConversionData conversionData)
        {
            PlayerPrefs.SetString(nameof(AppsflyerConversionData), conversionData.RawConversionData);
        }


        private static AppsflyerConversionData? GetAppsflyerConversionData()
        {
            var rawConversionData = PlayerPrefs.GetString(nameof(AppsflyerConversionData), string.Empty);
            return string.IsNullOrEmpty(rawConversionData) ? null : new AppsflyerConversionData(rawConversionData);
        }

        #endregion
        


        #region Utils
        
        private bool IsValidEvent(string name)
        {
            // 实现按照
            return _config.ExplicitEvents.Count != 0 && _config.ExplicitEvents.Contains(name);
            //|| config.Strategy.PriorityMatchers.Any(matcher => matcher.Match(name));
        }



        #endregion

    }
}