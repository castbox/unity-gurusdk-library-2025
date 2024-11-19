

namespace Guru
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using UnityEngine;
    using Firebase.RemoteConfig;
    using Firebase.Extensions;
    
    
    
    /// <summary>
    /// 运控配置管理器
    /// </summary>
    public class RemoteConfigManager
    {
        private const double DefaultUpdateHours = 2;
        private const double DefaultFetchTimeout = 15;
        internal const string Tag = "[Remote]";
        private static bool _initOnce = false;
        private static RemoteConfigManager _instance;
        public static RemoteConfigManager Instance
        {
            get
            {
                if (_instance == null) _instance = new RemoteConfigManager();
                return _instance;
            }
        }

        private FirebaseRemoteConfig _firebaseRemote;
        private static bool _isDebug = false;
        private static double _fetchIntervalHours = DefaultUpdateHours;

        private RemoteConfigModel _model;
        private RemoteConfigModel Model => _model ??= RemoteConfigModel.LoadOrCreate();
        
        private static Dictionary<string, object> _defaultValues;

        public static event Action<bool> OnFetchCompleted;

        private Dictionary<string, Action<string,string>> _changeEvents;

        private Dictionary<string, string> _staticValues;
        
        public static void Init(Dictionary<string, object> defaults = null, double updateHours = DefaultUpdateHours, bool isDebug = false)
        {
            if (_initOnce) return;
            Instance.InitAssets(defaults, updateHours, isDebug);
        }
        
        // 拉取所有的线上配置数据
        // onFetchComplete 传参 true: 拉取成功   false: 拉取失败
        public static void FetchAll(bool immediately = false)
        {
            if (!_initOnce)
            {
                LogE("Mgr not ready, call Init first.");
                return;
            }
            Instance.FetchAllConfigs(immediately);
        }

        public static void AddDefaultValues(Dictionary<string, object> dict)
        {
            if (!_initOnce)
            {
                LogE("Mgr not ready, call Init first.");
                return;
            }
            Instance.AppendDefaultValues(dict);
        }

        #region 初始化

        private void InitAssets(Dictionary<string, object> defaults = null, double updateHours = DefaultUpdateHours, bool isDebug = false)
        {
            _fetchIntervalHours = updateHours;
            _isDebug = isDebug;
            _firebaseRemote = FirebaseRemoteConfig.DefaultInstance;
            if (_firebaseRemote == null)
            {
                LogE("Can't find FirebaseRemoteConfig.DefaultInstance, init failed.");
                return;
            }
            
            // 设置默认配置
            _firebaseRemote.SetConfigSettingsAsync(new ConfigSettings()
            {
                FetchTimeoutInMilliseconds = (ulong)(DefaultFetchTimeout * 1000),
                // MinimumFetchInternalInMilliseconds = (ulong)(_fetchIntervalHours * 60 * 60 * 1000)
            });

            _firebaseRemote.OnConfigUpdateListener += OnFirebaseConfigUpdatedHandler;

            // 设置默认值
            AppendDefaultValues(defaults);
            _initOnce = true;
            
            // 监听事件合集
            _changeEvents = new Dictionary<string, Action<string,string>>(30);
            
            // 立即拉取所有的配置            
            FetchAll(true);
        }

        private void AppendDefaultValues(Dictionary<string, object> defaults)
        {
            if (defaults == null) return;
            
            if(_defaultValues == null) _defaultValues = new Dictionary<string, object>(20);

            for(int i = 0; i < defaults.Keys.Count; i++)
            {
                string key = defaults.Keys.ElementAt(i);
                object value = defaults.Values.ElementAt(i);
                _defaultValues[key] = value;
            }
            _firebaseRemote?.SetDefaultsAsync(_defaultValues);
        }

        /// <summary>
        /// 拉取所有Remote 配置并激活 （默认激活间隔 2 小时）
        /// 官方文档：
        /// https://firebase.google.com/docs/reference/unity/class/firebase/remote-config/firebase-remote-config#class_firebase_1_1_remote_config_1_1_firebase_remote_config_1a55b6f0ebc2b457e9c0e2ac7c52cc87fa
        /// </summary>
        /// <param name="immediately">如果=true，相当于执行一次 FetchAndActivate 立即激活拉取到的配置</param>
        private void FetchAllConfigs(bool immediately = false)
        {
            var span = TimeSpan.FromHours(_fetchIntervalHours);

            if (_isDebug || immediately)
            {
                span = TimeSpan.Zero;
            }
  
            _firebaseRemote.FetchAsync(span)
                .ContinueWithOnMainThread(fetchTask =>
                {
                    if (fetchTask.IsFaulted || fetchTask.IsCanceled)
                    {
                        string res = fetchTask.IsFaulted? "Faulted" : "Canceled";
                        LogE($" --- Fetch AllConfigs fails: {res}");
                        OnFetchCompleted?.Invoke(false);
                        return;
                    }

                    _firebaseRemote.ActivateAsync()
                        .ContinueWithOnMainThread(activateTask =>
                    {
                        if (activateTask.IsFaulted || activateTask.IsCanceled)
                        {
                            string res = activateTask.IsFaulted? "Faulted" : "Canceled";
                            LogE($" --- Active AllConfigs fails: {res}");
                            OnFetchCompleted?.Invoke(false);
                            return;
                        }
                   
                        LogI($"[REMOTE] --- ActiveAsync success!");
                        OnFetchDataCompleted();
                        OnFetchCompleted?.Invoke(true);   
                    });
                });
        }
        
        /// <summary>
        /// 获取值更新回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="updateEvent"></param>
        private void OnFirebaseConfigUpdatedHandler(object sender, ConfigUpdateEventArgs updateEvent)
        {
            if (updateEvent.Error != RemoteConfigError.None)
            {
                Debug.LogError($"{Tag} --- RemoteConfigError: {updateEvent.Error}");
                return;
            }
        
            _firebaseRemote.ActivateAsync().ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompleted) return;
                if (updateEvent.UpdatedKeys == null) return;
                
                var updateKeys = updateEvent.UpdatedKeys.ToArray();
                int count = updateEvent.UpdatedKeys.Count();
                for(int i = 0; i < count; i++)
                {
                    string key = updateEvent.UpdatedKeys.ElementAt(i);
                    if (_changeEvents.TryGetValue(updateKeys[i], out var callback))
                    {
                        callback?.Invoke(key, _firebaseRemote.GetValue(key).StringValue);   
                    }  
                }
            });
            
        }



        #endregion

        #region Model
        
        /// <summary>
        /// 判断是否为 Config 参数
        /// </summary>
        /// <param name="rawStr"></param>
        /// <returns></returns>
        private bool IsRemoteConfigStr(string rawStr)
        {
            if (string.IsNullOrEmpty(rawStr)) return false;
            
            if (rawStr.TrimStart().StartsWith("{") 
                && rawStr.TrimEnd().EndsWith("}") 
                && rawStr.Contains("\"enable\":"))
            {
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 拉取成功
        /// </summary>
        private void OnFetchDataCompleted()
        {
            var values = _firebaseRemote.AllValues;
            var updates = new Dictionary<string, string>(values.Count);
            var configs = new Dictionary<string, string>(values.Count);
            for (int i = 0; i < values.Keys.Count; i++)
            {
                var key = values.Keys.ElementAt(i);
                var value = values.Values.ElementAt(i);
                var str = value.StringValue;

                updates[key] = str;
                if (IsRemoteConfigStr(str))
                {
                    configs[key] = str;
                }
            }

            if (configs.Count > 0)
            {
                Model.UpdateConfigs(configs);
            }

            DispatchUpdateValues(updates);
        }
        
        #endregion
        
        #region 数据接口

        public bool HasKey(string key)
        {
            return _firebaseRemote.Keys.Contains(key);
        }


        private bool TryGetDefaultValue<T>(string key, out T value)
        {
            value = default(T);
            if(_defaultValues != null && _defaultValues.TryGetValue(key, out var obj))
            {
                value = (T)obj;
                return true;
            }
            return false;
        }


        public bool TryGetValue<T>(string key, out T value, out ValueSource source, T defaultValue = default)
        {
            bool result = false;
            value = defaultValue;
            source = ValueSource.DefaultValue;
            if (HasKey(key))
            {
                var v = _firebaseRemote.GetValue(key);
                source = v.Source;
                if (value is int)
                {
                    value = (T)Convert.ChangeType(v.LongValue, typeof(T));
                    result = true;
                }
                else if(value is long)
                {
                    value = (T)Convert.ChangeType(v.LongValue, typeof(T));
                    result = true;
                }
                else if(value is double)
                {
                    value = (T)Convert.ChangeType(v.DoubleValue, typeof(T));
                    result = true;
                }
                else if(value is float)
                {
                    value = (T)Convert.ChangeType(v.DoubleValue, typeof(T));
                    result = true;
                }
                else if(value is bool)
                {
                    value = (T)Convert.ChangeType(v.BooleanValue, typeof(T));
                    result = true;
                }
                else if (value is byte[])
                {
                    value = (T)Convert.ChangeType(v.ByteArrayValue.ToArray(), typeof(T));
                    result = true;
                }
                else if (value is string)
                {
                    value = (T)Convert.ChangeType(v.StringValue, typeof(T));
                    result = true;
                }
            }
            return result;
        }


        public string GetStringValue(string key, string defaultValue = "")
        {
            if (_firebaseRemote != null)
            {
                try
                {
                    return _firebaseRemote.GetValue(key).StringValue;
                }
                catch (Exception e)
                {
                    LogException(e);
                }
            }
            
            if (TryGetDefaultValue<string>(key, out var value))
            {
                return value;
            }
            
            return defaultValue;
        }

        public int GetIntValue(string key, int defaultValue = 0)
        {
            if (_firebaseRemote != null)
            {
                try
                {
                    return (int) _firebaseRemote.GetValue(key).LongValue;
                }
                catch (Exception e)
                {
                    LogException(e);
                }
            }

            if (TryGetDefaultValue<int>(key, out var value))
            {
                return value;
            }

            return defaultValue;
        }
        
        public long GetLongValue(string key, long defaultValue = 0)
        {
            if (_firebaseRemote != null)
            {
                try
                {
                    return _firebaseRemote.GetValue(key).LongValue;
                }
                catch (Exception e)
                {
                    LogException(e);
                }
            }
            
            if (TryGetDefaultValue<long>(key, out var value))
            {
                return value;
            }
            
            return defaultValue;
        }

        public double GetDoubleValue(string key, double defaultValue = 0)
        {
            if (_firebaseRemote != null)
            {
                try
                {
                    return _firebaseRemote.GetValue(key).DoubleValue;
                }
                catch (Exception e)
                {
                    LogException(e);
                }
            }
            
            if (TryGetDefaultValue<double>(key, out var value))
            {
                return value;
            }
            
            return defaultValue;
        }
        
        public bool GetBoolValue(string key, bool defaultValue = false)
        {
            if (_firebaseRemote != null)
            {
                try
                {
                    return _firebaseRemote?.GetValue(key).BooleanValue ?? defaultValue;
                }
                catch (Exception e)
                {
                    LogException(e);
                }
            }
            
            if (TryGetDefaultValue<bool>(key, out var value))
            {
                return value;
            }
            
            return defaultValue;
        }

        /// <summary>
        /// 获取全部值
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, ConfigValue> GetAllValues()
        {
            if (Instance._firebaseRemote != null)
            {
                try
                {
                    return (Dictionary<string, ConfigValue>)(Instance._firebaseRemote.AllValues);
                }
                catch (Exception e)
                {
                    LogException(e);
                }
            }
            return null;
        }

        [CanBeNull]
        public static string GetStaticValue(string key)
        {
            if (Instance._staticValues != null && Instance._staticValues.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

        #endregion
        
        #region 云控值获取

        public static string GetString(string key, string defaultValue = "")
        {
            return Instance.GetStringValue(key, defaultValue);
        }
        
        public static int GetInt(string key, int defaultValue = 0)
        {
            return Instance.GetIntValue(key, defaultValue);
        }
        
        public static long GetLong(string key, long defaultValue = 0)
        {
            return Instance.GetLongValue(key, defaultValue);
        }
        
        public static double GetDouble(string key, double defaultValue = 0)
        {
            return Instance.GetDoubleValue(key, defaultValue);
        }

        public static float GetFloat(string key, float defaultValue = 0)
        {
            return (float) Instance.GetDoubleValue(key, defaultValue);
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            return Instance.GetBoolValue(key, defaultValue);
        }

        #endregion
        
        #region 云控配置获取

        /// <summary>
        /// 注册云控配置对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultJson"></param>
        public static void RegisterConfig(string key, string defaultJson)
        {
            Instance.Model.SetDefaultConfig(key, defaultJson); // 配置默认值
        }


        /// <summary>
        /// 获取云控配置
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetConfig<T>(string key) where T : IRemoteConfig<T>
        {
            var config = Instance.Model.Get<T>(key);
            return config;
        }



        #endregion

        #region 监听云控值变化

        /// <summary>
        /// 注册值变化事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="onValueChanged"></param>
        public static void RegisterOnValueChanged(string key, Action<string,string> onValueChanged)
        {
            Instance.AddOnValueChangeListener(key, onValueChanged);
        }
        /// <summary>
        /// 取消注册值变化事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="onValueChanged"></param>
        public static void UnRegisterOnValueChanged(string key, Action<string,string> onValueChanged)
        {
            Instance.RemoveOnValueChangeListener(key, onValueChanged);
        }


        private void AddOnValueChangeListener(string key, Action<string,string> onValueChanged)
        {
            if (_changeEvents == null) _changeEvents = new Dictionary<string, Action<string,string>>(30);

            if (HasOnValueChangeListener(key))
            {
                _changeEvents[key] += onValueChanged;
            }
            else
            {
                _changeEvents[key] = onValueChanged;
            }
        }
        
        private void RemoveOnValueChangeListener(string key, Action<string,string> onValueChanged)
        {
            if (_changeEvents != null && HasOnValueChangeListener(key))
            {
                _changeEvents[key] -= onValueChanged;
            }
        }

        private bool HasOnValueChangeListener(string key)
        {
            if (_changeEvents != null)
            {
                return _changeEvents.ContainsKey(key);
            }

            return false;
        }

        /// <summary>
        /// 将所有云控值变化的参数通过回调通知给订阅者
        /// </summary>
        /// <param name="updates"></param>
        private void DispatchUpdateValues(Dictionary<string, string> updates)
        {
            Dictionary<string, string> changes = new Dictionary<string, string>(updates.Count);

            if (_staticValues == null) _staticValues = new Dictionary<string, string>();

            string key, value;
            for (int i = 0; i < updates.Keys.Count; i++)
            {
                key = updates.Keys.ElementAt(i);
                value = updates.Values.ElementAt(i);

                if (_staticValues.TryGetValue(key, out var oldValue))
                {
                    if (oldValue != updates[key] && _changeEvents.ContainsKey(key))
                    {
                        changes[key] = value;
                    }
                }
                else
                {
                    changes[key] = value;
                }
            }
            
            // --------- 发送值变化事件 ------------
            for (int i = 0; i < changes.Keys.Count; i++)
            {
                key = updates.Keys.ElementAt(i);
                value = updates.Values.ElementAt(i);

                if (_changeEvents.TryGetValue(key, out var callback))
                {
                    callback?.Invoke(key, value);
                }
            }
            _staticValues = updates;
        }


        #endregion
        
        #region Log

        private static void LogI(string msg, params object[] args)
        {
            Log.I(Tag, msg, args);
        }
        
        private static void LogE(string msg, params object[] args)
        {
            Log.E(Tag, msg, args);
        }
        
        private static void LogW(string msg, params object[] args)
        {
            Log.W(Tag, msg, args);
        }

        private static void LogException(Exception e)
        {
            Log.Exception(e);
        }
        
        #endregion
    }
}