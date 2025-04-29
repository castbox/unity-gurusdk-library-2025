

namespace Guru
{
    
    /// <summary>
    /// CDN 数据代理
    /// </summary>
    public class GuruCdnLoaderDataProvider: ICdnLoaderDataProvider
    {
        const string TAG = "[CdnLoader-DP}";
        const string DEFAULT_CDNCONFIG_KEY = "cdn_config";
        
        
        private readonly CDNConfig _defaultConfig;
        private CDNConfig _onlineConfig;
        private readonly string _configKey;

        /// <summary>
        /// GuruSDK 实现的CDNLoader全局数据提供器
        /// </summary>
        /// <param name="defaultConfigValue"></param>
        /// <param name="remoteKey"></param>
        public GuruCdnLoaderDataProvider(string defaultConfigValue, string remoteKey = "")
        {
            if(string.IsNullOrEmpty(remoteKey))
                _configKey = DEFAULT_CDNCONFIG_KEY;

            _defaultConfig = GetLoaderFromJson(defaultConfigValue);
            if (_defaultConfig == null)
            {
                UnityEngine.Debug.LogError($"{TAG} defaultCdnConfig is null!!");
            }
            
            _onlineConfig = UpdateOnlineConfig(); // 二次加载尝试获取线上值
        }

        private CDNConfig GetLoaderFromJson(string json)
        {
            if(string.IsNullOrEmpty(json))
                return null;
            
            return JsonParser.ToObject<CDNConfig>(json);
        }

        /// <summary>
        /// 更新线上数据
        /// </summary>
        /// <returns></returns>
        private CDNConfig UpdateOnlineConfig()
        {
            return GetLoaderFromJson(GuruSDK.GetRemoteString(_configKey));
        }


        /// <summary>
        /// 云控初始化就绪
        /// </summary>
        public void OnRemoteFetchSuccess()
        {
            _onlineConfig = UpdateOnlineConfig();
        }
        
        public CDNConfig GetDefaultConfig()
        {
            return _defaultConfig;
        }

        public CDNConfig GetOnlineConfig()
        {
            return _onlineConfig;
        }
    }
}