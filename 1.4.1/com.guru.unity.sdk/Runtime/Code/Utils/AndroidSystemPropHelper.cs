

#if UNITY_ANDROID

namespace Guru.Utils
{
    using System;
    using UnityEngine;
    
    /// <summary>
    /// Android 平台，在 SDK 启动的时候判断是否
    /// </summary>
    public class AndroidSystemPropHelper
    {
        
        private const string K_CMD_NAME_DEBUGGER = "debug.com.guru.debugger";
        private const string K_CMD_NAME_WATERMARK = "debug.com.guru.watermark";
        private const string K_CMD_NAME_CONSOLE = "debug.com.guru.console";
        private readonly string _appBundleId;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="appBundleId"></param>
        public AndroidSystemPropHelper(string appBundleId)
        {
            _appBundleId = appBundleId;
        }
        
        /// <summary>
        /// Debugger 是否可用
        /// </summary>
        /// <returns></returns>
        public bool IsDebuggerEnabled()
        {
            var value = GetPropValue(K_CMD_NAME_DEBUGGER);
            return value.Equals(_appBundleId);
        }

        /// <summary>
        /// 水印是否可用
        /// </summary>
        /// <returns></returns>
        public bool IsWatermarkEnabled()
        {
            var value = GetPropValue(K_CMD_NAME_WATERMARK);
            return value.Equals(_appBundleId);
        }

        /// <summary>
        /// 控制台是否可用
        /// </summary>
        /// <returns></returns>
        public bool IsConsoleEnabled()
        {
            var value = GetPropValue(K_CMD_NAME_CONSOLE);
            return value.Equals(_appBundleId);
        }
        
        
        #region Android API
        
        private AndroidJavaClass _systemPropsCls;
        private const string SYSTEM_PROPS_CLASS = "android.os.SystemProperties";

        private string GetPropValue(string key)
        {
            try
            {
                if (_systemPropsCls == null)
                {
                    _systemPropsCls = new AndroidJavaClass(SYSTEM_PROPS_CLASS);
                }

                if (_systemPropsCls != null)
                {
                    return _systemPropsCls.CallStatic<string>("get", key);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }

            return "";
        }
        
        

        #endregion
    }
}

#endif
