using Cysharp.Threading.Tasks;

namespace Guru.Notification
{
    using UnityEngine;
    using System;
    
    /// <summary>
    /// 消息管理器
    /// </summary>
    public class NotificationService
    {
        // 服务版本号
        public const string Version = "0.0.1";


        // 初始化标志位
        private static bool _initOnce;

        private static string DefaultPermissionStatus
        {
            get
            {
#if UNITY_IOS
                return "not_determined";
#endif
                return "denied";
            }
        }

        #region 初始化

        private static INotificationAgent _agent;

        internal static INotificationAgent Agent
        {
            get
            {
                if (_agent == null)
                {
                    _agent = GetAgent();
                }
                return _agent;
            }
        }

        private static INotificationAgent GetAgent()
        {
#if UNITY_EDITOR
            return new NotificationAgentStub();
#elif UNITY_ANDROID
            return new NotificationAgentAndroid();
#elif UNITY_IOS
            return new NotificationAgentIOS();
#endif
            return null;
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initialize()
        {
            if (_initOnce) return;
            _initOnce = true;
            Agent?.Init(); // 初始化代理
        }
        
        
        
        #endregion
        
        #region 接口
        
#if UNITY_IOS
        public static async UniTask<string> GetIOSDeviceToken()
        {
            try
            {
                return await ((NotificationAgentIOS)Agent).GetDeviceToken();
            }
            catch (Exception e)
            {
                Log.W($"[SDK][Noti] GetIOSDeviceToken failed: {e}");
                return null;
            }
        }
        
#endif
        
        /// <summary>
        /// 拉起 Noti 请求
        /// </summary>
        /// <param name="callback"></param>
        public static void RequestPermission(Action<string> callback = null)
        {
            if (Agent != null)
            {
                Agent.RequestPermission(callback);
                return;
            }
            Debug.LogError($"[SDK][Noti] --- Agent is missing, return default status: {DefaultPermissionStatus}");
            callback?.Invoke(DefaultPermissionStatus);
        }
        
        /// <summary>
        /// 创建推送渠道
        /// </summary>
        public static void CreatePushChannels()
        {
            Agent?.CreatePushChannels();
        }

        public static bool IsPermissionGranted()
        {
            return Agent?.IsAllowed() ?? false;
        }

        public static string GetStatus()
        {
            if(!_initOnce) Initialize();
            
            if(Agent != null) return Agent.GetStatus();
            
            Debug.LogError($"[SDK][Noti] --- Agent is missing, return default status: {DefaultPermissionStatus}");
            return DefaultPermissionStatus;
        }

        #endregion
        


    }
}