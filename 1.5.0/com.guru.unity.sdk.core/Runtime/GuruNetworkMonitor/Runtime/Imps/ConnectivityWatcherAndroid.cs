

namespace Guru.Network
{
#if UNITY_ANDROID
    using System;
    using UnityEngine;
    
    /// <summary>
    /// 连接监听器 Android
    /// </summary>
    public class ConnectivityWatcherAndroid: IConnectivityWatcher
    {
        public const string LOG_TAG = "[NET][A]";
        private const string CONNECTIVITY_CLASS_NAME = "com.guru.unity.monitor.Connectivity";
        private const string CONNECTIVITY_EVENT_CLASS_NAME = "com.guru.unity.monitor.ConnectivityEvent";
        private const string UNITY_PLAYER_CLASS_NAME = "com.unity3d.player.UnityPlayer";
        
        private readonly AndroidJavaObject _javaClass;
        private Action<string[]> _onNetworkStatusChanged;
        
        public ConnectivityWatcherAndroid(Action<bool> initCallback)
        {
            AndroidJavaObject currentActivity = null;
            using (var unityPlayer = new AndroidJavaClass(UNITY_PLAYER_CLASS_NAME))
            {
                currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }

            using (var connectivity = new AndroidJavaClass(CONNECTIVITY_CLASS_NAME))
            {
                _javaClass = connectivity.CallStatic<AndroidJavaObject>("getInstance");
            }
            
            // AndroidJavaObject currentActivity = new AndroidJavaObject(UNITY_PLAYER_CLASS_NAME).GetStatic<AndroidJavaObject>("currentActivity");
            // _javaClass = new AndroidJavaObject(CONNECTIVITY_CLASS_NAME).CallStatic<AndroidJavaObject>("getInstance");
            
            if (!_javaClass.Call<bool>("initialize", currentActivity))
            {
                Debug.LogError($"{LOG_TAG} --- Failed to initialize ConnectivityWatcherAndroid");
                initCallback?.Invoke(false);
                return;
            }

            // 注册状态监听器
            _javaClass.Call("registerListener", new ConnectivityEvent(OnNetworkStatusChanged));
            // 启动监听器
            _javaClass.Call("startupMonitor");
            initCallback?.Invoke(true);
        }
        
        
        
        
        /// <summary>
        /// 状态改变
        /// </summary>
        /// <param name="status"></param>
        private void OnNetworkStatusChanged(string[] status)
        {
            _onNetworkStatusChanged?.Invoke(status);
        }

        /// <summary>
        /// 主动绑定监听方法
        /// </summary>
        /// <param name="handler"></param>
        public void SetNetworkStatusListener(Action<string[]> handler)
        {
            _onNetworkStatusChanged = handler;
        }


        /// <summary>
        /// 主动获取当前的网络状态参数
        /// </summary>
        /// <returns></returns>
        public string[] GetNetConnectivity()
        {
            return _javaClass.Call<string[]>("checkNetConnectivity");  
        }


        private class ConnectivityEvent : AndroidJavaProxy
        {
            private readonly Action<string[]> _onStatusChanged;

            public ConnectivityEvent(Action<string[]> onChangeHandler) : base(CONNECTIVITY_EVENT_CLASS_NAME)
            {
                _onStatusChanged = onChangeHandler;
            }

            public void OnNetConnectivityChanging(string[] curState)
            {
                _onStatusChanged?.Invoke(curState);
            }
        }

       
    }
    
    
#endif
}