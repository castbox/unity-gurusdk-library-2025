namespace Guru
{
    using System;
    using System.Collections.Generic;
   
    /// <summary>
    /// 数据来源
    /// </summary>
    public enum ValueSource
    {
        None = 0,
        Default, // 默认值
        Local,   // 本地缓存, 每次从磁盘中加载后为， 类型为本地缓存
        Remote,  // 远程值, 当次数据从远程数据中写入后，类型为远程值
    }

    /// <summary>
    /// 云控配置管理器
    /// 负责远程配置的获取、缓存和管理
    /// </summary>
    public class RemoteConfigManager
    {
        private const float DEFAULT_FETCH_TIMEOUT = 15f;
        private const string TAG = "[Remote]";

        private readonly RemoteConfigModel _configModel;        // 配置数据模型
        private readonly FirebaseRemoteService _remoteService;  // 远程服务管理器
        private readonly bool _isDebugMode;                     // 是否为调试模式

        private bool _isInitialized;                            // 是否已初始化
        private DateTime _lastFetchTime;                        // 最后一次拉取时间

        /// <summary>
        /// 初始化远程配置管理器
        /// </summary>
        /// <param name="defaults">默认配置值</param>
        /// <param name="onFetchResult">拉取结果回调</param>
        /// <param name="isDebugMode">是否开启调试模式</param>
        public RemoteConfigManager(Dictionary<string, object> defaults = null, 
            Action<bool> onFetchResult = null,
            bool isDebugMode = false)
        {
			_isDebugMode = isDebugMode;

			// 初始化数据模型
            _configModel = new RemoteConfigModel(defaults ?? new Dictionary<string, object>());
            
			// 初始化 Firebase 远程管理器
            _remoteService = new FirebaseRemoteService(
                DEFAULT_FETCH_TIMEOUT, 
                OnRemoteKeysChanged);

            _isInitialized = true;
            
            // 启动时立即拉取所有配置
            FetchAllAsync(onFetchResult);
        }

        #region 配置管理

        /// <summary>
        /// 添加新的默认值配置
        /// </summary>
        /// <param name="defaults">要添加的默认配置字典</param>
        public void AddDefaultValues(Dictionary<string, object> defaults)
        {
            if (defaults == null || defaults.Count == 0) return;
            _configModel.UpdateDefaultValues(defaults);
        }

        
        #endregion

        #region 数据拉取

        /// <summary>
        /// 拉取所有远程配置
        /// </summary>
        /// <param name="callback">拉取完成回调(true:成功 false:失败)</param>
        public void FetchAllAsync(Action<bool> callback = null)
        {
            if (!_isInitialized)
            {
                LogW($"{TAG} RemoteConfig Not init");
                callback?.Invoke(false);
                return;
            }

            LogI($"{TAG} Start FetchAll");
            _remoteService.FetchAllConfigsAsync((success, configValues) =>
            {
                if (success)
                {
                    _configModel.UpdateConfigValues(configValues);
                    _lastFetchTime = DateTime.UtcNow;
                    LogI($"{TAG} FetchAllAsync success");
                }
                else
                {
                    LogW($"{TAG} FetchAllAsync failed");
                }
                callback?.Invoke(success);
            });
        }

        /// <summary>
        /// 远程配置更新回调
        /// </summary>
        private void OnRemoteKeysChanged(string[] changedKeys)
        {
            if (changedKeys == null || changedKeys.Length == 0) return;
            try
            {
                var updates = new Dictionary<string, string>(changedKeys.Length);
                foreach (var key in changedKeys)
                {
                    var oldValue = _configModel.GetStringValue(key);
                    var newValue = _remoteService.GetStringValue(key);
                    
                    if (_isDebugMode)
                    {
                        LogI($"Config[{key}] changed: {oldValue} -> {newValue}");
                    }
                    
                    updates[key] = newValue;
                }
                
                _configModel.UpdateConfigValues(updates);
            }
            catch (Exception ex)
            {
                LogE("Handle remote value changed and get error:", ex);
            }
        }

        #endregion

        #region 数据获取接口

        /// <summary>
        /// 获取字符串值
        /// </summary>
        public string GetStringValue(string key, string defaultValue = "") => 
            _configModel.GetStringValue(key, defaultValue);

        /// <summary>
        /// 获取整数值
        /// </summary>
        public int GetIntValue(string key, int defaultValue = 0) => 
            _configModel.GetIntValue(key, defaultValue);

        /// <summary>
        /// 获取长整数值
        /// </summary>
        public long GetLongValue(string key, long defaultValue = 0) => 
            _configModel.GetLongValue(key, defaultValue);

        /// <summary>
        /// 获取双精度浮点值
        /// </summary>
        public double GetDoubleValue(string key, double defaultValue = 0d) => 
            _configModel.GetDoubleValue(key, defaultValue);

        /// <summary>
        /// 获取布尔值
        /// </summary>
        public bool GetBoolValue(string key, bool defaultValue = false) => 
            _configModel.GetBoolValue(key, defaultValue);

        /// <summary>
        /// 获取单精度浮点值
        /// </summary>
        public float GetFloatValue(string key, float defaultValue = 0f) => 
            _configModel.GetFloatValue(key, defaultValue);

        /// <summary>
        /// 尝试获取配置值
        /// </summary>
        public bool TryGetRemoteData(string key, out GuruConfigValue value) => 
            _configModel.TryGetValue(key, out value);

        /// <summary>
        /// 获取配置值
        /// </summary>
        public GuruConfigValue GetData(string key)
        {
            TryGetRemoteData(key, out var value);
            return value;
        }

        /// <summary>
        /// 获取所有配置数据
        /// </summary>
        public Dictionary<string, GuruConfigValue> GetAllValues()
        {
            var defaultValue = new Dictionary<string, GuruConfigValue>();
            if (!_isInitialized)
            {
                LogW("RemoteConfigManager not initialized");
                return defaultValue;
            }

            try
            {
                return _configModel.GetAllCachedValues();
            }
            catch (Exception ex)
            {
                LogE($"Fetch Config get error:", ex);
                return defaultValue;
            }
        }

        
        #endregion

        #region 日志工具

        private static void LogI(string msg) => 
            Log.I(TAG, msg);
        
        private static void LogE(string msg, Exception ex)
        {
            Log.E(TAG, msg);
            if(ex != null) Log.Exception(ex);
        }
        
        private static void LogW(string msg) => 
            Log.W(TAG, msg);
        
        #endregion
    }
}
