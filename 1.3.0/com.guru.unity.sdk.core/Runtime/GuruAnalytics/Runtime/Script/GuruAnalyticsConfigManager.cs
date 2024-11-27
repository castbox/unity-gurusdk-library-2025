namespace Guru
{
    using System;
    using UnityEngine;
    using Random = UnityEngine.Random;
    using Firebase.RemoteConfig;
    using System.Linq;

    public class GuruAnalyticsConfigManager
    {

        private const string Tag = "[SDK][ANU][EXP]";

        private static bool IsDebug
        {
            get
            {
#if UNITY_EDITOR || DEBUG
                return true;
#endif
                return false;
            }
        }


        private static string _localExperimentGroupId = "";
        private static string LocalExperimentGroupId
        {
            get
            {
                if (string.IsNullOrEmpty(_localExperimentGroupId))
                {
                    _localExperimentGroupId = PlayerPrefs.GetString(nameof(LocalExperimentGroupId), "");
                }
                return _localExperimentGroupId;
            }
            set
            {
                _localExperimentGroupId = value;
                PlayerPrefs.SetString(nameof(LocalExperimentGroupId), value);
                PlayerPrefs.Save();
            }
        }

        /**
         * 原始数据
        private const string JSON_GROUP_B =
            "{\"cap\":\"firebase|facebook|guru\",\"init_delay_s\":10,\"experiment\":\"B\",\"guru_upload_ip_address\":[\"13.248.248.135\", \"3.33.195.44\"]}";

        private const string JSON_GROUP_C =
            "{\"cap\":\"firebase|facebook|guru\",\"init_delay_s\":10,\"experiment\":\"C\",\"guru_upload_ip_address\":[\"34.107.185.54\"],\"guru_event_url\":\"https://collect3.saas.castbox.fm\"}";
        **/

        /// <summary>
        /// 解析 JSON 字符串
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private static GuruAnalyticsExperimentData Parse(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            return JsonParser.ToObject<GuruAnalyticsExperimentData>(json);
        }

        /// <summary>
        /// 云控数据参数
        /// </summary>
        public const string KEY_GURU_ANALYTICS_EXP = "guru_analytics_exp";
        
        /// <summary>
        /// 默认的本地配置
        /// 2024-08-08 经后台确认，只保留 B 组，C 组已经被废弃 
        /// </summary>
        private const string DEFAULT_GURU_ANALYTICS_EXP = @"{
	""enable"": true,
	""experiments"": [{
		""groupId"": ""B"",
		""baseUrl"": ""https://collect.saas.castbox.fm"",
		""uploadIpAddress"": [""13.248.248.135"", ""3.33.195.44""],
        ""enableErrorLog"": false
	}]
}";
        
        /// <summary>
        /// 默认的本地配置
        /// 2024-08-06
        /// </summary>
        private const string DEFAULT_GURU_ANALYTICS_EXP2 = @"{
	""enable"": true,
	""experiments"": [{
		""groupId"": ""B"",
		""baseUrl"": ""https://collect.saas.castbox.fm"",
		""uploadIpAddress"": [""13.248.248.135"", ""3.33.195.44""],
        ""enableErrorLog"": false
	},
    {
		""groupId"": ""C"",
		""baseUrl"": ""https://collect3.saas.castbox.fm"",
		""uploadIpAddress"": [""34.107.185.54""],
        ""enableErrorLog"": false
	}]
}";
        
        /// <summary>
        /// 获取默认数据
        /// </summary>
        private static GuruAnalyticsExperimentData DefaultData => Parse(DEFAULT_GURU_ANALYTICS_EXP);


        /// <summary>
        /// 在当前版本中，随机获取线上配置的值
        /// 若无法获取线上配置，则默认是 B 分组
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="baseUrl"></param>
        /// <param name="uploadIpAddress"></param>
        /// <param name="isEnable"></param>
        internal static GuruAnalyticsExperimentConfig GetInitParams()
        {
            GuruAnalyticsExperimentConfig config;
            
            if(IsDebug) Debug.LogWarning($"{Tag} --- #0 Analytics EXP saved groupId :{LocalExperimentGroupId}");
            
            // 拉取云控数据
            var json = "";
            if(FirebaseUtil.IsFirebaseInitialized 
               && FirebaseRemoteConfig.DefaultInstance.Keys.Contains(KEY_GURU_ANALYTICS_EXP))   
                json = 	FirebaseRemoteConfig.DefaultInstance.GetValue(GuruAnalyticsConfigManager.KEY_GURU_ANALYTICS_EXP).StringValue;
			
            if (string.IsNullOrEmpty(json))
            {
                // 没有云控值，走本地的数据配置,随机取值
                if(IsDebug) Debug.LogWarning($"{Tag} --- #1 Analytics EXP json is Null -> using DefaultData");
                config = GetDefaultGuruAnalyticsExpConfig();
            }
            else
            {
                // 有云控值，则直接使用云控的数据
                if(IsDebug) Debug.LogWarning($"{Tag} --- #2 Analytics EXP Try to get remote json -> {json}");
                var expData = Parse(json);
                if (expData == null)
                {
                    // 如果云控值为空，则使用本地分组
                    if(IsDebug) Debug.LogWarning($"{Tag} --- #2.1 Analytics EXP Parse failed -> using DefaultData");
                    config = GetDefaultGuruAnalyticsExpConfig();
                }
                else
                {
                    // 如果云控值不为空，但不可用，则直接使用默认分组
                    if (!expData.enable)
                    {
                        Debug.LogWarning($"{Tag} --- #2.2 Analytics EXP Disabled -> using DefaultData");
                        expData = DefaultData;
                    }
                    config = expData.GetFirstConfig();
                }
            }

            // 最后取不到的话只能默认分组了
            if (config == null) {
                config = DefaultData.GetFirstConfig(); // 默认是 B 组
                if(IsDebug) Debug.LogWarning($"{Tag} --- #3.1 Try get config is Null -> using Default config");
            }
            
            LocalExperimentGroupId = config.groupId;
            return config;
        }
        
        private static GuruAnalyticsExperimentConfig GetDefaultGuruAnalyticsExpConfig()
        {
            GuruAnalyticsExperimentConfig config = null; 
            if (!string.IsNullOrEmpty(LocalExperimentGroupId))
            {
                config = DefaultData.GetConfig(LocalExperimentGroupId); // 非空则取值
            }
            else
            {
                config = DefaultData.GetRandomConfig();  // 随机获取本地的 Config
            }
            if(IsDebug) Debug.LogWarning($"{Tag} --- #1.1 using Default GroupId: {config.groupId}");
            return config;
        }
    }
    

    /// <summary>
    /// 实验数据主题
    /// </summary>
    [Serializable]
    internal class GuruAnalyticsExperimentData
    {
        public readonly bool enable = true; // 默认是打开的状态
        public GuruAnalyticsExperimentConfig[] experiments; // 实验列表
        public string ToJson() => JsonParser.ToJson(this); // 转换成 JSON 字符串 
        
        /// <summary>
        /// 获取随机分组
        /// </summary>
        /// <returns></returns>
        public GuruAnalyticsExperimentConfig GetRandomConfig()
        {
            if (experiments == null || experiments.Length == 0) return null;
            return experiments[Random.Range(0, experiments.Length)];
        }
        
        /// <summary>
        /// 根据分组名称获取分组
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public GuruAnalyticsExperimentConfig GetConfig(string groupId)
        {
            foreach (var g in experiments)
            {
                if (g.groupId == groupId) return g;
            }

            return null;
        }

        /// <summary>
        /// 获取首个配置
        /// </summary>
        /// <returns></returns>
        public GuruAnalyticsExperimentConfig GetFirstConfig()
        {
            if (experiments != null && experiments.Length > 0) return experiments[0];
            return null;
        }


        /// <summary>
        /// 分组是否存在
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public bool IsGroupExists(string groupId)
        {
            foreach (var g in experiments)
            {
                if (g.groupId == groupId) return true;
            }
            return false;
        }

    }
    
    /// <summary>
    /// 实验配置
    /// </summary>
    [Serializable]
    internal class GuruAnalyticsExperimentConfig
    {
        public string groupId;
        public string baseUrl;
        public string[] uploadIpAddress;
        public bool enableErrorLog;
        
        public override string ToString()
        {
            return JsonParser.ToJson(this);
        }
    }
}