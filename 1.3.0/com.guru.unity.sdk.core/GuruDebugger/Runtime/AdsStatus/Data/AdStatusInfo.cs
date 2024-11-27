

namespace Guru
{
    using System;
    
    
    /// <summary>
    /// 广告类型
    /// </summary>
    public enum AdType
    {
        Banner,
        Interstitial,
        Rewarded,
    }

    /// <summary>
    /// 广告状态枚举
    /// </summary>
    public enum AdStatusType
    {
        NotReady,
        Loading,
        Loaded,
        Failed,
        Closed,
        Paid,
        Clicked
    }


    /// <summary>
    /// 广告状态信息
    /// </summary>
    public class AdStatusInfo
    {
        public string adUnitId;
        public string placement;
        public AdType adType;
        public AdStatusType status = AdStatusType.NotReady;
        public int errorCode;
        public DateTime date;
        public string network;
        public string networkPlacement;
        public double revenue;
        public string waterfall;


        public string GetDate() => date.ToString("yy-MM-dd HH:mm:ss");

        public string ToLogString()
        {
            return $"[{GetDate()}] {adType}:{status}\tid:{adUnitId}\tnetwork{network}\trevenue:{revenue}\twaterfall:{waterfall}\terrorCode:{errorCode}";
        }

    }
}