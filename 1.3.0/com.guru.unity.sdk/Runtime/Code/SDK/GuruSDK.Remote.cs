namespace Guru
{
    using System;
    using System.Collections.Generic;
    using Firebase.RemoteConfig;

    public partial class GuruSDK
    {
        private RemoteConfigManager _remoteConfigManager;
        
        private void InitRemoteConfigManager(Dictionary<string, object> defaults = null, bool isDebug = false)
        {
            _remoteConfigManager = new RemoteConfigManager(defaults, 
                OnFirstFetchRemoteComplete,
                isDebug);
        }

        public static void FetchAllRemote(bool immediately = false)
        {
            Instance._remoteConfigManager.FetchAllAsync();
        }
        
        public static string GetRemoteString(string key, string defaultValue = "") => Instance._remoteConfigManager.GetStringValue(key, defaultValue);
        public static int GetRemoteInt(string key, int defaultValue = 0) => Instance._remoteConfigManager.GetIntValue(key, defaultValue);
        public static long GetRemoteLong(string key, long defaultValue = 0 ) => Instance._remoteConfigManager.GetLongValue(key, defaultValue);
        public static double GetRemoteDouble(string key, double defaultValue = 0) => Instance._remoteConfigManager.GetDoubleValue(key, defaultValue);
        public static float GetRemoteFloat(string key, float defaultValue = 0) => Instance._remoteConfigManager.GetFloatValue(key, defaultValue);
        public static bool GetRemoteBool(string key, bool defaultValue = false) => Instance._remoteConfigManager.GetBoolValue(key, defaultValue);
        public static GuruConfigValue GetConfigValue(string key) => Instance._remoteConfigManager.GetData(key);

        
        /// <summary>
        /// 获取所有云控配置
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, GuruConfigValue> GetConfigValues() => Instance._remoteConfigManager.GetAllValues();
        
    }
}