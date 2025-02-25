
namespace Guru
{
    using System.Collections.Generic;
    
    public class AdjustIapEvent
    {
        public readonly string eventName;
        public readonly double value;
        public readonly string currency; // 默认写死 USD
        public readonly string platform; // Android: google_play, iOS: appstore
        public readonly string productId;
        public readonly string orderId;
        public readonly string orderType;  // 订阅：SUB， 付费：IAP 
        public readonly string transTs; // timestamp 13位时间戳
        public readonly string purchaseToken; // Android purchaseToken
        public readonly string receipt; // iOS receipt

        public AdjustIapEvent(string eventName, 
            double value, 
            string productId,  
            string transTs, 
            string orderId,
            string orderType, 
            string platform = "", 
            string purchaseToken = "",
            string receipt = "",
            string currency = "")
        {
            this.eventName = eventName;
            this.value = value;
            this.productId = productId;
            this.transTs = transTs;
            this.orderId = orderId;
            this.orderType = orderType;
            this.currency = currency;
            this.platform = platform;
            this.purchaseToken = purchaseToken;
            this.receipt = receipt;

            if (string.IsNullOrEmpty(this.currency))
            {
                this.currency = "USD";
            }
            
            if (string.IsNullOrEmpty(this.orderType))
            {
                this.orderType = "IAP";
            }

            if (string.IsNullOrEmpty(this.platform))
            {
                this.platform = "google_play";
#if UNITY_IOS
                this.platform = "appstore";   
#endif
            }
            
        }
        
        /// <summary>
        /// 转化为字典
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>
            {
                ["platform"] = platform,
                ["value"] = value,
                ["currency"] = currency,
                ["product_id"] = productId,
                ["order_type"] = orderType,
                ["order_id"] = orderId,
                ["trans_ts"] = transTs,
                ["receipt"] = receipt,
                ["purchase_token"] = purchaseToken,
            };
            return dict;
        }
        
        public override string ToString()
        {
            return $"eventName: {eventName}\n" +
                   $"value: {value}\n" +
                   $"currency: {currency}\n" +
                   $"platform: {platform}\n" +
                   $"productId: {productId}\n" +
                   $"orderId: {orderId}\n" +
                   $"orderType: {orderType}\n" +
                   $"transTs: {transTs}\n" +
                   $"purchaseToken: {purchaseToken}\n" +
                   $"receipt: {receipt}\n";
        }
    }
}