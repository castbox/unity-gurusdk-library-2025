#if UNITY_IOS

namespace Guru
{
    using System;
    using System.Runtime.InteropServices;
    
    public class AnalyticsAgentIOS: IAnalyticsAgent
    {

        #region 属性定义

        private const string K_INTERNAL = "__Internal";
        

        // ------------- U3DAnalytics.mm Interface -----------------
        // object-c: void unityInitAnalytics(const char *appId, const char *deviceInfo, bool isDebug, const char *baseUrl, const char *uploadIpAddressStr)
        [DllImport(K_INTERNAL)] private static extern void unityInitAnalytics(string appId, string deviceInfo, bool isDebug, string baseUrl, string uploadIpAddressStr, string guruSDKVersion);
        [DllImport(K_INTERNAL)] private static extern void unitySetUserID(string uid);
        [DllImport(K_INTERNAL)] private static extern void unitySetScreen(string screenName);
        [DllImport(K_INTERNAL)] private static extern void unitySetAdId(string adId);
        [DllImport(K_INTERNAL)] private static extern void unitySetAdjustID(string adjustId);
        // [DllImport(K_INTERNAL)] private static extern void unitySetAppsflyerId(string appsflyerId); //TODO: 需要升级自打点库实现
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
                unitySetEnableErrorLog(_enableErrorLog);
            }
        }
        
        public void InitCallback(string objName, string method)
        {
            unityInitCallback(objName, method);
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
            
            string uploadIpAddressStr = string.Join(",", uploadIpAddress ?? Array.Empty<string>());
            unityInitAnalytics(appId, deviceInfo, isDebug, baseUrl, uploadIpAddressStr, guruSDKVersion);    
            unityInitException(); // 初始化报错守护进程

            onInitComplete?.Invoke();
        }

        public void SetScreen(string screenName)
        {
            if (string.IsNullOrEmpty(screenName)) return;
            unitySetScreen(screenName);
        }

        public void SetAdId(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            unitySetAdId(id);
        }

        public void SetUserProperty(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) return;
            unitySetUserProperty(key, value);
        }

        public void SetFirebaseId(string fid)
        {
            if (string.IsNullOrEmpty(fid)) return;
            unitySetFirebaseId(fid);
        }

        public void SetAdjustId(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            unitySetAdjustID(id);
        }
        
        public void SetAppsflyerId(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            // unitySetAppsflyerId(id);
            // TODO: 需要在后面升级实现
        }

        public void SetDeviceId(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId)) return;
            unitySetDeviceId(deviceId);
        }

        public void SetUid(string uid)
        {
            if (string.IsNullOrEmpty(uid)) return;
            unitySetUserID(uid);
        }

        public bool IsDebug => _isDebug;

        public void LogEvent(string eventName, string data, int priority = 0)
        {
            unityLogEvent(eventName, data);
        }
        
        public void ReportEventSuccessRate()
        {
            unityReportEventRate();
        }
        
        public void SetTch02Value(double value)
        {
            unitySetTch02Value(value);
        }
        

        // iOS 测试用事件
        public static void TestCrashEvent()=> unityTestUnrecognizedSelectorCrash();


        public int GetEventCountTotal()
        {
            return unityGetEventsCountAll();
        }

        public int GetEventCountUploaded()
        {
            return unityGetEventsCountUploaded();
        }


        public string GetAuditSnapshot()
        {
            // TODO：iOS 原生类并未实现此接口
            return "";
        }

        #endregion
        
    }
}

#endif