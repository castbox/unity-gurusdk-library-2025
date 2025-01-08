namespace Guru
{
    using System.Collections.Generic;
    using UnityEngine;
    using Firebase.Crashlytics;
    using System;
    using System.Linq;
    
    public static class CrashlyticsAgent
    {
        private static bool _initOnce;
        private static bool IsFirebaseReady => FirebaseUtil.IsFirebaseInitialized;
        
        /// <summary>
        /// 捕获列表
        /// </summary>
        private static readonly List<LogType> _catchFilter = new List<LogType>()
        {
            LogType.Exception,
        };
        
        /// <summary>
        /// 上报列表
        /// </summary>
        private static readonly List<LogType> _logFilter = new List<LogType>()
        {
            LogType.Exception,
            LogType.Error,
            LogType.Assert,
        };

        public static void Init()
        {
            if (_initOnce) return;
            _initOnce = true;
            
            // 接受主线程的 Message
            Application.logMessageReceived -= OnReceivedMessage;
            Application.logMessageReceived += OnReceivedMessage;
            
            // 接受其他线程的 Message
            // Application.logMessageReceivedThreaded -= OnReceivedMessage;
            // Application.logMessageReceivedThreaded += OnReceivedMessage;
            
            Crashlytics.IsCrashlyticsCollectionEnabled = true;
        }
        
        private static string ToLogTypeString(LogType type)
        {
            switch (type)
            {
                case LogType.Exception: return "ex";
                case LogType.Error: return "e";
                case LogType.Warning: return "w";
            }
            return "i";
        }


        private static void OnReceivedMessage(string condition, string stackTrace, LogType type)
        {
            
            string msg = $"{DateTime.Now:yy-MM-dd HH:mm:ss} [{ToLogTypeString(type)}] {condition}\n{stackTrace}";
            
            if (_catchFilter.Contains(type))
            {
                LogException(msg);
            }
            else if(_logFilter.Contains(type))
            {
                Log(msg);
            }
        }

        public static void LogException(Exception ex)
        {
#if UNITY_EDITOR
            Debug.LogException(ex);
            return;
#endif
            if (!IsFirebaseReady) return;
            Crashlytics.LogException(ex);
        }

        public static void LogException(string msg)
        {
            LogException(new Exception(msg));
        }

        public static void Log(string msg)
        {
            if (!IsFirebaseReady) return;
            Crashlytics.Log(msg);
        }
        
        
        public static void SetCustomKey(string key, string value)
        {
            if (!IsFirebaseReady) return;
            Crashlytics.SetCustomKey(key, value);
        }
        
        public static void SetUserId(string uid)
        {
            if (!IsFirebaseReady) return;
            Crashlytics.SetUserId(uid);
        }
    }
}