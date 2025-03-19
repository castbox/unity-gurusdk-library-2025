
namespace Guru
{
    using System;
    using UnityEngine;
    using UnityEngine.Purchasing;

    /// <summary>
    /// 商品配置类
    /// </summary>
    [Serializable]
    public partial class ProductSetting
    {
        /// <summary>
        /// 商品名称
        /// </summary>
        [Header("商品名称")] [Tooltip("[必填] 程序调用时, 统一使用 ProductName 来进行购买操作")]
        public string ProductName;
        /// <summary>
        /// 商品类型
        /// </summary>
        [Header("商品类型")] [Tooltip("[必填] 产品类型: 不可消耗, 可消耗, 订阅")]
        public ProductType Type;
        /// <summary>
        /// GooglePlay 商品ID
        /// </summary>
        [Header("Google商品ID")] 
        public string GooglePlayProductId;
        /// <summary>
        /// AppleStore 商品ID
        /// </summary>
        [Header("Apple商品ID")] 
        public string AppStoreProductId;

        [Header("自定义商品分类")][Tooltip("自定义标签:用于产品自有逻辑进行分类查找")]
        public string Category = "Store";
        
        [Header("是否免费(非必须)")]
        public bool IsFree = false;

        [Header("预设商品价格($)")]
        public double Price;
        
        /// <summary>
        /// 商品ID
        /// </summary>
        public string ProductId
        {
            get
            {
#if UNITY_IOS
                return AppStoreProductId;
#else
                return GooglePlayProductId;
#endif
            }
        }
    }
    
    /// <summary>
    /// 商品信息
    /// </summary>
    [Serializable]
    public partial class ProductInfo
    {
        private Product _product;
        private ProductSetting _setting;
        public ProductSetting Setting => _setting;
        public Product Product => _product;
        
        public ProductInfo(ProductSetting setting)
        {
            _setting = setting;
        }
        
        public void SetProduct(Product product) => _product = product;
        public string Name => _setting.ProductName;
        public string Id => _product?.definition?.id ?? _setting.ProductId;
        public double Price => (double?)_product?.metadata?.localizedPrice ?? _setting.Price;
        public string CurrencyCode => _product?.metadata?.isoCurrencyCode ?? "$";
        public string Category => _setting.Category;
        public string Type => _setting.Type == ProductType.Subscription ? "subscription" : "product";
        public bool IsFree => _setting.IsFree;
        public string LocalizedPriceString /*=> _product?.metadata?.localizedPriceString ?? $"{CurrencyCode}{_setting.Price}"*/{
            get
            {
                if (_product == null || _product.metadata == null)
                {
                    Debug.Log($"[IAP] 获取默认价格！");
                    return $"{CurrencyCode}{_setting.Price}";
                }
                Debug.Log($"[IAP] 从商店获取价格；CurrencyCode =  {CurrencyCode}; priceStr = {(_product.metadata.localizedPriceString)}");
#if UNITY_IOS
                return $"{CurrencyCode}{_product.metadata.localizedPriceString}";
#else
                return _product.metadata.localizedPriceString
#endif
            }
            // _product?.metadata?.localizedPriceString ?? $"{CurrencyCode}{_setting.Price}";
        };
    }
    
}