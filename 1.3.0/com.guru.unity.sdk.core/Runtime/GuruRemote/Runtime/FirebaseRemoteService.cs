namespace Guru
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Firebase.Extensions;
    using Firebase.RemoteConfig;
    using UnityEngine;
    using System.Threading.Tasks;
    
    /// <summary>
    /// Firebase 云控数据接口
    /// </summary>
    public class FirebaseRemoteService : IDisposable
    {
        private const string TAG = "[Remote][Firebase]";
        
        // Firebase远程配置实例
        private readonly FirebaseRemoteConfig _remoteConfig;
        
        // 配置更新回调
        private readonly Action<string[]> _onConfigUpdateCallback;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fetchTimeoutInSeconds">拉取超时时间(秒)</param>
        /// <param name="onConfigUpdate">配置更新回调</param>
        public FirebaseRemoteService(float fetchTimeoutInSeconds = 60f, 
            Action<string[]> onConfigUpdate = null)
        {
            try{
                _remoteConfig = FirebaseRemoteConfig.DefaultInstance;
                _onConfigUpdateCallback = onConfigUpdate;
                
                // 初始化配置
                InitializeConfig(fetchTimeoutInSeconds);
                
                // 挂载 Config 更新事件监听
                _remoteConfig.OnConfigUpdateListener += HandleConfigUpdate;
                
                IsInitialized = true;
                Debug.Log($"{TAG} init complete, fetch Timeout: {fetchTimeoutInSeconds}s");
            }
            catch (Exception e)
            {
                Debug.LogError($"{TAG} 初始化失败: {e.Message}");
                IsInitialized = false;
            }
        }
        
        /// <summary>
        /// 初始化Firebase配置
        /// </summary>
        private void InitializeConfig(float fetchTimeoutInSeconds)
        {
            var configSettings = new ConfigSettings
            {
                FetchTimeoutInMilliseconds = (ulong)(fetchTimeoutInSeconds * 1000),
                MinimumFetchIntervalInMilliseconds = 3600000 // 1小时
            };

            _remoteConfig.SetConfigSettingsAsync(configSettings);
        }
        
        /// <summary>
        /// 处理配置更新事件
        /// </summary>
        private void HandleConfigUpdate(object sender, ConfigUpdateEventArgs args)
        {

            if (args.Error != RemoteConfigError.None)
            {
                Debug.LogError($"{TAG} Update ConfigValue error: {args.Error}");
                return;
            }
            
            ActivateUpdatedConfig(args.UpdatedKeys?.ToArray());
        }
        
        /// <summary>
        /// 激活更新的配置
        /// </summary>
        private void ActivateUpdatedConfig(string[] updatedKeys)
        {
            try 
            {
                _remoteConfig.ActivateAsync()
                    .ContinueWithOnMainThread(task =>
                {
                    if (task.Result)
                    {
                        _onConfigUpdateCallback?.Invoke(updatedKeys);
                        Debug.Log($"{TAG} Active ConfigValue success, new value: {updatedKeys?.Length ?? 0}");
                    }
                    else
                    {
                        Debug.Log($"{TAG} Active ConfigValue failed");   
                    }
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"{TAG} Active ConfigValue failed: {e.Message}");
            }
        }
        
        /*/// <summary>
        /// 异步获取所有远程配置
        /// </summary>
        /// <returns>配置键值对字典</returns>
        public void FetchAllConfigsAsyncV1(Action<bool, Dictionary<string, string>> updateCallback)
        {
            try
            {
                Debug.Log($"{TAG} 开始拉取配置");
                _remoteConfig.FetchAsync(TimeSpan.Zero)
                    .ContinueWithOnMainThread(fetchTask =>
                {
                    if (!fetchTask.IsCompletedSuccessfully)
                    {
                        Debug.Log($"{TAG} 拉取配置失败");
                        updateCallback?.Invoke(false, null);
                        return;
                    }

                    Debug.Log($"{TAG}--- 开始激活配置 ---");
                    _remoteConfig.ActivateAsync()
                        .ContinueWithOnMainThread(activeTask =>
                        {
                            var success = activeTask.Result;
                            if (success)
                            {
                                updateCallback?.Invoke(true, GetAllConfigValues());
                                Debug.LogError($"{TAG} 配置激活成功");
                            }
                            else
                            {
                                updateCallback?.Invoke(false, null);
                                Debug.LogError($"{TAG} 配置激活失败");
                            }
                        });
                });
            }
            catch (Exception e)
            {
                updateCallback?.Invoke(false, null);
                Debug.LogError($"{TAG} 获取配置失败: {e.Message}");
            }
        }*/
        
        /// <summary>
        /// 异步获取所有远程配置
        /// </summary>
        public async void FetchAllConfigsAsync(Action<bool, Dictionary<string, string>> updateCallback)
        {
            try
            {
                Debug.Log($"{TAG} start fetch");
                var fetchTask =  _remoteConfig.FetchAsync(TimeSpan.Zero);
                await fetchTask;
                if (fetchTask.IsFaulted || fetchTask.IsCanceled)
                {
                    updateCallback?.Invoke(false, null);
                    Debug.LogError($"{TAG} fetch failed");
                    return;
                }
                
                var activeTask = _remoteConfig.ActivateAsync();
                await activeTask;
                if (activeTask.Status == TaskStatus.RanToCompletion)
                {
                    updateCallback?.Invoke(true, GetAllConfigValues());
                    Debug.Log($"{TAG} active success");
                }
                else
                {
                    updateCallback?.Invoke(false, null);
                    Debug.LogError($"{TAG} active failed: {activeTask.Status}");
                }
                
                // await _remoteConfig.ActivateAsync().ContinueWithOnMainThread(activeTask =>
                //     {
                //         var success = activeTask.Result;
                //         if (success)
                //         {
                //             updateCallback?.Invoke(true, GetAllConfigValues());
                //             Debug.Log($"{TAG} 配置激活成功");
                //         }
                //         else
                //         {
                //             updateCallback?.Invoke(false, null);
                //             Debug.LogError($"{TAG} 配置激活失败");
                //         }
                //     });
            }
            catch (Exception e)
            {
                updateCallback?.Invoke(false, null);
                Debug.LogError($"{TAG} Fetch config get error: {e.Message}");
            }
        }
        
        
        /// <summary>
        /// 获取所有 ConfigValues 
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetAllConfigValues()
        {
            return _remoteConfig.AllValues
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.StringValue);
        }

        /// <summary>
        /// 构建GuruConfigValue对象
        /// </summary>
        private GuruConfigValue BuildConfigValue(ConfigValue value, DateTime updateTime = default)
        {
            if (updateTime == default) 
            {
                updateTime = DateTime.UnixEpoch;
            }

            var source = value.Source switch
            {
                Firebase.RemoteConfig.ValueSource.RemoteValue => ValueSource.Remote,
                Firebase.RemoteConfig.ValueSource.DefaultValue => ValueSource.Default,
                Firebase.RemoteConfig.ValueSource.StaticValue => ValueSource.Local,
                _ => ValueSource.Default
            };

            return new GuruConfigValue
            {
                Source = source,
                Value = value.StringValue,
                LastUpdated = updateTime
            };
        }
        
        #region 获取配置值方法
        
        public string GetStringValue(string key, string defaultValue = "") =>
            _remoteConfig.AllValues.TryGetValue(key, out var value) ? value.StringValue : defaultValue;
        
        public long GetLongValue(string key, long defaultValue = 0) =>
            _remoteConfig.AllValues.TryGetValue(key, out var value) ? value.LongValue : defaultValue;
        
        public bool GetBoolValue(string key, bool defaultValue = false) =>
            _remoteConfig.AllValues.TryGetValue(key, out var value) ? value.BooleanValue : defaultValue;
        
        public double GetDoubleValue(string key, double defaultValue = 0) =>
            _remoteConfig.AllValues.TryGetValue(key, out var value) ? value.DoubleValue : defaultValue;
        
        
        #endregion
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_remoteConfig != null)
            {
                _remoteConfig.OnConfigUpdateListener -= HandleConfigUpdate;
            }
        }

    }
}