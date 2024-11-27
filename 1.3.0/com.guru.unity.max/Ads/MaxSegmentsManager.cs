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
        private enum SegmentKey
        {
            Default = 0,
            BuildNumber = 6,
            OSVersion = 7,
            PaidUser = 8,
            LT = 9,
            Connection = 10,  // Unity 可暂缓实现
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
        
        private readonly List<int> LtDays = new List<int>()
        {
            0, 1, 2, 3, 4, 5, 6, 14, 30, 60, 90, 120, 180
        };
        
        /// <summary>
        /// 生成 Segment 函数
        /// </summary>
        public MaxSegmentsManager(MaxSegmentsProfile profile)
        {
            int buildNumber = GetBuildNumber(profile.buildNumberStr);
            int paidUser = profile.isPaidUser ? 1 : 0;
            int osVersion = GetOSVersionInt(profile.osVersionStr);
            int lt = GetLTValue(profile.firstInstallDate);
            int connection = GetConnectionValue(profile.networkStatus);

            // 生成 Segment 集合
            var collection = MaxSegmentCollection.Builder()
                .AddSegment(GetSegment(SegmentKey.BuildNumber, buildNumber))
                .AddSegment(GetSegment(SegmentKey.PaidUser, paidUser))
                .AddSegment(GetSegment(SegmentKey.OSVersion, osVersion))
                .AddSegment(GetSegment(SegmentKey.LT, lt))
                .AddSegment(GetSegment(SegmentKey.Connection, connection))
                .Build();
            MaxSdk.SetSegmentCollection(collection);
            
            Debug.Log($"[ADS] Max Segments::  BuildNumber: {buildNumber}  PaidUser: {paidUser}  OSVersion: {osVersion}  LT: {lt}  Connection: {connection}");
        }


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
            switch (networkStatus)
            {
                case "wifi":
                    return (int)ConnectionValue.Wifi;
                case "mobile":
                    return (int)ConnectionValue.Mobile;
                case "wifi-vpn":
                    return (int)ConnectionValue.WifiVpn;
                case "mobile-vpn":
                    return (int)ConnectionValue.MobileVpn;
                case "ethernet":
                    return (int)ConnectionValue.Ethernet;
                case "ethernet-vpn":
                    return (int)ConnectionValue.EthernetVpn;
                case "bluetooth":
                    return (int)ConnectionValue.Bluetooth;
                default:
                    return (int)ConnectionValue.None;
            }
        }
        
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
    }

    /// <summary>
    /// 初始化参数
    /// </summary>
    public class MaxSegmentsProfile
    {
        public string buildNumberStr;
        public string osVersionStr;
        public bool isPaidUser;
        public DateTime firstInstallDate;
        public string networkStatus;
    }

}