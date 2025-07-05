
namespace Guru.IAP
{
    using System.Collections.Generic;
    
    public class IAPEvent: ITrackingEvent, IAdjustIapEvent
    {
        public string EventName { get; }
        public Dictionary<string, object> Data { get; }
        public EventSetting Setting { get; }
        public EventPriority Priority { get; }

        private readonly string _currency;
        private readonly double _value;
        private readonly string _productId;
        private readonly string _orderType;
        private readonly string _orderId;
        private readonly string _orderDate;
        private readonly bool _isSandbox;
        private readonly string _productToken;
        private readonly string _receipt;


        public IAPEvent(string eventName, 
            double value,
            string productId,
            string orderId,
            string orderType,
            string orderDate,
            bool isSandbox,
            string productToken = "",
            string receipt = "",
            string currency = "USD")
        {
            EventName = eventName;
            this._value = value;
            this._productId = productId;
            this._orderType = orderType;
            this._orderId = orderId;
            this._orderDate = orderDate;
            this._isSandbox = isSandbox;
            this._productToken = productToken;
            this._receipt = receipt;
            this._currency = currency;
            
            Data = ToDictionary();
            
            Setting = new EventSetting()
            {
                EnableFirebaseAnalytics = true,
                EnableAdjustAnalytics = true,
                EnableGuruAnalytics = true,
                EnableFacebookAnalytics = false,
            };
            
            Priority = (int)EventPriority.Emergence;
        }

        private string GetPlatform()
        {
#if UNITY_IOS
			return "appstore";
#endif
            return "google_play";
        }
        
        
        private Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>()
            {
                ["platform"] = GetPlatform(),
                ["value"] = _value,
                ["currency"] = _currency,
                ["product_id"] = _productId,
                ["order_type"] = _orderType,
                ["order_id"] = _orderId,
                ["trans_ts"] = _orderDate,
                ["sandbox"] = _isSandbox ? "true" : "false",
            };

            // if (!string.IsNullOrEmpty(_productToken))
            // {
            //     dict["purchase_token"] = _productToken;
            // }
            //
            // if (!string.IsNullOrEmpty(_receipt))
            // {
            //     dict["receipt"] = _receipt;
            // }

            return dict;
        }


        /// <summary>
        /// 转为 AdjustIap 事件
        /// </summary>
        /// <returns></returns>
        public AdjustIapEvent ToAdjustIapEvent()
        {
            return new AdjustIapEvent(EventName,
                _value,
                _productId,
                _orderDate,
                _orderId,
                _orderType,
                GetPlatform(),
                _productToken,
                _receipt,
                _currency,_isSandbox? "true": "false");
            
        }
    }
}