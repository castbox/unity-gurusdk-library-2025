
namespace Guru
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    
    /// <summary>
    /// 该数据用于向服务端发送对应的消息
    /// 同时兼具缓存用户数据
    /// 详见 <a>https://github.com/castbox/backend-dev/blob/main/saas/%E4%B8%AD%E5%8F%B0%E6%9C%8D%E5%8A%A1%E6%8E%A5%E5%85%A5%E6%89%8B%E5%86%8C.md#552google%E8%AE%A2%E5%8D%95%E4%B8%8A%E6%8A%A5</a>
    /// </summary>
    [Serializable]
    public class GoogleOrderData : BaseOrderData
    {
        private string _productId;
        public string subscriptionId; // 订阅道具名称
        public string packageName; //应用包名 
        public string token;  // 应用商店里面的购买token
        
        public GoogleOrderData(int orderType, string productId, string token, 
            string orderId, string date, int level, 
            string userCurrency, double payPrice, string scene, bool isFree = false,
            string offerId = "", string basePlanId = "")
            :base(orderType, productId, orderId, date, level, userCurrency, payPrice, scene, isFree, offerId, basePlanId)
        {
            _productId = productId;
            this.packageName = GuruSettings.Instance.GameIdentifier;
            this.token = token;
            if (orderType == 1)
            {
                this.subscriptionId = productId;
                this.productId = "";
            }
        }
        
        public bool Equals(GoogleOrderData other)
        {
            if (string.IsNullOrEmpty(guid)) guid = Guid.NewGuid().ToString();
            return guid == other.guid;
        }
        
        public string ToJson() => JsonConvert.SerializeObject(this);

        public override string GetProductId() => _productId;

        public override string ToString()
        {
            return "GoogleOrderData: " + base.ToString() + $",{nameof(subscriptionId)}: {subscriptionId}, {nameof(packageName)}: {packageName}, {nameof(token)}: {token}";
        }
    }
}