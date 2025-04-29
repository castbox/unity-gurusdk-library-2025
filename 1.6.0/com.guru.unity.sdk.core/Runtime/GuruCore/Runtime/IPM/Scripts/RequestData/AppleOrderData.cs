namespace Guru
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    
    /// <summary>
    /// 该数据用于向服务端发送对应的消息
    /// 同时兼具缓存用户数据
    /// 详见 <a>https://github.com/castbox/backend-dev/blob/main/saas/%E4%B8%AD%E5%8F%B0%E6%9C%8D%E5%8A%A1%E6%8E%A5%E5%85%A5%E6%89%8B%E5%86%8C.md#553apple-store%E8%AE%A2%E5%8D%95%E4%B8%8A%E6%8A%A5</a>
    /// </summary>
    [Serializable]
    public class AppleOrderData : BaseOrderData
    {
       
        public string bundleId; //应用包名
        public string receipt; // Apple Store返回的Receipt数据
        public string country; // 用户商店的国家2字大写代码
        public string idfv; // ios 的 IDFV
        
        public AppleOrderData(int orderType, string productId, string receipt, string orderId, 
            string date, int level, string userCurrency, double payPrice, string scene, string bundleId, string idfv, bool isFree = false,
            string offerId = "", string basePlanId = "") 
            :base(orderType, productId, orderId, date, level, userCurrency, payPrice, scene, isFree, offerId, basePlanId)
        {
            this.receipt = receipt;
            this.bundleId = bundleId;
            this.idfv = idfv;
            country = IPMConfig.IPM_COUNTRY_CODE;
        }
        
        
        public bool Equals(AppleOrderData other)
        {
            if (string.IsNullOrEmpty(guid)) guid = GetGuid();
            return guid.Equals(other.guid);
        }

        public override string GetProductId() => productId;
        
        public override string ToString()
        {
            return "AppleOrderData: " + base.ToString() + $", {nameof(bundleId)}: {bundleId}, {nameof(idfv)}: {idfv}, {nameof(receipt)}: {receipt}, {nameof(country)}: {country}";
        }

        public string ToJson() => JsonConvert.SerializeObject(this);
    }
}