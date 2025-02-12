namespace Guru
{
    using System;
    using UnityEngine;
    
    public class AnalyticsAgentAndroid: IAnalyticsAgent
    {
        
#if UNITY_ANDROID
        
        private static readonly string AnalyticsClassName = "com.guru.unity.analytics.Analytics";
        private static AndroidJavaClass _classAnalytics;
        private static AndroidJavaClass ClassAnalytics => _classAnalytics ??= new AndroidJavaClass(AnalyticsClassName);

#endif
        private static bool _isDebug = false;
            
        #region 工具方法
        
        /// <summary>
        /// 调用静态方法
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        private static void CallStatic(string methodName, params object[] args)
        {
#if UNITY_ANDROID
            try
            {
                if (ClassAnalytics != null)
                {
                    ClassAnalytics.CallStatic(methodName, args);
                    // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                    if(_isDebug) Debug.Log($"{GuruAnalytics.LOG_TAG} Android call static :: {methodName}");
                }
            }
            catch (Exception e)
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                Debug.LogError(e.Message);
            }
#endif    
        }
        
        /// <summary>
        /// 调用静态方法
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        private static T CallStatic<T>(string methodName, params object[] args)
        { 
#if UNITY_ANDROID
            try
            {
                if (ClassAnalytics != null)
                {
                    if(_isDebug) Debug.Log($"{GuruAnalytics.LOG_TAG} Android call static <{typeof(T)}> :: {methodName}");
                    return ClassAnalytics.CallStatic<T>(methodName, args);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
#endif       
            return default(T);  
        }
        
        #endregion

        #region 接口实现

        /// <summary>
        /// 面向 Android 启动专用的 API
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="deviceInfo"></param>
        /// <param name="baseUrl"></param>
        /// <param name="uploadIpAddress"></param>
        /// <param name="guruSDKVersion"></param>
        /// <param name="onInitComplete"></param>
        /// <param name="isDebug"></param>
        public void Init(string appId, string deviceInfo, string baseUrl, string[] uploadIpAddress, string guruSDKVersion,
            Action onInitComplete = null, bool isDebug = false)
        {
            _isDebug = isDebug;
            string bundleId = Application.identifier;
            CallSDKInit(appId, deviceInfo, bundleId, _isDebug, baseUrl, uploadIpAddress , true, false, guruSDKVersion); // 调用接口   
            onInitComplete?.Invoke();
        }
        
        
        /********* Android API **********
         * Ver U3DAnalytics-1.12.0
        public static void init(String appId,
                               String deviceInfo,
                               String bundleId,
                               boolean debug,
                               String baseUrl,
                               String uploadIpAddressStr,
                               boolean useWorker,
                               boolean enabledCronet)
         */
        private void CallSDKInit(string appId, 
            string deviceInfo, 
            string bundleId, 
            bool isDebug = false,
            string baseUrl = "", 
            string[] uploadIpAddress = null, 
            bool useWorker = true, 
            bool useCronet = false,
            string guruSDKVersion = "")
        {
            string  uploadIpAddressStr= string.Join(",", uploadIpAddress ?? Array.Empty<string>());
            CallStatic("init", appId, deviceInfo, bundleId, isDebug, baseUrl, uploadIpAddressStr, useWorker, useCronet, guruSDKVersion); // 调用接口 1.12.0 参数顺序有调整  
        }
        
        public void SetScreen(string screenName)
        {
            if (string.IsNullOrEmpty(screenName)) return;
            CallStatic("setScreen", screenName);
        }
        public void SetAdId(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            CallStatic("setAdId", id);
        }

        public void SetUserProperty(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) return;
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            CallStatic("setUserProperty", key, value);
        }
        public void SetFirebaseId(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            CallStatic("setFirebaseId", id);
        }

        public void SetAdjustId(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            CallStatic("setAdjustId", id);
        }

        public void SetDeviceId(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId)) return;
            CallStatic("setDeviceId", deviceId);
        }

        public void SetUid(string uid)
        {
            if (string.IsNullOrEmpty(uid)) return;
            CallStatic("setUid", uid);
        }

        public bool IsDebug => CallStatic<bool>("isDebug");
        public void LogEvent(string eventName, string parameters, int priority = 0) => CallStatic("logEvent", eventName, parameters, priority);
        public void ReportEventSuccessRate() => CallStatic("reportEventRate");
        public void SetTch02Value(double value) => CallStatic("setTch02Value", value);
        public void InitCallback(string objName, string method) => CallStatic("initCallback", objName, method);
        
        private bool _enableErrorLog;
        public bool EnableErrorLog
        {
            get => _enableErrorLog;
            set
            {
                _enableErrorLog = value;
                CallStatic("setEnableErrorLog", _enableErrorLog);  
            }
        }
        
        public int GetEventCountTotal() => CallStatic<int>("getEventCountAll");
        public int GetEventCountUploaded() => CallStatic<int>("getEventCountUploaded");
        public string GetAuditSnapshot() => CallStatic<string>("getAuditSnapshot");
        
        #endregion
        
    }
}