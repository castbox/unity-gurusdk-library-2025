
namespace Guru
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    
    [Serializable]
    public abstract class BaseOrderData
    {
        public string productId; // 商品ID 当orderType=0时，传递该参数
        protected string guid; // 唯一标识
        public int level; // 关卡 ID
        public string userCurrency; // 用户商店货币
        public double payPrice; // 用户支付的费用
        public string scene; // 用户支付的场景
        public bool isFree; // 是否是试用道具
        // ---- Offer Data -------
        public string orderId; // 订单的 OrderID
        public string payedDate; // 支付时间 (13位时间戳)
        public int orderType; // 订单类型，可选值：0:IAP 订单 1:SUB 订阅订单
        public string basePlanId; // 订阅商品的planId
        public string offerId; // 订阅商品的offerId
        
        public Dictionary<string, object> userInfo; // 当前用户信息。目前包含： level: 用户属性中的"b_level"的值
        public EventConfig eventConfig; // 	事件打点所需信息
        
        public BaseOrderData(int orderType, string productId, string orderId, 
            string payedDate, int level, string userCurrency, double payPrice, string scene, bool isFree = false,
            string offerId = "", string basePlanId = "")
        {
            guid = GetGuid();
            this.userCurrency = userCurrency;
            this.payPrice = payPrice;
            this.scene = scene;
            this.isFree = isFree;
            this.orderType = orderType;
            this.productId = productId;
            this.orderId = orderId;
            this.payedDate = payedDate;
            this.basePlanId = basePlanId;
            this.offerId = offerId;
            this.level = level;
            userInfo = new Dictionary<string, object> { ["level"] = level };
            eventConfig = EventConfig.Build();
        }

        protected string GetGuid() => Guid.NewGuid().ToString();

        public abstract string GetProductId();

        public string OrderType() => orderType == 1 ? "SUB": "IAP";
        public string OrderTypeII() => orderType == 1 ? "subscription" : "product";

        public override string ToString()
        {
            return $"{nameof(orderType)}: {orderType}, {nameof(productId)}: {productId}, {nameof(orderId)}: {orderId}, {nameof(payedDate)}: {payedDate}";
        }
        
    }
}