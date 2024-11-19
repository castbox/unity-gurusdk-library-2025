namespace Guru
{
    using System;
    using System.Collections.Generic;
    using Firebase.RemoteConfig;

    public partial class GuruSDK
    {

        public static void FetchAllRemote(bool immediately = false) => RemoteConfigManager.FetchAll(immediately);
        
        /// <summary>
        /// 注册云控配置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static void RegisterRemoteConfig(string key, string defaultValue)
        {
            RemoteConfigManager.RegisterConfig(key, defaultValue);
        }

        /// <summary>
        /// 获取运控配置
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetRemoteConfig<T>(string key) where T : IRemoteConfig<T>
        {
            return RemoteConfigManager.GetConfig<T>(key);
        }
        public static string GetRemoteString(string key, string defaultValue = "") => RemoteConfigManager.GetString(key, defaultValue);
        public static int GetRemoteInt(string key, int defaultValue = 0) => RemoteConfigManager.GetInt(key, defaultValue);
        public static long GetRemoteLong(string key, long defaultValue = 0 ) => RemoteConfigManager.GetLong(key, defaultValue);
        public static double GetRemoteDouble(string key, double defaultValue = 0) => RemoteConfigManager.GetDouble(key, defaultValue);
        public static float GetRemoteFloat(string key, float defaultValue = 0) => RemoteConfigManager.GetFloat(key, defaultValue);
        public static bool GetRemoteBool(string key, bool defaultValue = false) => RemoteConfigManager.GetBool(key, defaultValue);

        
        /// <summary>
        /// 注册监听某个 Key 的变化
        /// </summary>
        /// <param name="key"></param>
        /// <param name="onValueChanged"></param>
        public static void RegisterOnValueChanged(string key, Action<string,string> onValueChanged)
        {
            RemoteConfigManager.RegisterOnValueChanged(key, onValueChanged);
        }
        
        /// <summary>
        /// 注销监听某个 Key 的变化
        /// </summary>
        /// <param name="key"></param>
        /// <param name="onValueChanged"></param>
        public static void UnRegisterOnValueChanged(string key, Action<string,string> onValueChanged)
        {
            RemoteConfigManager.UnRegisterOnValueChanged(key, onValueChanged);
        }
        
        /// <summary>
        /// 获取所有云控配置
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, ConfigValue> GetRemoteAllValues() => RemoteConfigManager.GetAllValues();
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetRemoteStaticValue(string key) => RemoteConfigManager.GetStaticValue(key);
        
    }
}