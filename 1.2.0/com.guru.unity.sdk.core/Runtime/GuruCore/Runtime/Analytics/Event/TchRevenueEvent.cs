
namespace Guru
{
    using System.Collections.Generic;

    /// <summary>
    /// tch 收入事件
    /// </summary>
    public class TchRevenueEvent: ITrackingEvent
    {
        public string EventName { get; }
        public Dictionary<string, object> Data { get; }
        public EventSetting Setting { get; }
        public EventPriority Priority { get; }
        
        public TchRevenueEvent(string eventName, string platform, double value, string productId = "", 
            string orderId = "", string orderType = "", string timestamp = "", 
            string sandbox = "", string currency = "USD")
        {
            EventName = eventName;
            Priority = EventPriority.Emergence;
            Setting = EventSetting.FirebaseAndGuru();// 只有 Firebase 和 Guru
            Data = new Dictionary<string, dynamic>()
            {
                { Analytics.ParameterAdPlatform, platform },
                { Analytics.ParameterValue, value },
                { Analytics.ParameterCurrency, currency },
            };
            
            //--------- Extra data for IAP receipt ---------------
            if(!string.IsNullOrEmpty(orderType)) Data["order_type"] = orderType;
            if(!string.IsNullOrEmpty(productId)) Data["product_id"] = productId;
            if(!string.IsNullOrEmpty(orderId)) Data["order_id"] = orderId;
            if(!string.IsNullOrEmpty(timestamp)) Data["trans_ts"] = timestamp;
            if(!string.IsNullOrEmpty(sandbox)) Data["sandbox"] = sandbox;
            //--------- Extra data for IAP receipt ---------------
        }
    }
}