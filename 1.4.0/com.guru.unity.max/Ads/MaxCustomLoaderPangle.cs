using System;
using UnityEngine;

namespace Guru.Ads.Max
{
    /// <summary>
    /// Pangle 启动器
    /// </summary>
    public class MaxCustomLoaderPangle
    {
        private const string TAG = "[Ads][TT]";
#if UNITY_ANDROID
        private const string PAG_CONFIG_PACKAGE_NAME = "com.bytedance.sdk.openadsdk.api.init.PAGConfig";
#endif
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="debugMode"></param>
        public MaxCustomLoaderPangle(bool debugMode = false)
        {
#if UNITY_ANDROID
            InitAndroidConfig(debugMode);
#endif
            // 之后可以添加 iOS 的初始化信息
        }

#if UNITY_ANDROID

        /// <summary>
        /// 初始化配置
        /// </summary>
        /// <param name="debugMode"></param>
        /// <param name="audienceFlag"></param>
        private void InitAndroidConfig(bool debugMode = false, int audienceFlag = -1)
        {
            try
            {
                var pagConfig = new AndroidJavaClass(PAG_CONFIG_PACKAGE_NAME);
                
                pagConfig.CallStatic("debugLog", debugMode); // 开启 Debug 模式
                Debug.Log($"{TAG} ---  Set debugMode: {debugMode}");
                
                pagConfig.CallStatic("updateAudienceFlag", audienceFlag); // 调用合规接口
                Debug.Log($"{TAG} ---  Set audienceFlag: {audienceFlag}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{TAG} --- Pangle Exception: {ex.Message}");
            }
        }

#endif
    }
}