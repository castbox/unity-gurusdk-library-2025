
namespace Guru
{
    using System.Linq;
    using System;
    using System.Collections.Generic;
    using Firebase.RemoteConfig;
    using Newtonsoft.Json;
    using UnityEngine;
    
    /// <summary>
    /// ABTEST 管理器
    /// </summary>
    public class ABTestManager : Singleton<ABTestManager>
    {
        public const string Version = "1.0.2";
        private FirebaseRemoteConfig _remoteConfig;
        private List<ABParamData> _params;
        
        
        internal static void LogCrashlytics(string msg) => Analytics.LogCrashlytics(msg);
        
        internal static void LogCrashlytics(Exception ex) => Analytics.LogCrashlytics(ex);

        
        #region 初始化
        
        /// <summary>
        ///  初始化
        /// </summary>
        public static void Init()
        {
            try
            {
                Instance.Setup();
            }
            catch (Exception e)
            {
                LogCrashlytics(e);
                Debug.LogError(e);
            }
            
        }
        
        /// <summary>
        /// 安装服务
        /// </summary>
        private void Setup()
        {
            Debug.Log($"[AB] --- <color=#88ff00>ABTest Init</color>");
            _params = new List<ABParamData>();
            
            _remoteConfig = FirebaseRemoteConfig.DefaultInstance;
            
            Debug.Log($"[AB] --- remoteConfig Counts: {_remoteConfig.Keys.Count()}");
            
            string strValue;
            foreach (var key in _remoteConfig.Keys)
            {
                strValue = _remoteConfig.GetValue(key).StringValue;
                // Debug.Log($"[AB] --- raw config: [{key}] : {strValue}");
                AddParam(strValue);
            }
            
            // ------- ABTest -----------
            // Debug.Log($"<color=orange> --- start parse test string --- </color>");
            // var testStr = @"{""enabled"":true,""value"":2,""id"":""B"",""guru_ab_23100715"":""B""}";
            // AddParam(testStr);
            
            if (_params.Count > 0)
            {
                for (int i = 0; i < _params.Count; i++)
                {
                    // 上报实验AB属性
                    GuruAnalytics.Instance.SetUserProperty(_params[i].id, _params[i].group);
#if UNITY_EDITOR
                    Debug.Log($"[AB] --- Add AB Param <color=cyan>{_params[i].ToString()}</color>");
#else
                    Debug.Log($"[AB] --- Add AB Param {_params[i].ToString()}");
#endif
                }
            }
        
        }
        
        #endregion
        
        #region 添加AB参数

        /// <summary>
        /// 添加AB参数
        /// </summary>
        /// <param name="value"></param>
        private void AddParam(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Contains("guru_ab_"))
            {
                var p = ABParamData.Parse(value);
                if(p != null) _params.Add(p); // 添加参数
            }
        }
        
        #endregion

        #region 单元测试

        public static void TestConfig(string json)
        {
            var p = ABParamData.Parse(json);
            if (p == null)
            {
                Debug.LogError($"Could not parse config: {json}");
                return;
            }

            if (!string.IsNullOrEmpty(p.group))
            {
                Debug.Log($"ID: <color=#88ff00>{p.id}</color>");
                Debug.Log($"Group: <color=#88ff00>{p.group}</color>");
                Debug.Log($"Value: <color=#88ff00>{p.value}</color>");
            }
        }
        
        #endregion
    }
    
    [Serializable]
    internal class ABParamData
    {
        private const int PARAM_NAME_LENGTH = 23; // 从开始"ab_" 计算, 往后20个字符
        
        public string id;
        public string group;
        public string value;
        
        public static ABParamData Parse(string value)
        {
            Debug.Log($"--- ABParamData.Parse: {value}");
            try
            {
                // 发现Guru AB测试标志位
                // var dict = JsonMapper.ToObject<Dictionary<string, JsonData>>(value);
                var dict =  JsonConvert.DeserializeObject<Dictionary<string, object>>(value);
                if (null != dict)
                {
                    foreach (var k in dict.Keys)
                    {
                        if (k.StartsWith("guru_ab"))
                        {
                            return new ABParamData()
                            {
                                id = GetItemKey(k),
                                group = dict[k].ToString(),
                                value = value
                            };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string msg = $"[AB] --- Parse AB Param Error -> Value: {value}\n{e.Message}";
                ABTestManager.LogCrashlytics(msg);
                Debug.Log(msg);
            }
            return null;
        }

        private static string GetItemKey(string raw)
        {
            int ln = "guru_".Length;
            var key = raw.Substring(ln, Mathf.Min(PARAM_NAME_LENGTH, raw.Length - ln)); // 最大长度23
            return key; 
        }

        /// <summary>
        /// 输出字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{id} : {group}";
        }

    }

}