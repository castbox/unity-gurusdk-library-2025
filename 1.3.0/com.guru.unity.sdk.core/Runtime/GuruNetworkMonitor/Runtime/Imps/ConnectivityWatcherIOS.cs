namespace Guru.Network
{
#if UNITY_IOS
    using System;
    using UnityEngine;
    using System.Runtime.InteropServices;
    using AOT;
    
    /// <summary>
    /// 连接监听器 iOS
    /// </summary>
    public class ConnectivityWatcherIOS : IConnectivityWatcher
    {
        public const string LOG_TAG = "[NET][I]";
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void StartupMonitorCompletionEvent(bool success);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NetConnectivityChangingEvent(IntPtr result);

        [DllImport("__Internal")]
        private static extern bool initialize();

        [DllImport("__Internal")]
        private static extern void startupMonitor(StartupMonitorCompletionEvent listener);

        [DllImport("__Internal")]
        private static extern void registerNetConnectivityChangingListener(NetConnectivityChangingEvent listener);
        
        [DllImport("__Internal")]
        private static extern IntPtr checkNetConnectivity();
        
        [DllImport("__Internal")]
        private static extern void freeResult(IntPtr result);
        
        private static Action<string[]> _onNetworkStatusChanged;

        public ConnectivityWatcherIOS(Action<bool> initCallback)
        {
            var result = initialize();
            if (!result)
            f
                Debug.LogWarning($"{LOG_TAG} --- Failed to initialize ConnectivityWatcherIOS");
                initCallback?.Invoke(false);
                return; 
            }

            // 注册状态变更监听
            registerNetConnectivityChangingListener(NetConnectivityChangingEventProxy);
            // 启动监听器
            startupMonitor(StartupMonitorCompleteEventProxy);
            initCallback?.Invoke(true);
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
            IntPtr ptr = checkNetConnectivity();
            string[] result = IntPtrToArray(ptr);
            freeResult(ptr);
            return result;
        }

        /// <summary>
        /// 指针转字符数组
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        private static string[] IntPtrToArray(IntPtr ptr)
        {
            string result = Marshal.PtrToStringAnsi(ptr);
            return result?.Split(',');
        }

        // -------- iOS Listener Callback Define -------
        [MonoPInvokeCallback(typeof(StartupMonitorCompletionEvent))]
        private static void StartupMonitorCompleteEventProxy(bool success)
        {
            //TODO: Monitor 启动成功结果， 暂时不做任何处理
            Debug.Log($"{LOG_TAG} --- Monitor Startup: {success}");
        }

        [MonoPInvokeCallback(typeof(NetConnectivityChangingEvent))]
        private static void NetConnectivityChangingEventProxy(IntPtr resultPtr)
        {
            _onNetworkStatusChanged?.Invoke(IntPtrToArray(resultPtr));
        }
    }
#endif
}