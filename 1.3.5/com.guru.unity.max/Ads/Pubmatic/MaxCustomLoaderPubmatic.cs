namespace Guru.Ads.Max
{
    using OpenWrapSDK;
    using System;
    using UnityEngine;
    using Guru.Ads;
    
    public class MaxCustomLoaderPubmatic
    {

        /// <summary>
        /// Pubmatic 广告渠道预加载器
        /// 需要在启动前设置商店链接
        /// </summary>
        /// <param name="storeUrl"></param>
        public MaxCustomLoaderPubmatic(string storeUrl)
        {
            if (string.IsNullOrEmpty(storeUrl))
            {
                Debug.LogWarning($"[Ads][Pubmatic] storeUrl is Empty, will not initial Pubmatic SDK !!!");
                return;
            }

            var appInfo = new POBApplicationInfo
            {
                StoreURL = new Uri(storeUrl)
            };
            
#if UNITY_EDITOR
            // 真机才会启动脚本
            Debug.Log($"[Ads][Pubmatic] Pubmatic Editor runner start  with storeUrl:{storeUrl}");
            return;
#endif
            POBOpenWrapSDK.SetApplicationInfo(appInfo);
        }

    }
}