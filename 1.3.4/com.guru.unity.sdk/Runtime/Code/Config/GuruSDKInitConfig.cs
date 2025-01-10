
namespace Guru
{
    using System.Collections.Generic;
    using System.Text;
    using System;
    
    /// <summary>
    /// 启动参数配置
    /// </summary>
    public class GuruSDKInitConfig
    {
        #region Properties

        /// <summary>
        /// 使用自定义的ConsentFlow启动流程
        /// </summary>
        public bool UseCustomConsent = false;
        /// <summary>
        /// SDK初始化完成后自动加载广告
        /// </summary>
        public bool AutoLoadWhenAdsReady = true;
        /// <summary>
        /// 使用IAP支付插件功能
        /// </summary>
        public bool IAPEnabled = true;
        /// <summary>
        /// 自动申请推送授权信息
        /// </summary>
        public bool AutoNotificationPermission = true;
        /// <summary>
        /// 自动记录完成的关卡
        /// </summary>
        [Obsolete("Will be removed from InitConfig in next version. Use the <b_level> and <b_play> data from the GameUserData from game itself instead!")]
        public bool AutoRecordFinishedLevels = false;
        /// <summary>
        /// 自定义 Service 云控 Key
        /// </summary>
        public string CustomServiceKey = "";
        /// <summary>
        /// Banner 背景颜色 Hex 值
        /// </summary>
        public string BannerBgColor = "#000000";
        /// <summary>
        /// 设置 banner 的宽
        /// </summary>
        public float BannerWidth = 0;
        /// <summary>
        /// 已购买去广告道具
        /// </summary>
        public bool IsBuyNoAds = false;
        /// <summary>
        /// Debug模式（默认关闭）
        /// </summary>
        public bool DebugMode = false;
        /// <summary>
        /// Debug模式下开启打点（默认关闭）
        /// </summary>
        public bool LogEnabledInDebugMode = false;
        /// <summary>
        /// Adjust 延迟打点策略（默认关闭）
        /// </summary>
        public bool AdjustDeferredReportAdRevenueEnabled = false;
        /// <summary>
        /// iOS ATT 延迟上报时间
        /// </summary>
        public int AdjustIOSAttWaitingTime = 0;

        private Dictionary<string, object> _defaultRemoteData = new Dictionary<string, object>();
        /// <summary>
        /// 云控参数的默认配置
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> DefaultRemoteData
        {
            set
            {
                if (value != null)
                {
                    _defaultRemoteData = value;
                }
            }
            get => _defaultRemoteData;
        }

        /// <summary>
        /// 启用 AdjustDeeplink
        /// </summary>
        public Action<string> OnAdjustDeeplinkCallback = null;
        
        /// <summary>
        /// 支付初始化Keys
        /// </summary>
        public byte[] GoogleKeys;       // 数据取自 GooglePlayTangle.Data();
        public byte[] AppleRootCerts;   // 数据取自 AppleTangle.Data();

        #endregion
        
        #region Print

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"------- Custom InitConfig -------");
            sb.AppendLine($"\t  UseCustomConsent: {UseCustomConsent}");
            sb.AppendLine($"\t  AutoLoadWhenAdsReady: {AutoLoadWhenAdsReady}");
            sb.AppendLine($"\t  IAPEnabled: {IAPEnabled}");
            sb.AppendLine($"\t  AutoNotificationPermission: {AutoNotificationPermission}");
            // sb.AppendLine($"\t  AutoRecordFinishedLevels: {AutoRecordFinishedLevels}");
            sb.AppendLine($"\t  CustomServiceKey: {CustomServiceKey}");
            sb.AppendLine($"\t  BannerBgColor: {BannerBgColor}");
            sb.AppendLine($"\t  BannerWidth: {BannerWidth}");
            sb.AppendLine($"\t  IsBuyNoAds: {IsBuyNoAds}");
            sb.AppendLine($"\t  DebugMode: {DebugMode}");
            sb.AppendLine($"\t  DefaultRemote: Count: {DefaultRemoteData.Count}");
            sb.AppendLine($"------- Custom InitConfig -------");
            return sb.ToString();
        }



        #endregion

        #region Builder

        /// <summary>
        /// 构造器
        /// </summary>
        /// <returns></returns>
        public static GuruSDKInitConfigBuilder Builder() => new GuruSDKInitConfigBuilder();

        #endregion
    }
    
    /// <summary>
    /// 构建器
    /// </summary>
    public class GuruSDKInitConfigBuilder
    {
        private readonly GuruSDKInitConfig _config = new GuruSDKInitConfig();
        
        /// <summary>
        /// 构建配置
        /// </summary>
        /// <returns></returns>
        public GuruSDKInitConfig Build()
        {
            return _config;
        }

        public GuruSDKInitConfigBuilder SetUseCustomConsent(bool value)
        {
            _config.UseCustomConsent = value;
            return this;
        }
        public GuruSDKInitConfigBuilder SetAutoLoadWhenAdsReady(bool value)
        {
            _config.AutoLoadWhenAdsReady = value;
            return this;
        }
        public GuruSDKInitConfigBuilder SetIAPEnabled(bool value)
        {
            _config.IAPEnabled = value;
            return this;
        }
        // public GuruSDKInitConfigBuilder SetAutoRecordFinishedLevels(bool value)
        // {
        //     _config.AutoRecordFinishedLevels = value;
        //     return this;
        // }
        public GuruSDKInitConfigBuilder SetIsBuyNoAds(bool value)
        {
            _config.IsBuyNoAds = value;
            return this;
        }
        public GuruSDKInitConfigBuilder SetBannerBgColor(string value)
        {
            _config.BannerBgColor = value;
            return this;
        }
        public GuruSDKInitConfigBuilder SetBannerWidth(float value)
        {
            _config.BannerWidth = value;
            return this;
        }
        public GuruSDKInitConfigBuilder SetDebugMode(bool value)
        {
            _config.DebugMode = value;
            return this;
        }
        public GuruSDKInitConfigBuilder SetOnAdjustDeeplinkCallback(Action<string> callback)
        {
            _config.OnAdjustDeeplinkCallback = callback;
            return this;
        }
        public GuruSDKInitConfigBuilder SetGoogleKeys(byte[] value)
        {
            _config.GoogleKeys = value;
            return this;
        }
        public GuruSDKInitConfigBuilder SetAppleRootCerts(byte[]  value)
        {
            _config.AppleRootCerts = value;
            return this;
        }
        public GuruSDKInitConfigBuilder SetDefaultRemoteData(Dictionary<string, object> value)
        {
            _config.DefaultRemoteData = value;
            return this;
        }
        public GuruSDKInitConfigBuilder SetEnableDebugLogEvent(bool value)
        {
            _config.LogEnabledInDebugMode = value;
            return this;
        }
        public GuruSDKInitConfigBuilder SetCustomServiceKey(string value)
        {
            _config.CustomServiceKey = value;
            return this;
        }
        public GuruSDKInitConfigBuilder SetAutoNotificationPermission(bool value)
        {
            _config.AutoNotificationPermission = value;
            return this;
        }
        public GuruSDKInitConfigBuilder SetAdjustDeferredReportAdRevenueEnabled(bool value)
        {
            _config.AdjustDeferredReportAdRevenueEnabled = value;
            return this;
        }
        public GuruSDKInitConfigBuilder SetAdjustIOSAttWaitingTime(int value)
        {
            _config.AdjustIOSAttWaitingTime = value;
            return this;
        }
        
    }
}