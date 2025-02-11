

#if UNITY_EDITOR

namespace Guru.Network
{
    using System.Collections;
    using UnityEditor;
    using System;
    using UnityEngine;
    using Guru;
    
    public class ConnectivityWatcherEditor: IConnectivityWatcher
    {
        private const string LOG_TAG = "[NET][E]";
        private NetworkReachability _lastNetReachability;
        private Action<string[]> _onNetworkStatusChanged;
        private readonly Action<bool> _initCompleteHandler;

        private static ConnectivityWatcherEditor _instance;

        public ConnectivityWatcherEditor(Action<bool> initResult)
        {
            Debug.Log($"{LOG_TAG} --- Init ConnectivityWatcherEditor ---");
            _lastNetReachability = Application.internetReachability;
            CoroutineHelper.Instance.StartCoroutine(OnEditorLoopCheck());
            initResult?.Invoke(true);

            _instance = this;
        }

        /// <summary>
        /// 获取虚拟的网络状态
        /// </summary>
        /// <returns></returns>
        private string GetMockNetworkStatus(NetworkReachability reachability)
        {
            switch (reachability)
            {
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    return "mobile";
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    return "wifi";
            }
            return "none";
        }


        public void SetNetworkStatusListener(Action<string[]> handler)
        {
            _onNetworkStatusChanged = handler;
        }

        public string[] GetNetConnectivity()
        {
            return new string[] { GetMockNetworkStatus(_lastNetReachability) };
        }


        IEnumerator OnEditorLoopCheck()
        {
            while (EditorApplication.isPlaying)
            {
                if (_lastNetReachability != Application.internetReachability)
                {
                    _lastNetReachability = Application.internetReachability;
                    _onNetworkStatusChanged?.Invoke(GetNetConnectivity());
                }
                yield return new WaitForSeconds(10);
            }
        }

        public static void OnExternalStatusChange(string[] mockStatus)
        {
            if (_instance == null)
            {
                Debug.LogWarning("No ConnectivityWatcherEditor Instance found. cannot set Mock status...");
                return;
            }
            
            _instance._onNetworkStatusChanged?.Invoke(mockStatus);
        }

    }
}
#endif