

namespace Guru
{
    using System;
    using System.Collections;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UnityEngine;
    
    /// <summary>
    /// GuruConsent 流程封装
    /// </summary>
    public class GuruConsent
    {
        // Guru Consent Version
        public static string Version = "1.0.9";
        public static string Tag = "[GuruConsent]";

        #region 公用接口

        private static Action<int> onCompleteHandler = null;

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
        /// <param name="onComplete"></param>
        /// <param name="deviceId"></param>
        /// <param name="debugGeography"></param>
        /// <param name="dmaMapRule"></param>
        /// <param name="enableCountryCheck"></param>
        public static void StartConsent(Action<int> onComplete = null, 
            string deviceId = "", int debugGeography = -1,
            string dmaMapRule = "", bool enableCountryCheck = false)
        {
            Debug.Log($"{Tag} --- GuruConsent::StartConsent [{Version}] - deviceId:[{deviceId}]  debugGeography:[{debugGeography}]  dmaMapRule:[{dmaMapRule}]  enableCountryCheck:[{enableCountryCheck}]");

            _dmaMapRule = dmaMapRule;
            _enableCountryCheck = enableCountryCheck;
            onCompleteHandler = onComplete;
            // 初始化SDK对象
            GuruSDKCallback.AddCallback(OnSDKCallback);
            if (debugGeography == -1) debugGeography = DebugGeography.DEBUG_GEOGRAPHY_EEA;

            Agent?.RequestGDPR(deviceId, debugGeography);
        }

        
        
        /// <summary>
        /// 获取SDK回调
        /// </summary>
        /// <param name="msg"></param>
        private static void OnSDKCallback(string msg)
        {
            GuruSDKCallback.RemoveCallback(OnSDKCallback); // 移除回调
            
            //-------- Fetch DMA status and report -----------
            var value = Agent?.GetPurposesValue() ?? "";
            GoogleDMAHelper.SetDMAStatus(value, _dmaMapRule, _enableCountryCheck);
            
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
                            onCompleteHandler?.Invoke(status);
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
            if (onCompleteHandler != null)
            {
                onCompleteHandler.Invoke(status);
                onCompleteHandler = null;
            }
        }

        /// <summary>
        /// 上报异常
        /// </summary>
        /// <param name="ex"></param>
        public static void LogException(Exception ex)
        {
            Analytics.LogCrashlytics(ex);
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
    
}
