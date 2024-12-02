


namespace Guru
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Cysharp.Threading.Tasks;
    
    /// <summary>
    /// 本地缓存数据对象
    /// </summary>
    [Serializable]
    internal class RemoteConfigLocalCache
    {
        private const int DEFAULT_CAPACITY = 20;
        
        /// <summary>
        /// 缓存的配置条目
        /// </summary>
        public Dictionary<string, CacheEntry> entries;
        
        /// <summary>
        /// 初始化缓存文档
        /// </summary>
        /// <param name="configValues">初始配置值</param>
        public RemoteConfigLocalCache(Dictionary<string, GuruConfigValue> configValues = null)
        {
            entries = new Dictionary<string, CacheEntry>(configValues?.Count ?? DEFAULT_CAPACITY);

            if (configValues == null) return;
            
            foreach (var (key, value) in configValues)
            {
                entries[key] = new CacheEntry(value);
            }
        }
        
        
        #region 实例方法

        /// <summary>
        /// 将缓存条目转换为配置值字典
        /// </summary>
        public Dictionary<string, GuruConfigValue> ToConfigValues()
        {
            var configValues = new Dictionary<string, GuruConfigValue>(entries.Count);
            
            foreach (var (key, entry) in entries)
            {
                configValues[key] = entry.ToConfigValue();
            }
            
            return configValues;
        }

       

        #endregion
        
    }
    
    
    /// <summary>
    /// RemoteDataInfo
    /// </summary>
    [Serializable]
    internal class CacheEntry
    {
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public string LastUpdateTime { get; set; }
        
        /// <summary>
        /// 配置值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 创建缓存条目
        /// </summary>
        /// <param name="configValue">配置值对象</param>
        public CacheEntry(GuruConfigValue configValue)
        {
            LastUpdateTime = configValue.LastUpdated.ToString("yyyy-MM-dd HH:mm:ss");
            Value = configValue.Value;
        }

        /// <summary>
        /// 转换为配置值对象
        /// </summary>
        public GuruConfigValue ToConfigValue()
        {
            return new GuruConfigValue
            {
                LastUpdated = DateTime.Parse(LastUpdateTime),
                Value = Value,
                Source = ValueSource.Local // 从本地存储加载的数据源为Local
            };
        }

    }


    internal class RemoteConfigLocalCacheIO
    {
        
        private const string TAG = "[RCLocalCache]";
        private const string CACHE_KEY = "remote_config_local_cache";

        
        #region 静态工具方法

        /// <summary>
        /// 从本地加载缓存的配置数据
        /// </summary>
        /// <returns>配置数据字典,无缓存时返回空字典</returns>
        public static async UniTask<Dictionary<string, GuruConfigValue>> LoadFromDisk()
        {
            await UniTask.SwitchToMainThread();
            
            string json = PlayerPrefs.GetString(CACHE_KEY);
            
            if (string.IsNullOrEmpty(json))
            {
                Debug.Log($"{TAG} 本地无缓存数据");
                return new Dictionary<string, GuruConfigValue>();
            }

            var doc = JsonParser.ToObject<RemoteConfigLocalCache>(json);
            return doc?.ToConfigValues() ?? new Dictionary<string, GuruConfigValue>();
        }

        /// <summary>
        /// 将配置数据保存到本地缓存
        /// </summary>
        /// <param name="configValues">要缓存的配置数据</param>
        public static async UniTask SaveToDisk(Dictionary<string, GuruConfigValue> configValues)
        {
            if (configValues == null)
            {
                Debug.LogWarning($"{TAG} 配置数据为空,取消保存");
                return;
            }
            var doc = new RemoteConfigLocalCache(configValues);
            await UniTask.SwitchToMainThread();
            string json = JsonParser.ToJson(doc);
            PlayerPrefs.SetString(CACHE_KEY, json);
            PlayerPrefs.Save();
            // Debug.Log($"{TAG} 已保存 {doc._entries.Count} 个配置项到本地缓存");
        }

        #endregion

    }
    
    
    
    
    
}
