// ReSharper disable ReplaceSubstringWithRangeIndexer
namespace Guru
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UnityEngine;
    using System.Diagnostics.CodeAnalysis;
    
    public class GuruAnalytics
    {
        // Plugin Version
        private const string Version = "1.13.1";
        
        public static readonly string LOG_TAG = "[GA]";
        private static readonly string ActionName = "logger_error";
        private const int EventPriorityDefault = 10;


        private static GuruAnalytics _instance;

        public static GuruAnalytics Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GuruAnalytics();
                }
                return _instance;
            }
        }


        private static bool _isReady = false;
        
        public static bool IsReady => _isReady;
        
        private IAnalyticsAgent _agent;
        private IAnalyticsAgent Agent
        {
            get
            {
                if (_agent == null)
                {
#if UNITY_EDITOR
                    _agent = new AnalyticsAgentMock();
#elif UNITY_ANDROID
                    _agent = new AnalyticsAgentAndroid();
#elif UNITY_IOS
                    _agent = new AnalyticsAgentIOS();
#endif
                }
                
                if (_agent == null)
                {
                    throw new NotImplementedException("You Should Implement IAnalyticsAgent on platform first.");
                }
                
                return _agent;
            }
        }

        private Dictionary<string, string> _userProperties;
        /// <summary>
        /// 用户属性缓存字典
        /// </summary>
        private Dictionary<string, string> UserProperties
        {
            get
            {
                if (_userProperties == null)
                {
                    _userProperties = new Dictionary<string, string>(10);
                }
                return _userProperties;
            }
        }

        private bool _enableErrorLog;

        private string _experimentGroupId;
        public string ExperimentGroupId => _experimentGroupId;
        private DateTime _lastReportTime;
        
        /// <summary>
        /// 启动日志错误上报
        /// </summary>
        public bool EnableErrorLog
        {
            get => _enableErrorLog;
            set
            {
                _enableErrorLog = value;
                if (_enableErrorLog) InitCallbacks(); // 激活错误日志回调
                if (Agent != null) Agent.EnableErrorLog = _enableErrorLog;
            }
        }

        #region 公用接口
        
        /// <summary>
        /// 初始化接口
        /// </summary>
        public void Init(string appId, string deviceInfo, string firebaseId, string guruSDKVersion, Action onInitComplete, bool isDebug = false)
        {
            Debug.Log($"{LOG_TAG} --- Guru Analytics [{Version}] initialing...");
            if (_isReady) return;

            if (Agent == null)
            {
                // Agent 不存在则抛异常
                throw new NotImplementedException($"{LOG_TAG} Agent is null, please check your implementation of IAnalyticsAgent.");
            }

            string groupId = "not_set";
            string baseUrl = "";
            string[] uploadIpAddress = null;
            bool enabelErrorLog = true;
            
            // 获取云控参数
            // TODO: 针对 GuruSDK 整体的云控值做一个分组的解决方案
            var guruInitParams = GuruAnalyticsConfigManager.GetInitParams();
            
            if (guruInitParams != null)
            {
                // 如果分组实验打开
                groupId = guruInitParams.groupId;
                baseUrl = guruInitParams.baseUrl;
                uploadIpAddress = guruInitParams.uploadIpAddress;
                enabelErrorLog = guruInitParams.enableErrorLog;
            }
            
            if (!string.IsNullOrEmpty(firebaseId))
                Agent.SetFirebaseId(firebaseId); // 需要提前设置 Firebase ID
            
            // 分组ID赋值
            _experimentGroupId = groupId;
            EnableErrorLog = enabelErrorLog;
            
            _lastReportTime = new DateTime(1970, 1, 1); // 初始化上报时间
            _isReady = true; // 初始化成功标志位
            
            // 初始化参数
            Agent.Init(appId, deviceInfo, baseUrl, uploadIpAddress, guruSDKVersion, onInitComplete, isDebug);
            
            Debug.Log($"{LOG_TAG} --- Guru Analytics [{Version}] initialized.");
            Debug.Log($"{LOG_TAG} --- GroupId: {groupId}");
        }
        
        /// <summary>
        /// 设置视图名称
        /// </summary>
        /// <param name="screenName"></param>
        public void SetScreen(string screenName)
        {
            if (!_isReady) 
            {
                Debug.LogWarning($"{LOG_TAG}[GA] --- is Not Ready SetScreen: {screenName}");
                return;
            }
            if (string.IsNullOrEmpty(screenName)) return;
            // CacheUserProperty($"screen_name", screenName);
            Agent.SetScreen(screenName);
        }

        /// <summary>
        /// 设置广告ID
        /// </summary>
        /// <param name="id"></param>
        public void SetAdId(string id)
        {
            if (!_isReady) 
            {
                Debug.LogWarning($"{LOG_TAG}[GA] --- is Not Ready SetAdId: {id}");
                return;
            }
            if (string.IsNullOrEmpty(id)) return;
            // CacheUserProperty($"ad_id", id);
            Agent.SetAdId(id);
        }

        /// <summary>
        /// 设置用户属性
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetUserProperty(string key, string value)
        {
            if (!_isReady)
            {
                Debug.LogWarning($"{LOG_TAG} --- is Not Ready SetUserProperty: [{key}, {value}] failed");
                return;
            }
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) return;
            // CacheUserProperty(key, value); // 添加用户属性
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            Debug.Log($"{LOG_TAG}  --- SetUserProperty: [{key}, {value}]");
            Agent.SetUserProperty(key, value);
        }
        
        /*
        /// <summary>
        /// 设置Firebase ID
        /// 此接口已经废弃，改为在自打点初始化时直接注入 FIREBASE_ID
        /// </summary>
        /// <param name="id"></param>
        public void SetFirebaseId(string id)
        {
            if (!_isReady) 
            {
                Debug.LogWarning($"{LOG_TAG}[GA] --- is Not Ready SetFirebaseId: {id}");
                return;
            };
            if (string.IsNullOrEmpty(id)) return;
            // CacheUserProperty($"firebase_id", id);
            Agent.SetFirebaseId(id);
        }
        */

        /// <summary>
        /// 设置Adjust ID
        /// </summary>
        /// <param name="id"></param>
        public void SetAdjustId(string id)
        {
            if (!_isReady)
            {
                Debug.LogWarning($"{LOG_TAG}[GA] --- is Not Ready SetAdjustId: {id}");
                return;
            }
            if (string.IsNullOrEmpty(id)) return;
            // CacheUserProperty($"adjust_id", id);
            Agent.SetAdjustId(id);
        }

        /// <summary>
        /// 设置设备ID
        /// </summary>
        /// <param name="deviceId"></param>
        public void SetDeviceId(string deviceId)
        {
            if (!_isReady)
            {
                Debug.LogWarning($"{LOG_TAG}[GA] --- is Not Ready SetDeviceId: {deviceId}");
                return;
            }
            if (string.IsNullOrEmpty(deviceId)) return;
            // CacheUserProperty($"device_id", deviceId);
            Agent.SetDeviceId(deviceId);
        }


        public void SetAndroidId(string androidId)
        {
            if (!_isReady) 
            {
                Debug.LogWarning($"{LOG_TAG}[GA] --- is Not Ready SetAndroidId: {androidId}");
                return;
            }
            if (string.IsNullOrEmpty(androidId)) return;
            // CacheUserProperty(Analytics.PropertyAndroidID, androidId);
            Agent.SetUserProperty(Analytics.PropertyAndroidId, androidId);
        }
        
        public void SetIDFV(string idfv)
        {
            if (!_isReady) 
            {
                Debug.LogWarning($"{LOG_TAG}[GA] --- is Not Ready SetIDFV: {idfv}");
                return;
            }
            if (string.IsNullOrEmpty(idfv)) return;
            // CacheUserProperty(Analytics.PropertyIDFV, idfv);
            Agent.SetUserProperty(Analytics.PropertyIDFV, idfv);
        }
        
        public void SetIDFA(string idfa)
        {
            if (!_isReady)
            {
                Debug.LogWarning($"{LOG_TAG}[GA] --- is Not Ready SetIDFA: {idfa}");
                return;
            }
            if (string.IsNullOrEmpty(idfa)) return;
            // CacheUserProperty(Analytics.PropertyIDFA, idfa);
            Agent.SetUserProperty(Analytics.PropertyIDFA, idfa);
        }


        /// <summary>
        /// 设置用户ID
        /// </summary>
        /// <param name="uid"></param>
        public void SetUid(string uid)
        {
            if (!_isReady)
            {
                Debug.LogWarning($"{LOG_TAG}[GA] --- is Not Ready SetUid: {uid}");
                return;
            }

            if (string.IsNullOrEmpty(uid)) return;
            // CacheUserProperty($"uid", uid);
            Agent.SetUid(uid);
        }

        /// <summary>
        /// 上报事件成功率
        /// </summary>
        public void ReportEventSuccessRate() => Agent.ReportEventSuccessRate();

        /// <summary>
        /// 上报打点事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="data">INT类型的值</param>
        /// <param name="priority"></param>
        [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
        public void LogEvent(string eventName, Dictionary<string, dynamic> data = null, EventPriority priority = EventPriority.Unknown)
        {
            string raw = "";
            if (data != null && data.Count > 0)
            {
                raw = BuildParamsJson(data);
            }
            if (priority == EventPriority.Unknown) priority = EventPriority.Default;
            Debug.Log($"{LOG_TAG} --- LogEvent GuruAnalytics:{eventName} | raw: {raw} | priority: {priority}");
            Agent.LogEvent(eventName, raw, (int)priority);
        }

        /*
        private static string BuildParamsString(Dictionary<string, dynamic> data)
        {
            string raw = "";
            List<string> strList = new List<string>(data.Count);
            foreach (var kvp in data)
            {
                strList.Add(BuildStringValue(kvp));
                raw = string.Join(",", strList);
            }
            return raw;
        }
        */

        [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
        private static string BuildParamsJson(Dictionary<string, dynamic> data)
        {
            try
            {
                //  强制转换加入国家设置
                return JsonConvert.SerializeObject(data, new JsonSerializerSettings()
                {
                    Culture = new CultureInfo("en-US"),
                });
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return "";
        }
        
        #endregion

        #region iOS独有接口

#if UNITY_IOS
        // 触发测试崩溃埋点
        public static void TestCrash() => AnalyticsAgentIOS.TestCrashEvent();
#endif

        #endregion

        #region 用户属性

        /// <summary>
        /// 记录用户属性
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void CacheUserProperty(string key, string value)
        {
            // bool needUpdate = !UserProperties.ContainsKey(key) || UserProperties[key] != value;
            UserProperties[key] = value;
            // if (needUpdate) UpdateAllUserProperties();
        }


        #endregion

        #region 日志回调

        private void InitCallbacks()
        {
            try
            {
                GuruSDKCallback.RemoveCallback(OnSDKCallback);
                GuruSDKCallback.AddCallback(OnSDKCallback);
                if (Agent != null)
                    Agent.InitCallback(GuruSDKCallback.ObjectName, GuruSDKCallback.MethodName);
            }
            catch (Exception ex)
            {
                Analytics.LogCrashlytics(ex);
            }

        }


        /// <summary>
        /// 获取SDK回调
        /// </summary>
        /// <param name="raw"></param>
        private void OnSDKCallback(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return;
            if (!raw.Contains($"\"{ActionName}\"")) return; // 不对其他行为的日志进行过滤
            ParseJsonAndSendEvent(raw);
        }

        /// <summary>
        /// 上报错误信息
        /// </summary>
        /// <param name="code"></param>
        /// <param name="errorInfo"></param>
        /// <param name="category"></param>
        /// <param name="extra"></param>
        private void ReportDevAuditEvent(int code, string errorInfo = "", string category = "", Dictionary<string, object> extra = null)
        {
            // Debug.Log($"{LOG_TAG} --- OnLoggerErrorEvent: code:{code}\t info:{errorInfo}");
            
            var codeString = ((AnalyticsCode)code).ToString();
            if (string.IsNullOrEmpty(codeString)) codeString = $"ErrorCode:{code}";
            if (string.IsNullOrEmpty(errorInfo)) errorInfo = "Empty";
            
            var dict = new Dictionary<string, dynamic>()
            {
                {"item_name", codeString},
                {"country", IPMConfig.IPM_COUNTRY_CODE},
                {"network", Application.internetReachability.ToString()},
                {"exp", _experimentGroupId}
            };

            if (!string.IsNullOrEmpty(category))
            {
                dict[Analytics.ParameterItemCategory] = category;
            }

            int len = 96;
            if (errorInfo.Length > len) 
                errorInfo = errorInfo.TrimStart().Substring(0, len);
            
            if (!string.IsNullOrEmpty(errorInfo))
                dict["err"] = errorInfo;

            if (extra != null)
            {
                foreach (var kvp in extra)
                {
                    dict[kvp.Key] = kvp.Value;
                }
            }
            
            // Only for firebase GA
            Analytics.LogDevAudit(dict);
        }
        
        private void ParseJsonAndSendEvent(string json)
        {
            Debug.Log($"{LOG_TAG} ------ ParseWithJson: json:\n{json}");
            
            int code = (int)AnalyticsCode.UNITY_INTERNAL_ERROR;
            string info = json;
            try
            {
                var dict = JsonConvert.DeserializeObject<JObject>(json);
                if (dict == null || !dict.TryGetValue("data", out var jData)) return;
                var j = jData.Value<JObject>();
                if (j == null || !j.TryGetValue("code", out var jCode)) return;
                code = jCode.Value<int>();
                if (!j.TryGetValue("msg", out var jMsg)) return;
                info = jMsg.Value<string>();
                ReportWithCodeAndInfo(code, info);
            }
            catch (Exception)
            {
                string p = "\"msg\":\"";
                string m = json;
                if (json.Contains(p)) m = json.Substring(json.IndexOf(p, StringComparison.Ordinal) + p.Length);
                info = $"JsonEX:{m}";
                // Debug.Log($"{LOG_TAG} --- {info}");
                Analytics.LogCrashlytics(json, false);
                Analytics.LogCrashlytics(info);
                ReportWithCodeAndInfo(code, info);
            }
        }

        /// <summary>
        /// 上报异常
        /// 上报条件：上报总数 > 30 条, 上报成功率小于 0.7, 且间隔 5 分钟
        /// Native 已经处理数量和成功率判断
        /// </summary>
        /// <param name="code"></param>
        /// <param name="info"></param>
        private void ReportWithCodeAndInfo(int code, string info)
        {
            if (Agent == null) return;
            if (Application.internetReachability == NetworkReachability.NotReachable) return; // 网络不可用时不上报

            ReportAnalyticsAudit(); // 上报
            
            // 源码：https://github.com/castbox/flutter_jigsort/blob/3.2.0V2/lib/data/jigsort_compliance_protocol.dart
            
            var ac = (AnalyticsCode)code;
            Debug.Log($"{LOG_TAG} ------ Get Code And Info: code:{code}[{ac}]  \tinfo:{info}");
            switch (ac)
            {
                case AnalyticsCode.UNITY_INTERNAL_ERROR:        // -1
                    ReportUnityErrorEvent(code, info);
                    break;
                // case AnalyticsCode.DELETE_EXPIRED:
                case AnalyticsCode.UPLOAD_FAIL:                 //14
                    ReportUploadFailEvent(code, info);
                    break;
                // case AnalyticsCode.NETWORK_LOST:
                // case AnalyticsCode.CRONET_INIT_FAIL:
                // case AnalyticsCode.CRONET_INIT_EXCEPTION:
                // case AnalyticsCode.ERROR_API:
                // case AnalyticsCode.ERROR_RESPONSE:
                // case AnalyticsCode.ERROR_CACHE_CONTROL:
                // case AnalyticsCode.ERROR_DELETE_EXPIRED:
                case AnalyticsCode.ERROR_LOAD_MARK:             // 105
                    ReportRuntimeErrorEvent(code, info);
                    break;
                // case AnalyticsCode.ERROR_DNS:
                // case AnalyticsCode.ERROR_ZIP:
                // case AnalyticsCode.ERROR_DNS_CACHE:
                // case AnalyticsCode.CRONET_INTERCEPTOR:
                // case AnalyticsCode.ERROR_SESSION_START_ERROR:
                case AnalyticsCode.EVENT_LOOKUP:                // 1003
                    ReportDNSErrorEvent(code, info);
                    break;
                case AnalyticsCode.EVENT_SESSION_ACTIVE:        // 1004    
                    ReportSessionActiveErrorEvent(code, info);
                    break;
            }

        }
        
        private int _reportUploadFailCount = 0;
        /// <summary>
        /// 上报失败事件 (14)
        /// </summary>
        /// <param name="code"></param>
        /// <param name="info"></param>
        private void ReportUploadFailEvent(int code, string info)
        {
            if (Agent.GetEventCountTotal() < 50) return; // 数量太少不报
            if ((float)Agent.GetEventCountUploaded() / Agent.GetEventCountTotal() > 0.6f) return; // 成功率太高也不报
            if (_reportUploadFailCount >= 5) return; // N 次之后不再上报
            ReportDevAuditEvent(code, info);
            _reportUploadFailCount++;
        }

        private int _reportRuntimeExceptionTimes = 0;
        // 105
        private void ReportRuntimeErrorEvent(int code, string info)
        {
            if (_reportRuntimeExceptionTimes >= 5) return; // N 次之后不再上报
            ReportDevAuditEvent(code, info);
            _reportRuntimeExceptionTimes++;
        }
        
        // 1003
        private void ReportDNSErrorEvent(int code, string info)
        {
            ReportDevAuditEvent(code, info, "ga_dns");
        }
        // 1004
        private void ReportSessionActiveErrorEvent(int code, string info)
        {
            ReportDevAuditEvent(code, info, "session_active");
        }
        // -1
        private void ReportUnityErrorEvent(int code, string info)
        {
            ReportDevAuditEvent(code, info, "unity");
        }

        // 上报 Snapshot 数据
        private void ReportAnalyticsAudit()
        {
            if(DateTime.UtcNow - _lastReportTime < TimeSpan.FromMinutes(5)) // 5 分钟内只上报一次
                return; 
            
            var snapshot = Agent.GetAuditSnapshot();
            if (string.IsNullOrEmpty(snapshot)) return; // 空字段不报
            
            var data = JsonParser.ToObject<Dictionary<string, object>>(snapshot);
            if (data == null) return; // 解析失败不报
            
            // 上报事件
            ReportDevAuditEvent(0, "","analytics_audit", data);
            _lastReportTime = DateTime.UtcNow;
        }

        #endregion

        #region UNIT_TEST
        
#if UNITY_EDITOR

        public static void TestOnCallback(string msg)
        {
            Instance.OnSDKCallback(msg);
        }
#endif

        #endregion
    }
    
    /// <summary>
    /// 网络状态枚举
    /// 详见 guru_analytics 库 guru.core.analytics.handler.AnalyticsCode 类
    /// </summary>
    public enum AnalyticsCode
    {
        UNITY_INTERNAL_ERROR = -1,     // unity 内部错误
        
        DELETE_EXPIRED = 12,            // 删除过期事件
        UPLOAD_FAIL = 14,               // 上报事件失败
        NETWORK_LOST = 22,              // 网络状态不可用
        CRONET_INIT_FAIL = 26,          // 开启Cronet失败
        CRONET_INIT_EXCEPTION = 27,     // 开启Cronet报错
        
        ERROR_API = 101,                // 调用api出错
        ERROR_RESPONSE = 102,           // api返回结果错误
        ERROR_CACHE_CONTROL = 103,      // 设置cacheControl出错
        ERROR_DELETE_EXPIRED = 104,     // 删除过期事件出错
        ERROR_LOAD_MARK = 105,          // 从数据库取事件以及更改事件状态为正在上报出错
        ERROR_DNS = 106,                // dns 错误
        ERROR_ZIP = 107,                // zip 错误
        ERROR_DNS_CACHE = 108,          // zip 错误
        CRONET_INTERCEPTOR = 109,       // cronet拦截器
        ERROR_SESSION_START_ERROR = 110, 
        
        EVENT_LOOKUP = 1003,
        EVENT_SESSION_ACTIVE = 1004,
    }
}

