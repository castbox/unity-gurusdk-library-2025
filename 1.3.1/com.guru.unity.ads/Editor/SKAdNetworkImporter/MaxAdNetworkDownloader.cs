namespace Guru.Editor
{
    using System;
    using UnityEngine.Networking;
    using UnityEditor;
    
    public class MaxAdNetworkDownloader
    {
        public const string VERSION = "1.0.0";
        private const string APPLOVIN_SKADNETWORK_API = "https://skadnetwork-ids.applovin.com/v1/skadnetworkids.json";
        private const string REQUEST_API = "adnetworks";
        private static readonly string[] ADNETWORKS = new string[]
        {
            "amazon-ad-marketplace",
            "bidmachine",
            "bigoads",
            "chartboost",
            "fyber",
            "google-ad-manager",
            "google",
            "hyprmx",
            "inmobi",
            "ironsource",
            "vungle",
            "line",
            "maio",
            "facebook",
            "mintegral",
            "mobilefuse",
            "moloco",
            "ogury-presage",
            "tiktok",
            "pubmatic",
            "smaato",
            "unityads",
            "verve",
            "mytarget",
            "yandex",
            "yso-network"
        };

        private UnityWebRequest _request;
        private Action<bool, string> _onLoadCompleteHandler;
        private Action<float> _onLoadingProgress;

        public MaxAdNetworkDownloader()
        {
            
        }


        public void StartDownload(Action<bool, string> onComplete, Action<float> onProgress = null)
        {
            _onLoadCompleteHandler = onComplete;
            _onLoadingProgress = onProgress;
            
            var url = $"{APPLOVIN_SKADNETWORK_API}?{REQUEST_API}={string.Join(",", ADNETWORKS)}";
            _request = UnityWebRequest.Get(url);
            EditorApplication.update += OnCheckLoadResult;
            _request.SendWebRequest();

        }
        
        /// <summary>
        /// 检查结果
        /// </summary>
        private void OnCheckLoadResult()
        {
            var stopLoading = false;

            switch (_request.result)
            {
                case UnityWebRequest.Result.Success:
                    stopLoading = true;
                    _onLoadCompleteHandler?.Invoke(true, _request.downloadHandler.text);
                    break;
                case UnityWebRequest.Result.InProgress:
                    // TODO loading Progress
                    _onLoadingProgress?.Invoke(_request.downloadProgress);
                    break;
                default:
                    stopLoading = true;
                    _onLoadCompleteHandler?.Invoke(false, _request.error);
                    break;
            }

            if (stopLoading)
            {
                EditorApplication.update -= OnCheckLoadResult;
            }

        }


    }
}