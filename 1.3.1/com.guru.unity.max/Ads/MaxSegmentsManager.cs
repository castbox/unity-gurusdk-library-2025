namespace Guru.Ads.Max
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    
    /// <summary>
    /// Max Segments 管理器
    /// 中台需求详细页： https://www.tapd.cn/33527076/prong/stories/view/1133527076001021649?from_iteration_id=1133527076001002778
    /// </summary>
    public class MaxSegmentsManager
    {
        private const int FB_AD_REVENUE_DAYS_THRESHOLD = 3; // FB广告收益的天数阈值
        
        // 分段键值枚举
        private enum SegmentKey
        {
            Default = 0,
            BuildNumber = 6,
            OSVersion = 7,
            PaidUser = 8,
            LT = 9, // Life Time (用户生命周期)
            Connection = 10,  // Unity 可暂缓实现
            FBAdPreviousRevenue  = 11, // FB 上次产生广告收益的时机
        }
        
        /*
         * 0: none
           1: wifi
           2: mobile
           3: ethernet
           4: bluetooth
           9: wifi-vpn  (0x01 | 0x08)
           10: mobile-vpn (0x02 | 0x08)
           11: ethernet-vpn (0x04 | 0x08)
         */
        private enum ConnectionValue // Unity 可暂缓实现
        {
            None = 0,
            Wifi = 1,
            Mobile = 2,
            Ethernet = 3,
            Bluetooth = 4,
            WifiVpn = 9,
            MobileVpn = 10,  
            EthernetVpn = 11,
        }
        
        // Lifetime 天数分段值
        private readonly List<int> LtDays = new List<int>()
        {
            0, 1, 2, 3, 4, 5, 6, 14, 30, 60, 90, 120, 180
        };
        
        /// <summary>
        /// 构造函数,初始化并上报分组数据
        /// </summary>
        public MaxSegmentsManager(MaxSegmentsProfile profile)
        {
            int buildNumber = GetBuildNumber(profile.buildNumberStr);
            int paidUser = profile.isPaidUser ? 1 : 0;
            int osVersion = GetOSVersionInt(profile.osVersionStr);
            int lt = GetLTValue(profile.firstInstallDate);
            int connection = GetConnectionValue(profile.networkStatus);
            int previousFBAdValue = GetFBAdRevenuePreviousValue(profile.previousFbAdRevenueDate);

            // 生成 Segment 集合
            var collection = MaxSegmentCollection.Builder()
                .AddSegment(GetSegment(SegmentKey.BuildNumber, buildNumber))
                .AddSegment(GetSegment(SegmentKey.PaidUser, paidUser))
                .AddSegment(GetSegment(SegmentKey.OSVersion, osVersion))
                .AddSegment(GetSegment(SegmentKey.LT, lt))
                .AddSegment(GetSegment(SegmentKey.Connection, connection))
                .AddSegment(GetSegment(SegmentKey.FBAdPreviousRevenue, previousFBAdValue)) // 上次 FB 收入数据
                .Build();
            MaxSdk.SetSegmentCollection(collection);
            
            Debug.Log($"[ADS] Max Segments::  BuildNumber: {buildNumber}  PaidUser: {paidUser}  OSVersion: {osVersion}  LT: {lt}  Connection: {connection}");
        }

        /// <summary>
        /// 创建单个分组对象
        /// </summary>
        private MaxSegment GetSegment(SegmentKey key, int value)
        {
            return new MaxSegment((int)key, new List<int>(){value});
        }

        /// <summary>
        /// 获取网络状态值
        /// </summary>
        /// <param name="networkStatus"></param>
        /// <returns></returns>
        private int GetConnectionValue(string networkStatus)
        {
            switch (networkStatus.ToLowerInvariant())
            {
                case "wifi": return (int)ConnectionValue.Wifi;
                case "mobile": return (int)ConnectionValue.Mobile;
                case "wifi-vpn": return (int)ConnectionValue.WifiVpn;
                case "mobile-vpn": return (int)ConnectionValue.MobileVpn;
                case "ethernet": return (int)ConnectionValue.Ethernet;
                case "ethernet-vpn": return (int)ConnectionValue.EthernetVpn;
                case "bluetooth": return (int)ConnectionValue.Bluetooth;
                default: return (int)ConnectionValue.None;
            }
        }
        
        /// <summary>
        /// 解析构建版本号
        /// 从第3位开始取5个数字，例如 24080917 -> 08091
        /// </summary>
        private int GetBuildNumber(string buildNumberStr)
        {
            // 从第三位开始取 5 个数字， 例如 24080917 -> 08091
            var s = buildNumberStr.Substring(2, 5);
            int.TryParse(s, out var value);
            return value;
        }

        /*
         * Android: 10000 + Android Sdk Int 10032 <- 当前 AndroidOS 版本 32
         * IOS: 20000 + IOS Version Int 20172 <- 当前 iOS 系统版本 17.2
         */
        private int GetOSVersionInt(string osVersionStr)
        {
            var s = osVersionStr.Replace(".", "");
            int.TryParse(s, out var value);
#if UNITY_ANDROID
            return 10000 + value;
#elif UNITY_IOS
            return 20000 + value;
#endif
            return value;
        }

        /// <summary>
        /// 获取LT值
        /// </summary>
        /// <returns></returns>
        private int GetLTValue(DateTime firstInstallDate)
        {
            // Debug.Log($"{Tag} Get first install date: {FirstInstallDate}");
            int days = (int)(DateTime.UtcNow - firstInstallDate).TotalDays;
            
            // 匹配上报分段天数
            for (int i = 0; i < LtDays.Count; i++)
            {
                if (days <= LtDays[i])
                {
                    return LtDays[i];
                }
            }
            
            // 超过最大天数， 按最大天数计算
            int lastIndex = LtDays.Count - 1;
            return LtDays[lastIndex];
        }
        
        /// <summary>
        /// 获取 Fb 上次产生收益的设置值
        /// 当 3 天之内有过 FB 广告收益的用户，返回 1
        /// 否则，返回 0
        /// </summary>
        /// <returns></returns>
        private int GetFBAdRevenuePreviousValue(DateTime previousDate)
        {
            try
            {
                var value = (DateTime.UtcNow - previousDate).TotalDays < 3 ? 1 : 0;
                return value;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            return 0;
        }

    }
    
    
    /// <summary>
    /// Max分组初始化参数配置
    /// </summary>
    public class MaxSegmentsProfile
    {
        public string buildNumberStr = string.Empty;      // 构建版本号
        public string osVersionStr = string.Empty;        // 操作系统版本
        public bool isPaidUser = false;            // 是否付费用户
        public DateTime firstInstallDate = new(1970, 1, 1);  // 首次安装日期
        public string networkStatus = string.Empty;       // 网络状态
        public DateTime previousFbAdRevenueDate = new(1970, 1, 1);
    }

}