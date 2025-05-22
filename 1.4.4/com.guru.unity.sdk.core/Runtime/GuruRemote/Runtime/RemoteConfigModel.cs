
using UnityEngine;

namespace Guru
{
    using System;
    using System.Collections.Generic;
    
    /// <summary>
    /// 远程配置管理模型，用于处理远程配置的获取、缓存和更新
    /// </summary>
    internal class RemoteConfigModel
    {
        private const string TAG = "[RCModel]";
        // 布尔值的字符串匹配模式
        internal static readonly string[] BOOL_TRUE_PATTERNS = new string[] { "1", "true", "on", "yes" };
        internal static readonly string[] BOOL_FALSE_PATTERNS = new string[] { "0", "false", "off", "no" };
        private static readonly DateTime EPOCH = DateTime.UnixEpoch;
        // 存储默认配置值
        private readonly Dictionary<string, object> _defaultValues;
        // 存储缓存的配置值
        private Dictionary<string, GuruConfigValue> _cachedValues;
        // 最后一次成功更新的时间
        private DateTime _lastUpdateSuccessTime;
        
        /// <summary>
        /// 初始化远程配置模型
        /// </summary>
        public RemoteConfigModel(Dictionary<string, object> defaults)
        {
            _lastUpdateSuccessTime = EPOCH;
            _defaultValues = new Dictionary<string, object>(defaults);
            LoadLocalCache();
        }

        private async void LoadLocalCache()
        {
            try
            {
                _cachedValues = await RemoteConfigLocalCacheIO.LoadFromDisk();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{TAG} 加载缓存失败: {ex.Message}");
                _cachedValues = new Dictionary<string, GuruConfigValue>();
            }
        }


        /// <summary>
        /// 添加新的默认配置值
        /// </summary>
        /// <param name="newDefaults">新的默认配置键值对</param>
        public void UpdateDefaultValues(Dictionary<string, object> newDefaults)
        {
            foreach (var kvp in newDefaults)
            {
                _defaultValues[kvp.Key] = kvp.Value;
            }
        }
        
        /// <summary>
        /// 更新给定的远程配置数据
        /// </summary>
        /// <param name="configValues"></param>
        public void UpdateConfigValues(Dictionary<string, string> configValues)
        {
            _lastUpdateSuccessTime = DateTime.UtcNow; // 记录成功拉取的时间
            
            foreach (var kvp in configValues)
            {
                UpdateConfigValue(kvp.Key, kvp.Value, _lastUpdateSuccessTime, false);
            }
            SaveToDisk(_cachedValues);
        }

        /// <summary>
        /// 更新单个配置值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="value">配置值</param>
        /// <param name="updateTime">更新时间</param>
        /// <param name="saveImmediately">是否立即保存到缓存</param>
        private void UpdateConfigValue(string key, string value, DateTime updateTime = default, bool saveImmediately = true)
        {
            if(updateTime == default) updateTime = DateTime.UtcNow;
            
            _cachedValues[key] = new GuruConfigValue()
            {
                Source = ValueSource.Remote,
                Value = value,
                LastUpdated = updateTime
            };

            if (saveImmediately)
            {
                SaveToDisk(_cachedValues);
            }
        }
        
        /// <summary>
        /// 保存当前配置到本地缓存
        /// </summary>
        private static async void SaveToDisk(Dictionary<string, GuruConfigValue> configValues)
        {
            try
            {
                await RemoteConfigLocalCacheIO.SaveToDisk(configValues);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{TAG} 保存缓存失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 尝试获取配置值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="configValue">输出的配置值</param>
        /// <returns>是否成功获取配置值</returns>
        public bool TryGetValue(string key, out GuruConfigValue configValue)
        {
            if (_cachedValues.TryGetValue(key, out var cachedValue))
            {
                configValue = cachedValue;
                return true;
            }

            if (_defaultValues.TryGetValue(key, out var defaultValue))
            {
                configValue = new GuruConfigValue
                {
                    Source = ValueSource.Default,
                    Value = defaultValue.ToString(),
                    LastUpdated = EPOCH
                };
                return true;
            }
            
            configValue = new GuruConfigValue
            {
                Source = ValueSource.None,
                Value = string.Empty,
                LastUpdated = EPOCH
            };
            return false;
        }
        
        #region 数据获取

        public string GetStringValue(string key, string defaultValue = "")
        {
            return TryGetValue(key, out var value) ? value.GetValue(defaultValue) : defaultValue;
        }

        public int GetIntValue(string key, int defaultValue = 0)
        {
            return TryGetValue(key, out var value) ? value.GetValue(defaultValue) : defaultValue;
        }
        
        public long GetLongValue(string key, long defaultValue = 0)
        {
            return TryGetValue(key, out var value) ? value.GetValue(defaultValue) : defaultValue;
        }

        public double GetDoubleValue(string key, double defaultValue = 0)
        {
            return TryGetValue(key, out var value) ? value.GetValue(defaultValue) : defaultValue;
        }
        
        public float GetFloatValue(string key, float defaultValue = 0)
        {
            return TryGetValue(key, out var value) ? value.GetValue(defaultValue) : defaultValue;
        }
        
        public bool GetBoolValue(string key, bool defaultValue = false)
        {
            return TryGetValue(key, out var value) ? value.GetValue(defaultValue) : defaultValue;
        }
        
        /// <summary>
        /// 获取所有缓存的配置数据
        /// </summary>
        public Dictionary<string, GuruConfigValue> GetAllCachedValues()
        {
            return new Dictionary<string, GuruConfigValue>(_cachedValues);
        }

        #endregion
        
    }
    
    

}