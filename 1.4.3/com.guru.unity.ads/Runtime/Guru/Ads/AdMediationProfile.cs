namespace Guru.Ads
{
    using System;
    
    /// <summary>
    /// 广告初始化参数
    /// </summary>
    public class AdMediationProfile
    {
        public AdMediationType mediationType = AdMediationType.Max;
        public string bannerUnitId;
        public string interstitialUnitId;
        public string rewardedUnitId;
        public string amazonAppId;
        public string amazonBannerId;
        public string amazonInterstitialId;
        public string amazonRewardedId;
        public string storeUrl;
        public string bannerBgColorHex = "#000000";
        public float bannerWidth= 0;
        public string[] customAdUnitIds = null;
        public string uid;
        public bool isNoAds = false;
        public bool enableAdaptiveBanner = false;
        public bool debugModeEnabled = false;
        //---- For MAX Segments ----
        public string appVersionCode;
        public bool isIapUser = false;
        public string osVersionStr;
        public DateTime firstInstallDate;
        public DateTime previousFBAdRevenueDate;
        public string networkStatus = "none";

        public AdMediationProfile()
        {
        }

        public override string ToString()
        {
            string buff = $"------ AdInitConfig ------\n";
            buff += $"  bannerUnitId: {bannerUnitId}\n";
            buff += $"  interstitialUnitId: {interstitialUnitId}\n";
            buff += $"  rewardedUnitId: {rewardedUnitId}\n";
            buff += $"  amazonAppId: {amazonAppId}\n";
            buff += $"  amazonBannerId: {amazonBannerId}\n";
            buff += $"  amazonInterstitialId: {amazonInterstitialId}\n";
            buff += $"  amazonRewardedId: {amazonRewardedId}\n";
            buff += $"  storeUrl: {storeUrl}\n";
            buff += $"  bannerBackColorHex: {bannerBgColorHex}\n";
            buff += $"  customAdUnitIds: {customAdUnitIds}\n";
            buff += $"  uid: {uid}\n";
            buff += $"  isNoAds: {isNoAds}\n";
            buff += $"  enableAdaptiveBanner: {enableAdaptiveBanner}\n";
            buff += $"  isIapUser: {isIapUser}\n";
            buff += $"  networkStatus: {networkStatus}\n";
            buff += $"  debugModeEnabled: {debugModeEnabled}\n";
            buff += $"------ AdInitConfig ------\n";
            return buff;
        }
    }
    
    /// <summary>
    /// AdInitConfig 构建器
    /// </summary>
    public class AdInitConfigBuilder
    {
        private readonly AdMediationProfile _config = new AdMediationProfile();

        public AdInitConfigBuilder()
        {

        }
        
        public AdInitConfigBuilder SetMediationType(AdMediationType type)
        {
            _config.mediationType = type;
            return this;
        }
        
        public AdInitConfigBuilder SetBannerUnitId(string bannerUnitId)
        {
            _config.bannerUnitId = bannerUnitId;
            return this;
        }
        
        public AdInitConfigBuilder SetBannerWidth(float value)
        {
            _config.bannerWidth = value;
            return this;
        }

        public AdInitConfigBuilder SetInterstitialUnitId(string interstitialUnitId)
        {
            _config.interstitialUnitId = interstitialUnitId;
            return this;
        }

        public AdInitConfigBuilder SetRewardedUnitId(string rewardedUnitId)
        {
            _config.rewardedUnitId = rewardedUnitId;
            return this;
        }
        
        public AdInitConfigBuilder SetAmazonAppId(string amazonAppId)
        {
            _config.amazonAppId = amazonAppId;
            return this;
        }
        
        public AdInitConfigBuilder SetAmazonBannerId(string amazonBannerId)
        {
            _config.amazonBannerId = amazonBannerId;
            return this;
        }

        public AdInitConfigBuilder SetAmazonInterstitialId(string amazonInterstitialId)
        {
            _config.amazonInterstitialId = amazonInterstitialId;
            return this;
        }

        public AdInitConfigBuilder SetAmazonRewardedId(string amazonRewardedId)
        {
            _config.amazonRewardedId = amazonRewardedId;
            return this;
        }

        public AdInitConfigBuilder SetStoreUrl(string storeUrl)
        {
            _config.storeUrl = storeUrl;
            return this;
        }

        public AdInitConfigBuilder SetBannerBackColorHex(string bannerBackColorHex)
        {
            _config.bannerBgColorHex = bannerBackColorHex;
            return this;
        }
        
        public AdInitConfigBuilder SetCustomAdUnitIds(string[] customAdUnitIds)
        {
            _config.customAdUnitIds = customAdUnitIds;
            return this;
        }
        public AdInitConfigBuilder SetUserId(string uid)
        {
            if(!string.IsNullOrEmpty(uid)) _config.uid = uid;
            return this;
        }
        public AdInitConfigBuilder SetIsNoAds(bool isNoAds)
        {
            _config.isNoAds = isNoAds;
            return this;
        }
        public AdInitConfigBuilder SetDebugModeEnabled(bool debugModeEnabled)
        {
            _config.debugModeEnabled = debugModeEnabled;
            return this;
        }
        
        public AdInitConfigBuilder SetIsIapUser(bool isIapUser)
        {
            _config.isIapUser = isIapUser;
            return this;
        }
        public AdInitConfigBuilder SetNetworkStatus(string networkStatus)
        {
            _config.networkStatus = networkStatus;
            return this;
        }
        public AdInitConfigBuilder SetVersionCodeStr(string versionCodeStr)
        {
            _config.appVersionCode = versionCodeStr;
            return this;
        }
        public AdInitConfigBuilder SetOSVersionStr(string osVersionStr)
        {
            _config.osVersionStr = osVersionStr;
            return this;
        }   
        public AdInitConfigBuilder SetFirstInstallDate(DateTime firstInstallDate)
        {
            _config.firstInstallDate = firstInstallDate;
            return this;
        }
        public AdInitConfigBuilder SetPreviousFBAdRevenueDate(DateTime previousDate)
        {
            _config.previousFBAdRevenueDate = previousDate;
            return this;
        }
        public AdInitConfigBuilder SetEnableAdaptiveBanner(bool enableAdaptiveBanner)
        {
            _config.enableAdaptiveBanner = enableAdaptiveBanner;
            return this;
        }
        
    

        public AdMediationProfile Build()
        {
            return _config;
        }
    }
    



}