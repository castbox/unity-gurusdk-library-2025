

namespace Guru.Ads.Max
{
    using System;
    using UnityEngine;
    using System.Collections;
    using Guru.Ads;
    
    internal class AdCoroutineHelper:MonoBehaviour
    {
        private static AdCoroutineHelper _instance;

        public static AdCoroutineHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("__ads_coroutine_helper__");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<AdCoroutineHelper>();
                }
                return _instance;
            }
        }
        
        public Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return StartCoroutine(enumerator);
        }


        public Coroutine StartDelayed(float delay, Action callback)
        {
            return StartCoroutine(DelayWithSeconds(delay, callback));
        }
        
        
        public Coroutine StartDelayed(WaitForSecondsRealtime realtime, Action callback)
        {
            return StartCoroutine(DelayWithRealSeconds(realtime, callback));
        }

        /// <summary>
        /// 延迟执行
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator DelayWithSeconds(float delay, Action callback)
        {
            yield return new WaitForSeconds(delay);
            callback?.Invoke();
        }
        
        
        private IEnumerator DelayWithRealSeconds(WaitForSecondsRealtime realSeconds, Action callback)
        {
            yield return realSeconds;
            callback?.Invoke();
        }

    }

    public class MaxAdHelper
    {

        /// <summary>
        /// 获取重试等待时间
        /// </summary>
        /// <param name="retryCount"></param>
        /// <returns></returns>
        private static float GetRetryDelaySeconds(int retryCount)
        {
            // 最低 2 秒 
            // 递增 2^retryCount 秒
            // 最高 2^6 = 64 秒
            return (float)Math.Pow(2, Math.Min(AdConst.RELOAD_TIME, retryCount));  
        }

        /// <summary>
        /// 延迟处理
        /// </summary>
        /// <param name="delaySeconds"></param>
        /// <param name="callback"></param>
        public static Coroutine DelayAction(float delaySeconds, Action callback)
        {
            return AdCoroutineHelper.Instance.StartDelayed(new WaitForSecondsRealtime(delaySeconds), callback);
        }
        
        /// <summary>
        /// 异步重新加载广告
        /// </summary>
        /// <param name="retryCount"></param>
        /// <param name="reloadHandler"></param>
        public static Coroutine ReloadAdAsync(int retryCount, Action reloadHandler)
        {
            return AdCoroutineHelper.Instance.StartCoroutine(OnAdReloading(retryCount, reloadHandler));
        }
        
        private static IEnumerator OnAdReloading(int retryCount, Action reloadHandler)
        {
            while (IsNetworkNotReachable())
            {
                yield return new WaitForSecondsRealtime(AdConst.NO_NETWORK_WAITING_TIME);
            }

            var delaySeconds = GetRetryDelaySeconds(retryCount);
            yield return new WaitForSecondsRealtime(delaySeconds);
            
            reloadHandler?.Invoke();
        }
        
        /// <summary>
        /// 网络不可用
        /// </summary>
        /// <returns></returns>
        private static bool IsNetworkNotReachable()
        {
            return Application.internetReachability == NetworkReachability.NotReachable;
        }
    }
}