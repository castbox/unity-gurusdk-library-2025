

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
    internal class RemoteCacheDoc
    {
        private const string TAG = "[RemoteCache]";
        private const int DEFAULT_CAPACITY = 20;
        private const string CACHE_KEY = "guru_remote_cache_doc";
        
        /// <summary>
        /// 缓存的配置条目
        /// </summary>
        public Dictionary<string, CacheEntry> _entries;
        
        /// <summary>
        /// 初始化缓存文档
        /// </summary>
        /// <param name="configValues">初始配置值</param>
        public RemoteCacheDoc(Dictionary<string, GuruConfigValue> configValues = null)
        {
            _entries = new Dictionary<string, CacheEntry>(configValues?.Count ?? DEFAULT_CAPACITY);

            if (configValues == null) return;
            
            foreach (var kvp in configValues)
            {
                _entries[kvp.Key] = new CacheEntry(kvp.Value);
            }
        }
        
        #region 静态工具方法

        /// <summary>
        /// 从本地加载缓存的配置数据
        /// </summary>
        /// <returns>配置数据字典,无缓存时返回空字典</returns>
        public static Dictionary<string, GuruConfigValue> LoadFromCache()
        {
            try
            {
                string json = PlayerPrefs.GetString(CACHE_KEY);
                
                if (string.IsNullOrEmpty(json))
                {
                    Debug.Log($"{TAG} 本地无缓存数据");
                    return new Dictionary<string, GuruConfigValue>();
                }

                var doc = JsonParser.ToObject<RemoteCacheDoc>(json);
                return doc?.ToConfigValues() ?? new Dictionary<string, GuruConfigValue>();
            }
            catch (Exception e)
            {
                Debug.LogError($"{TAG} 加载缓存失败: {e.Message}");
                return new Dictionary<string, GuruConfigValue>();
            }
        }

        /// <summary>
        /// 将配置数据保存到本地缓存
        /// </summary>
        /// <param name="configValues">要缓存的配置数据</param>
        public static async UniTask SaveToCache(Dictionary<string, GuruConfigValue> configValues)
        {
            if (configValues == null)
            {
                Debug.LogWarning($"{TAG} 配置数据为空,取消保存");
                return;
            }

            try
            {
                var doc = new RemoteCacheDoc(configValues);
                await doc.SaveAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"{TAG} 保存缓存失败: {e.Message}");
            }
        }

        #endregion
        
        
        #region 实例方法

        /// <summary>
        /// 将缓存条目转换为配置值字典
        /// </summary>
        public Dictionary<string, GuruConfigValue> ToConfigValues()
        {
            var configValues = new Dictionary<string, GuruConfigValue>(_entries.Count);
            
            foreach (var (key, entry) in _entries)
            {
                configValues[key] = entry.ToConfigValue();
            }
            
            return configValues;
        }

        /// <summary>
        /// 异步保存缓存到本地
        /// </summary>
        private async UniTask SaveAsync()
        {
            await UniTask.SwitchToMainThread();
            
            string json = JsonParser.ToJson(this);
            PlayerPrefs.SetString(CACHE_KEY, json);
            PlayerPrefs.Save();
            
            Debug.Log($"{TAG} 已保存 {_entries.Count} 个配置项到本地缓存");
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
                Source = ValueSource.Local // 从缓存加载的数据源为Local
            };
        }

    }
    
}
