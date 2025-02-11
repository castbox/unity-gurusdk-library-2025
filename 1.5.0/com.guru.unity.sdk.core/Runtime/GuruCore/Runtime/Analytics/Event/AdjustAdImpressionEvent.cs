

namespace Guru
{
    using System.Collections.Generic;
    
    /// <summary>
    /// 广告通用事件买点
    /// 参数详解：https://docs.google.com/spreadsheets/d/1OrUxmCgahps4PDPxLztBTBeEGvNllFMAjSWA22ofR2M/edit?gid=0#gid=0
    /// </summary>
    public class AdjustAdImpressionEvent
    {
        public string eventName;
        public double value;
        public string currency; // 默认写死 USD
        public string adPlatform; // "使用MAX广告平台：MAX    使用ADMOB广告平台：ADMOB"
        public string adSource;
        public string adFormat;
        public string adUnitId;  
        public string adPlacement; //  广告网络创建用于配到MAX的ID，网络生成的ID，（biddingid /瀑布流id）
        public string adCreativeId; // max 专用
        
        public AdjustAdImpressionEvent(string eventName, 
            double value, 
            string adPlatform,
            string adSource,
            string adFormat,
            string adUnitId,
            string adPlacement,
            string adCreativeId = "",
            string currency = "")
        {
            this.eventName = eventName;
            this.value = value;
            this.adPlatform = adPlatform;
            this.currency = currency;
            this.adSource = adSource;
            this.adFormat = adFormat;
            this.adUnitId = adUnitId;
            this.adPlacement = adPlacement;
            this.adCreativeId = adCreativeId;

            if (string.IsNullOrEmpty(currency))
                currency = "USD";
        }
        
        public override string ToString()
        {
            return $"eventName: {eventName}\n" +
                   $"value: {value}\n" +
                   $"currency: {currency}\n" +
                   $"adPlatform: {adPlatform}\n" +
                   $"adSource: {adSource}\n" +
                   $"adFormat: {adFormat}\n" +
                   $"adUnitId: {adUnitId}\n" +
                   $"adPlacement: {adPlacement}\n" +
                   $"adCreativeId: {adCreativeId}\n";
        }
        
        
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>()
            {
                ["value"] = value,
                ["currency"] = currency,
                ["adSource"] = adSource,
                ["adUnitId"] = adUnitId,
                ["adPlacement"] = adPlacement,
                ["adPlatform"] = adPlatform,
                ["adFormat"] = adFormat,
                ["adCreativeId"] = adCreativeId,
            };
        }
        
    }

}