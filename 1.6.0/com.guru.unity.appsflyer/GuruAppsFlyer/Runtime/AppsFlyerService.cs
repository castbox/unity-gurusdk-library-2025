#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using AppsFlyerSDK;
using Cysharp.Threading.Tasks;
using Firebase.Analytics;
using Firebase.Messaging;
using Guru.Ads;
using UnityEngine;

namespace Guru
{
    enum AppsFlyerSdkState
    {
        Idle,
        Prepared,
        Initializing,
        Initialized
    }
    
    public class AppsflyerEventRule
    {
        public delegate Dictionary<string, object?> EventParamsConvertor(Dictionary<string, object>? parameters);
        
        public readonly EventParamsConvertor? Convertor;
        
        // 后面可以扩展更多规则，比如是否需要转换参数，是否需要特殊处理等

        internal AppsflyerEventRule(EventParamsConvertor? convertor)
        {
            Convertor = convertor;
        }

        private static Dictionary<string, object?> ValueToAfRevenue(Dictionary<string, object?>? parameters)
        {
            var result = new Dictionary<string, object?>(parameters ?? new Dictionary<string, object?>());
            if (result.Remove("value", out var revenue))
            {
                result["af_revenue"] = revenue;
            }
            else if (result.Remove("revenue", out revenue))
            {
                result["af_revenue"] = revenue;
            }

            return result;
        }

        public static readonly AppsflyerEventRule Default = new AppsflyerEventRule(null);

        public static readonly AppsflyerEventRule Revenue = new AppsflyerEventRule(ValueToAfRevenue);
    }

    public class AppsflyerEventDefinition
    {
        public readonly string EventName;

        public readonly AppsflyerEventRule Rule;

        private AppsflyerEventDefinition(string eventName, AppsflyerEventRule rule)
        {
            EventName = eventName;
            Rule = rule;
        }

        public static AppsflyerEventDefinition Define(string eventName, AppsflyerEventRule? rule = null)
        {
            return new AppsflyerEventDefinition(eventName, rule ?? AppsflyerEventRule.Default);
        }

    }
    
    public class AppsFlyerConfig
    {
        internal readonly string DevKey;
        internal readonly string AppId;
        internal Dictionary<string, AppsflyerEventRule> ExplicitEvents { get; set; }
        internal readonly bool DebugMode;
        internal readonly bool AppsflyerForceDataUsage;
        
        [Obsolete(
            "Use Create method with AppsflyerEventDefinition instead! The middleware will remove this constructor in version 2.6.0+")]
        public AppsFlyerConfig(string devKey, string appId,  
            HashSet<string> explicitEvents, 
            bool? appsflyerForceDataUsage = null, 
            bool debugMode = false):this(devKey, appId, explicitEvents.ToDictionary(evt => evt, evt => AppsflyerEventRule.Default), appsflyerForceDataUsage, debugMode)
        {
            
            
        }
        
        private AppsFlyerConfig(string devKey, string appId, Dictionary<string, AppsflyerEventRule> explicitEvents, bool? appsflyerForceDataUsage = null, 
            bool debugMode = false)
        {
            DevKey = devKey;
            AppId = appId;
            ExplicitEvents = explicitEvents;
            DebugMode = debugMode;
            AppsflyerForceDataUsage = appsflyerForceDataUsage ?? true;
        }

        public static AppsFlyerConfig Create(string devKey, string appId,List<AppsflyerEventDefinition> eventDefinitions,
            bool? appsflyerForceDataUsage = null, 
            bool debugMode = false)
        {
            return new AppsFlyerConfig(devKey, appId,eventDefinitions.ToDictionary(ed => ed.EventName, ed => ed.Rule), appsflyerForceDataUsage, debugMode);
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

        private int retryCount = 0;

        private UniTimer? retryTimer;
        
#if UNITY_IOS && !UNITY_EDITOR
        private const int WaitForAttUserAuthorizationInSeconds = 40;
        private const int AdditionalFaultToleranceIntervalInSeconds = 10;
#endif

        private AppsFlyerSdkState _sdkState = AppsFlyerSdkState.Idle;
        private readonly List<Action> _pendingAppsFlyersEventActions = new();
        
        
        public static AppsflyerConversionData? ConversionData
        {
            get => _appsflyerConversionData;
            private set => _appsflyerConversionData = value;
        }
        
        private static event Action<AppsflyerConversionData?> OnConversionData;

        public AppsFlyerService(AppsFlyerConfig afConfig, IGuruSDKApiProxy proxy)
        {
            _config = afConfig;
            _guruSDK = proxy;
        }

        public void Prepare()
        {
            if (_sdkState != AppsFlyerSdkState.Idle)
            {
                Log.W($"AppsFlyer already prepared or initializing. Current state: {_sdkState}");
                return;
            }
            
            if (_config == null)
            {
                throw new NullReferenceException("AppsFlyerConfig is null, Can not create AppsFlyerEventDriver!!");
            }
            _sdkState = AppsFlyerSdkState.Prepared;
            GuruAppsflyerManagerV1.InitAppsFlyerMonoBehaviour();
            
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
                
#if UNITY_IOS && !UNITY_EDITOR
                AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(WaitForAttUserAuthorizationInSeconds);
#endif
            }
            catch (Exception exception)
            {
                Log.W($"[Appsflyer] : Failed to get AppsFlyerId {exception}");
            }

            if (GuruAppsflyerManagerV1.Instance != null)
                OnConversionData += GuruAppsflyerManagerV1.InvokeOnGetConversionData;
        }

        public async UniTask InitializeAsync()
        {
            
            if (_sdkState == AppsFlyerSdkState.Initialized)
            {
                Log.I("[Appsflyer] : AppsFlyer already initialized");
                return;
            }

            if (_sdkState != AppsFlyerSdkState.Prepared)
            {
                Log.W($"[Appsflyer] : AppsFlyer not in Initializing state. Current state: {_sdkState}");
                return;
            }

            _sdkState = AppsFlyerSdkState.Initializing;
            
            
            
#if UNITY_IOS && !UNITY_EDITOR
            var status = GuruConsent.AttStatus;
            Log.I($"[Appsflyer] : AppsFlyer Initialize called! {status}");
            if (status is TrackingAuthorizationStatus.Unknown or TrackingAuthorizationStatus.NotDetermined)
            {
                ListenAttStatusChanged();
                Log.W("[Appsflyer] : AppsFlyer not initialized, ATT status is unknown or not determined");
                UniTimer.Delayed(
                    TimeSpan.FromSeconds(WaitForAttUserAuthorizationInSeconds +
                                         AdditionalFaultToleranceIntervalInSeconds),
                    () => { _ = InitializeInternal(); }).Start();
                return;
            }
#endif
            
            await InitializeInternal();
            // AppsFlyer.startSDK();
            // var appsFlyerId = await RefreshAppsflyerId();
            // Log.I($"AppsFlyer Initialized: {appsFlyerId.ToSecretString(maxVisible: 3)}");
            // CheckAndListenAppsflyerConversionData();
            // 在外部触发 Flush 所有的缓存事件
        }
        
#if UNITY_IOS && !UNITY_EDITOR
        private void ListenAttStatusChanged()
        {
            GuruConsent.AddAttStatusListen(ProcessAttStatusChanged);
        }
#endif
        
        private void ProcessAttStatusChanged(TrackingAuthorizationStatus status)
        {
            Log.I($"[Appsflyer] : AppsFlyer ATT Status Changed: {status}");
            if (status != TrackingAuthorizationStatus.Unknown && status != TrackingAuthorizationStatus.NotDetermined)
            {
                _ = InitializeInternal();
            }
   
        }

        private async UniTask InitializeInternal()
        {
            if (_sdkState == AppsFlyerSdkState.Initialized)
            {
                Log.I("[Appsflyer] : AppsFlyer already initialized");
                return;
            }

            if (_sdkState != AppsFlyerSdkState.Initializing)
            {
                Log.W($"[Appsflyer] : AppsFlyer not in Initializing state. Current state: {_sdkState}");
                return;
            }

            try
            {
                AppsFlyer.startSDK();
            }
            catch (Exception exception)
            {
                retryCount++;
                retryTimer?.Dispose();
                retryTimer = null;

                Log.W($"[Appsflyer][{retryCount}] Failed to initialize AppsFlyer; SDK: {exception}");
                retryTimer = UniTimer.Delayed(TimeSpan.FromSeconds(MathUtils.Fibonacci(retryCount).Clamp(2, 600)),
                    () => _ = InitializeInternal());
                retryTimer.Start();
                return;
            }
            
            _sdkState = AppsFlyerSdkState.Initialized;
            var appsFlyerId = await RefreshAppsflyerId();
            TryDispatchPendingAppsFlyerEventActions();
            CheckAndListenAppsflyerConversionData();
            Log.I($"[Appsflyer] : AppsFlyer Initialized: {appsFlyerId.ToSecretString(maxVisible: 3)}");
        }
        
        #region 事件打点

        private AppsflyerEventRule.EventParamsConvertor? GetEventParamsConvertor(string eventName)
        {
            try
            {
                return _config.ExplicitEvents.TryGetValue(eventName, out var rule) ? rule.Convertor : null;
            }
            catch (Exception exception)
            {
                Log.W("[Appsflyer] : GetEventParamsConvertor failed {exception}");
            }

            return null;
        }
        
        
        public void ReportEvent(string eventName, Dictionary<string, object>? evtParams = null)
        {
            // 上报事件
            if (IsValidEvent(eventName))
            {
                var converter = GetEventParamsConvertor(eventName);
                var afParameters = converter != null
                    ? DictionaryUtil.ToStringDictionary(converter(evtParams))
                    : DictionaryUtil.ToStringDictionary(evtParams);
                
                // 上报事件
                AppsFlyer.sendEvent(eventName, afParameters);
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
            
            OnConversionData(ConversionData);
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
        
        
        private async UniTaskVoid ProcessFcmTokenReceive(object sender, TokenReceivedEventArgs token)
        {
#if UNITY_ANDROID
            try
            {
                Log.I($"[Appsflyer] : FCM Token Received: {token.Token.ToSecretString()}");
                await UniTask.SwitchToMainThread();
                if (token.Token is { Length: > 0 })
                {
                    AppsFlyer.updateServerUninstallToken(token.Token);
                }

            }
            catch (Exception ex)
            {
                Log.W($"[Appsflyer] : Failed to process FCM token: {ex}");
            }
#endif
        }
        
        public void OnFCMTokenReceived(object sender, TokenReceivedEventArgs token)
        {
            SafeDispatch(() => { _ = ProcessFcmTokenReceive(sender, token); });
        }
        
#if UNITY_IOS
        private byte[] ConvertHexStringToByteArray(string hexString)
        {

            var data = new byte[hexString.Length / 2];
            for (var index = 0; index < data.Length; index++)
            {
                var byteValue = hexString.Substring(index * 2, 2);
                data[index] = System.Convert.ToByte(byteValue, 16);
            }
            return data;
        }
        
        
        
        public void OnIOSDeviceTokenReceived(string token)
        {
            SafeDispatch(() =>
            {
                try
                {
                    var tokenBytes = ConvertHexStringToByteArray(token);
                    Log.I($"[Appsflyer] : tokenBytes: {tokenBytes}");
                    AppsFlyer.registerUninstall(tokenBytes);
                }
                catch (Exception ex)
                {
                    Log.W($"[Appsflyer] : Failed to process iOS device token: {ex}");
                }
            });
        }
#endif

        #region Utils
        
        private bool IsValidEvent(string name)
        {
            // 实现按照
            return _config.ExplicitEvents.Count != 0 && _config.ExplicitEvents.ContainsKey(name);
            //|| config.Strategy.PriorityMatchers.Any(matcher => matcher.Match(name));
        }

        private void TryDispatchPendingAppsFlyerEventActions()
        {
            if (_pendingAppsFlyersEventActions.Count <= 0) return;
            var eventActions = new List<Action>(_pendingAppsFlyersEventActions);
            _pendingAppsFlyersEventActions.Clear();
            foreach (var action in eventActions)
            {
                action.Invoke();
            }
        }


        private void SafeDispatch(Action action)
        {
            if (_sdkState == AppsFlyerSdkState.Initialized)
            {
                action();
            }
            else
            {
                _pendingAppsFlyersEventActions.Add(action);
            }
        }


        #endregion

    }
}