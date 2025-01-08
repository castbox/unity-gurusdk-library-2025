


namespace Guru.Ads.Max
{
    using System;
    using UnityEngine;
    using Guru.Ads;
    using Cysharp.Threading.Tasks;
    
    public class MaxAdHelper
    {

        /// <summary>
        /// 获取重试等待时间
        /// </summary>
        /// <param name="retryCount"></param>
        /// <returns></returns>
        private static double GetRetryDelaySeconds(int retryCount)
        {
            // 最低 2 秒 
            // 递增 2^retryCount 秒
            // 最高 2^6 = 64 秒
            return Math.Pow(2, Math.Clamp(retryCount, 1, AdConst.MAX_RELOAD_COUNT)); // (2^1 ~ 2^6)  
        }

        /// <summary>
        /// 异步等待秒数
        /// </summary>
        /// <param name="delaySeconds"></param>
        /// <param name="ignoreTimeScale"></param>
        public static async UniTask WaitSecondsAsync(float delaySeconds, bool ignoreTimeScale = true)
        {
            await UniTask.Delay((int)delaySeconds * 1000, ignoreTimeScale);
        }

        /// <summary>
        /// 延迟执行某事
        /// </summary>
        /// <param name="delaySeconds"></param>
        /// <param name="callback"></param>
        public static async UniTaskVoid DelayAction(int delaySeconds, Action callback)
        {
            await WaitSecondsAsync(delaySeconds);
            callback?.Invoke();
        }

        /// <summary>
        /// 通过重试加载广告
        /// </summary>
        /// <param name="retryCount"></param>
        /// <param name="reloadHandler"></param>
        public static async UniTaskVoid ReloadByRetryCount(int retryCount, Action reloadHandler)
        {
            while (IsNetworkNotReachable())
            {
                // Debug.Log($"[Ads] --- Network Not reachable: {Application.internetReachability}");
                await UniTask.Delay(TimeSpan.FromSeconds(AdConst.NO_NETWORK_WAITING_TIME));
            }
            
            // await UniTask.Delay(TimeSpan.FromSeconds(GetRetryDelaySeconds(retryCount)));
            await WaitSecondsAsync((float)GetRetryDelaySeconds(retryCount));
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
        
        
        /// <summary>
        /// Hex 字符串转 Color 对象
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static Color HexToColor(string hexString)
        {
            if(string.IsNullOrEmpty(hexString)) return Color.clear;
            
            var hex = hexString.Replace("#", "");
            if(hex.Length < 6) return Color.clear;
            
            int num = System.Convert.ToInt32(hex, 16);
 
            // 将一个十六进制数转换为Color
            // 假设十六进制字符串是 RRGGBBAA 格式
            byte r = (byte)(num & 0xFF); // 红色
            byte g = (byte)(num >> 8 & 0xFF); // 绿色
            byte b = (byte)(num >> 16 & 0xFF); // 蓝色
            byte a = (byte)(num >> 24 & 0xFF); // 透明度
            // 创建Color对象
            return new Color32(r, g, b, a);
        }
        
    }
}