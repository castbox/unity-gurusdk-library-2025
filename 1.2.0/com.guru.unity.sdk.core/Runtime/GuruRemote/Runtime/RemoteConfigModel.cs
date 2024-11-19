using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Guru
{
    
    
    
    [Serializable]
    internal class RemoteConfigModel
    {
        private static float SaveInterval = 2f;
        private const string SaveKey = "comr.guru.remote.model.save";
        public Dictionary<string, string> configs;
        public long last_modified = 0;

        private float _lastSavedTime = 0;
        
        /// <summary>
        /// 创建或加载
        /// </summary>
        /// <returns></returns>
        public static RemoteConfigModel LoadOrCreate()
        {
            RemoteConfigModel model = null;
            if (PlayerPrefs.HasKey(SaveKey))
            {
                string json = LoadStringValue(SaveKey);
                model = JsonParser.ToObject<RemoteConfigModel>(json);
            }
            if (model == null) model = new RemoteConfigModel();
            return model;
        }


        /// <summary>
        /// 默认赋值数据
        /// </summary>
        private Dictionary<string, string> _defConfigs;

        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static string LoadStringValue(string key, string defaultValue = "")
        {
            if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetString(key, defaultValue);
            }
            return defaultValue;
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private static void SaveToPlayerPrefs(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }


        /// <summary>
        /// 初始化
        /// </summary>
        public RemoteConfigModel()
        {
            _defConfigs = new Dictionary<string, string>(20);
            configs = new Dictionary<string, string>(20);
        }

        /// <summary>
        /// 是否有数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKey(string key) => configs.ContainsKey(key);

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="forceSave"></param>
        public void Save(bool forceSave = false)
        {
            if (forceSave || (Time.realtimeSinceStartup - _lastSavedTime > SaveInterval))
            {
                _lastSavedTime = Time.realtimeSinceStartup;
                last_modified = TimeUtil.GetCurrentTimeStamp();
                SaveToPlayerPrefs(SaveKey, JsonParser.ToJson(this));
            }
        }
        
        /// <summary>
        /// 设置默认值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetDefaultConfig(string key, string value)
        {
            _defConfigs[key] = value;
            if (!HasKey(key))
            {
                SetConfigValue(key, value);
            }
        }
        
        /// <summary>
        /// 设置当前值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="config"></param>
        public void SetDefaultConfig<T>(string key, T config) where T : IRemoteConfig<T>
        {
            var json = config.ToJson();
            SetDefaultConfig(key, json);
        }
        
        /// <summary>
        /// 获取配置对象
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(string key) where T : IRemoteConfig<T>
        {
            string json = "";
            if (HasKey(key))
            {
                json = configs[key];
            }
            else if (_defConfigs.TryGetValue(key, out var defValue))
            {
                json = defValue;   
            }

            if (!string.IsNullOrEmpty(json))
            {
                return JsonParser.ToObject<T>(json);
            }

            Log.E(RemoteConfigManager.Tag, $" --- Remote Key {key} has never been registered.");
            return default(T);
        }
        
        
        /// <summary>
        /// 设置对象值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        internal void SetConfigValue(string key, string value)
        {
            configs[key] = value;
            Save();
        }

        /// <summary>
        /// 更新所有的配置
        /// </summary>
        /// <param name="updates"></param>
        public void UpdateConfigs(Dictionary<string, string> updates)
        {
            string key, value;
            for (int i = 0; i < updates.Keys.Count; i++)
            {
                key = updates.Keys.ElementAt(i);
                value = updates.Values.ElementAt(i);

                if (!HasKey(key) || configs[key] != value)
                {
                  // New Key or Value Changed
                  configs[key] = value;
                }
            }
            Save(true); // 直接保存
        }


    }
}