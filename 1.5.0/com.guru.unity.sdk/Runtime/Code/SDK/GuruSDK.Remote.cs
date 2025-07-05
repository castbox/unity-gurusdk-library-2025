
namespace Guru
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    public partial class GuruSDK
    {
        private RemoteConfigManager _remoteConfigManager;
        
        public static bool IsRemoteReady => Instance._remoteConfigManager?.IsReady ?? false;
        
        private void InitRemoteConfigManager(Dictionary<string, object> defaults = null, bool isDebug = false)
        {
            _remoteConfigManager = new RemoteConfigManager(defaults, 
                OnFetchRemoteComplete,
                isDebug);
        }

        public static void FetchAllRemote()
        {
            // Instance._remoteConfigManager.FetchAllAsync();
            Instance._remoteConfigManager.FetchAllAsync().Forget();
        }
        
        public static string GetRemoteString(string key, string defaultValue = "") => Instance._remoteConfigManager.GetStringValue(key, defaultValue);
        public static int GetRemoteInt(string key, int defaultValue = 0) => Instance._remoteConfigManager.GetIntValue(key, defaultValue);
        public static long GetRemoteLong(string key, long defaultValue = 0 ) => Instance._remoteConfigManager.GetLongValue(key, defaultValue);
        public static double GetRemoteDouble(string key, double defaultValue = 0) => Instance._remoteConfigManager.GetDoubleValue(key, defaultValue);
        public static float GetRemoteFloat(string key, float defaultValue = 0) => Instance._remoteConfigManager.GetFloatValue(key, defaultValue);
        public static bool GetRemoteBool(string key, bool defaultValue = false) => Instance._remoteConfigManager.GetBoolValue(key, defaultValue);
        public static GuruConfigValue GetConfigValue(string key) => Instance._remoteConfigManager.GetData(key);

        
        /// <summary>
        /// 获取已更新的云控配置
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, GuruConfigValue> GetUpdatedConfigValues() => Instance._remoteConfigManager.GetAllValues();

        /// <summary>
        /// 获取所有的云控配置
        /// 包括已更新的和预设值
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, GuruConfigValue> GetAllConfigValues()
        {
            string[] defaultKeys = Instance._initConfig.DefaultRemoteData.Keys.ToArray();
            Dictionary<string, GuruConfigValue> configValues = new Dictionary<string, GuruConfigValue>(defaultKeys.Length);
            
            // #1 先填充预设值
            foreach (var k in defaultKeys)
            {
                configValues[k] = GetConfigValue(k);
            }

            // #2 再填充所有更新值
            foreach (var (key, value) in GetUpdatedConfigValues())
            {
                configValues[key] = value;
            }
            
            return configValues;
        }
    }
}