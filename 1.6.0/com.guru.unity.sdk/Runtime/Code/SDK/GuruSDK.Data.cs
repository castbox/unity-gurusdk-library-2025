#nullable enable
using System.Collections.Generic;

namespace Guru
{
    
    
    
    public partial class GuruSDK
    {

        #region Adjust 配置
        
        private string? _adjustToken;
        private Dictionary<string, string>? _adjustEventMap = null;

        /// <summary>
        /// 初始化 Adjust 字典和配置
        /// </summary>
        /// <param name="settings"></param>
        private void InitAdjustEventMap()
        {
            
            if(!_appServicesConfig.IsAdjustEnabled())
            {
                UnityEngine.Debug.Log("!Adjust is not enabled");
                return;
            }
            
            var settings = _appServicesConfig.adjust_settings;
            
#if UNITY_ANDROID
            _adjustToken = settings.AndroidToken();
#elif UNITY_IOS
            _adjustToken = settings.iOSToken();
#endif
            
            // 填充字典
            _adjustEventMap = new Dictionary<string, string>(settings.events.Length);
            for (int i = 0; i < settings.events.Length; i++)
            {
                var raw = settings.events[i];

                if (string.IsNullOrEmpty(raw))
                    continue;
                
                var arr = raw.Split(',');
                var eventName = arr[0];
                var eventToken = "";
#if UNITY_ANDROID
                if (arr.Length > 1) eventToken = arr[1];
#elif UNITY_IOS
                if (arr.Length > 2) eventToken = arr[2];
#endif
                _adjustEventMap[eventName] = eventToken;

            }
        }
        
        // Adjust Token 配置        
        public string? GetAdjustToken()
        {
            if (!_appServicesConfig.IsAdjustEnabled())
            {
                UnityEngine.Debug.Log("!Adjust is not enabled");
                return null;
            }
            return _adjustToken;
        }

        // Adjust 事件地图
        public Dictionary<string, string>? GetAdjustEventMap()
        {
            if (!_appServicesConfig.IsAdjustEnabled())
            {
                UnityEngine.Debug.Log("!Adjust is not enabled");
                return null;
            }
            return _adjustEventMap;
        }

        public string? GetAdjustEventToken(string eventName)
        {
            if (!_appServicesConfig.IsAdjustEnabled())
            {
                UnityEngine.Debug.Log("!Adjust is not enabled");
                return null;
            }

            return _adjustEventMap.GetValueOrDefault(eventName);
        }

        #endregion

    }
}