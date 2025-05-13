
namespace Guru
{
    using System;
    using System.Runtime.InteropServices;
    
    public class AnalyticsAgentIOS: IAnalyticsAgent
    {

        #region 属性定义

        private const string K_INTERNAL = "__Internal";
        
#if UNITY_IOS
        // ------------- U3DAnalytics.mm Interface -----------------
        // object-c: void unityInitAnalytics(const char *appId, const char *deviceInfo, bool isDebug, const char *baseUrl, const char *uploadIpAddressStr)
        [DllImport(K_INTERNAL)] private static extern void unityInitAnalytics(string appId, string deviceInfo, bool isDebug, string baseUrl, string uploadIpAddressStr, string guruSDKVersion);
        [DllImport(K_INTERNAL)] private static extern void unitySetUserID(string uid);
        [DllImport(K_INTERNAL)] private static extern void unitySetScreen(string screenName);
        [DllImport(K_INTERNAL)] private static extern void unitySetAdId(string adId);
        [DllImport(K_INTERNAL)] private static extern void unitySetAdjustID(string adjustId);
        [DllImport(K_INTERNAL)] private static extern void unitySetFirebaseId(string fid);
        [DllImport(K_INTERNAL)] private static extern void unitySetDeviceId(string did);
        [DllImport(K_INTERNAL)] private static extern void unitySetUserProperty(string key, string value);
        [DllImport(K_INTERNAL)] private static extern void unityLogEvent(string key, string data);
        [DllImport(K_INTERNAL)] private static extern void unityReportEventRate();
        [DllImport(K_INTERNAL)] private static extern void unityInitException();
        [DllImport(K_INTERNAL)] private static extern void unityTestUnrecognizedSelectorCrash();
        [DllImport(K_INTERNAL)] private static extern void unitySetTch02Value(double value);
        [DllImport(K_INTERNAL)] private static extern void unitySetEnableErrorLog(bool value);
        [DllImport(K_INTERNAL)] private static extern void unityInitCallback(string objName, string method);
        [DllImport(K_INTERNAL)] private static extern int unityGetEventsCountAll();
        [DllImport(K_INTERNAL)] private static extern int unityGetEventsCountUploaded();
#endif
        
        private static bool _isDebug = false;    
            
        #endregion

        #region 接口实现

        private bool _enableErrorLog;
        public bool EnableErrorLog
        {
            get => _enableErrorLog;
            set
            {
                _enableErrorLog = value;
#if UNITY_IOS
                unitySetEnableErrorLog(_enableErrorLog);
#endif
            }
        }
        
        public void InitCallback(string objName, string method)
        {
#if UNITY_IOS
            unityInitCallback(objName, method);
#endif
        }


        /// <summary>
        /// 初始化 SDK
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="deviceInfo"></param>
        /// <param name="baseUrl"></param>
        /// <param name="uploadIpAddress"></param>
        /// <param name="guruSDKVersion"></param>
        /// <param name="onInitComplete"></param>
        /// <param name="isDebug"></param>
        public void Init(string appId, string deviceInfo, string baseUrl, string[] uploadIpAddress,  string guruSDKVersion,
            Action onInitComplete, bool isDebug = false)
        {
            _isDebug = isDebug;
            
#if UNITY_IOS
            string uploadIpAddressStr = string.Join(",", uploadIpAddress ?? Array.Empty<string>());
            unityInitAnalytics(appId, deviceInfo, isDebug, baseUrl, uploadIpAddressStr, guruSDKVersion);    
            unityInitException(); // 初始化报错守护进程
#endif
            onInitComplete?.Invoke();
        }

        public void SetScreen(string screenName)
        {
            if (string.IsNullOrEmpty(screenName)) return;
#if UNITY_IOS
            unitySetScreen(screenName);
#endif
        }

        public void SetAdId(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
#if UNITY_IOS
            unitySetAdId(id);
#endif
        }

        public void SetUserProperty(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) return;
#if UNITY_IOS
            unitySetUserProperty(key, value);
#endif
        }

        public void SetFirebaseId(string fid)
        {
            if (string.IsNullOrEmpty(fid)) return;
#if UNITY_IOS
            unitySetFirebaseId(fid);
#endif
        }

        public void SetAdjustId(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
#if UNITY_IOS
            unitySetAdjustID(id);
#endif
        }

        public void SetDeviceId(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId)) return;
#if UNITY_IOS
            unitySetDeviceId(deviceId);
#endif
        }

        public void SetUid(string uid)
        {
            if (string.IsNullOrEmpty(uid)) return;
#if UNITY_IOS
            unitySetUserID(uid);
#endif
        }

        public bool IsDebug => _isDebug;

        public void LogEvent(string eventName, string data, int priority = 0)
        {
#if UNITY_IOS
            unityLogEvent(eventName, data);
#endif
        }
        
        public void ReportEventSuccessRate()
        {
#if UNITY_IOS
            unityReportEventRate();
#endif
        }
        
        public void SetTch02Value(double value)
        {
#if UNITY_IOS
            unitySetTch02Value(value);
#endif
        }
        
#if UNITY_IOS
        // iOS 测试用事件
        public static void TestCrashEvent()=> unityTestUnrecognizedSelectorCrash();
#endif

        public int GetEventCountTotal()
        {
#if UNITY_IOS
            return unityGetEventsCountAll();
#endif
            return 1;
        }

        public int GetEventCountUploaded()
        {
#if UNITY_IOS
            return unityGetEventsCountUploaded();
#endif
            return 0;
        }


        public string GetAuditSnapshot()
        {
            // TODO：iOS 原生类并未实现此接口
            return "";
        }

        #endregion
        
    }
}