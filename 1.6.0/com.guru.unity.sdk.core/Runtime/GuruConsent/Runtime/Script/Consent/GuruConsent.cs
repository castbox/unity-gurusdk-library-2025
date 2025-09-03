#nullable enable
using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Guru
{
    /// <summary>
    /// GuruConsent 流程封装
    /// </summary>
    public class GuruConsent
    {
        // Guru Consent Version
        public static string Version = "1.0.9";
        public static string Tag = "[GuruConsent]";

        #region 公用接口

        private static Action<int>? _onGdprResultHandler = null;
        private static Action<ConsentData>? _onConsentResultHandler = null;

        private static IConsentAgent _agent;
        private static IConsentAgent Agent
        {
            get
            {
                if (_agent == null)
                {
#if UNITY_EDITOR
                    _agent = new ConsentAgentStub();
#elif UNITY_ANDROID
                    _agent = new ConsentAgentAndroid();
#elif UNITY_IOS
                    _agent = new ConsentAgentIOS(); 
#endif
                    _agent?.Init(GuruSDKCallback.ObjectName, GuruSDKCallback.MethodName);
                }
                return _agent;
            }
        }
        
        private static string _dmaMapRule = "";
        private static bool _enableCountryCheck = true;

        /// <summary>
        /// 对外公开接口
        /// </summary>
        /// <param name="onGdprResult"></param>
        /// <param name="onConsentResult"></param>
        /// <param name="testDeviceId"></param>
        /// <param name="debugGeography"></param>
        /// <param name="dmaMapRule"></param>
        /// <param name="enableCountryCheck"></param>
        public static void StartConsent(Action<int> onGdprResult, Action<ConsentData>? onConsentResult = null,
            string? testDeviceId = null, int debugGeography = -1,
            string dmaMapRule = "", bool enableCountryCheck = false)
        {
            Debug.Log($"{Tag} --- GuruConsent::StartConsent [{Version}] - deviceId:[{testDeviceId}]  debugGeography:[{debugGeography}]  dmaMapRule:[{dmaMapRule}]  enableCountryCheck:[{enableCountryCheck}]");

            _dmaMapRule = dmaMapRule;
            _enableCountryCheck = enableCountryCheck;
            _onGdprResultHandler = onGdprResult;
            _onConsentResultHandler = onConsentResult;
            // 初始化SDK对象
            GuruSDKCallback.AddCallback(OnSDKCallback);
            if (debugGeography == -1) debugGeography = DebugGeography.DEBUG_GEOGRAPHY_EEA;

            testDeviceId ??= string.Empty;
            Agent?.RequestGDPR(testDeviceId, debugGeography);
        }


        public static void AddAttStatusListen(Action<TrackingAuthorizationStatus> listen)
        {
#if UNITY_IOS
            listen?.Invoke(AttStatus);
            if (listen != null)
                ATTManager.Instance.OnTrackingAuthorization += listen;
#endif
        }
        
        /// <summary>
        /// 更新 Conset 状态
        /// </summary>
        public static void RefreshConsentData()
        {
            var value = Agent?.GetPurposesValue() ?? "";
            var consentData = GoogleDMAHelper.UpdateDmaStatus(value, _dmaMapRule, _enableCountryCheck);
            _onConsentResultHandler?.Invoke(consentData);
        }


        /// <summary>
        /// 获取SDK回调
        /// </summary>
        /// <param name="msg"></param>
        private static void OnSDKCallback(string msg)
        {
            GuruSDKCallback.RemoveCallback(OnSDKCallback); // 移除回调
            
            //-------- Fetch DMA status and report -----------
            // #1. 首次更新 ConsentData 在获取到 FirebaseID 后， 等待 2s 后开始更新
            // #2. GDPR 拉取结束之后会再次刷新一下 ConsentData 
            Debug.Log($"{Tag} #2. RefreshConsentData after Gdpr result: {msg}");
            RefreshConsentData(); 
            
            int status = StatusCode.UNKNOWN;
            //------- message send to unity ----------
            Debug.Log($"{Tag} get callback msg:\n{msg}");
            try
            {
                var data = JsonConvert.DeserializeObject<JObject>(msg);
                if (data != null && data.TryGetValue("action", out var jAtc))
                {
                    if (jAtc.ToString() == "gdpr" && data.TryGetValue("data", out var jData)) 
                    {
                        if (jData is JObject jObj)
                        {
                            string message = "";
                            if (jObj.TryGetValue("status", out var jStatus))
                            {
                                int.TryParse(jStatus.ToString(), out status);
                            }
                            if (jObj.TryGetValue("msg", out var jMsg))
                            {
                                message = jMsg.ToString();
                            }
                            Debug.Log($"{Tag} ---  status: {status}    msg: {message}");
                            _onGdprResultHandler?.Invoke(status);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Analytics.LogCrashlytics(ex);
            }
            
            Debug.LogError($"{Tag} Parse callback Error");
            if (_onGdprResultHandler != null)
            {
                _onGdprResultHandler.Invoke(status);
                _onGdprResultHandler = null;
            }
        }

        /// <summary>
        /// 获取 ATT 状态
        /// </summary>
        /// <returns></returns>
        public static int GetTrackingAuthorizationStatus()
        {
#if UNITY_IOS
            return ATTManager.Instance.GetTrackingAuthorizationStatus();
#endif
            return StatusCode.OBTAINED; // Android 和 Editor 均直接获得授权
        }


        public static TrackingAuthorizationStatus AttStatus =>(TrackingAuthorizationStatus) GetTrackingAuthorizationStatus();
        
        /// <summary>
        /// 上报异常
        /// </summary>
        /// <param name="ex"></param>
        public static void LogException(Exception ex)
        {
            Analytics.LogCrashlytics(ex);
        }

        public static string ToAttSummary(TrackingAuthorizationStatus attStatus)
        {
            return attStatus switch
            {
                TrackingAuthorizationStatus.NotDetermined => "notDetermined",
                TrackingAuthorizationStatus.Restricted => "restricted",
                TrackingAuthorizationStatus.Denied => "denied",
                TrackingAuthorizationStatus.Authorized => "authorized",
                _ => "unknown",
            };
        }
        #endregion
        
        #region 常量定义
        
        /// <summary>
        /// Consent 状态
        /// </summary>
        public static class StatusCode
        {
            public const int NOT_AVAILABLE = -100;
            public const int NOT_REQUIRED = 1;
            public const int OBTAINED = 3;
            public const int REQUIRED = 2;
            public const int UNKNOWN = 0;
        }

        /// <summary>
        /// DEBUG地理信息
        /// </summary>
        public static class DebugGeography
        {
            public const int DEBUG_GEOGRAPHY_DISABLED = 0;
            public const int DEBUG_GEOGRAPHY_EEA = 1;
            public const int DEBUG_GEOGRAPHY_NOT_EEA = 2;
        }

       
        
        #endregion
        
    }
    
    public enum TrackingAuthorizationStatus
    {
        NotDetermined = 0,
        Restricted,
        Denied,
        Authorized,
        Unknown
    }

    
}
