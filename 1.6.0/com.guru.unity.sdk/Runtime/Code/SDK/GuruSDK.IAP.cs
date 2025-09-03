using System.Collections.Generic;
using Guru.IAP;
using UnityEngine;
namespace Guru
{
    using System;
    using System.Linq;
    
    public partial class GuruSDK
    {
        public static bool IsIAPReady = false;

        //---------- 支付失败原因 ----------
        public const string BuyFail_PurchasingUnavailable = "PurchasingUnavailable";
        public const string BuyFail_Pending = "ExistingPurchasePending";
        public const string BuyFail_ProductUnavailable = "ProductUnavailable";
        public const string BuyFail_SignatureInvalid = "SignatureInvalid";
        public const string BuyFail_UserCancelled = "UserCancelled";
        public const string BuyFail_PaymentDeclined = "PaymentDeclined";
        public const string BuyFail_DuplicateTransaction = "DuplicateTransaction";
        public const string BuyFail_Unknown = "Unknown";
        
        #region Start
        
        /// <summary>
        /// 初始化IAP 功能
        /// </summary>
        // public static void InitIAP(string uid, byte[] googleKey, byte[] appleRootCerts, string bundleId, string idfv = "")
        private static void InitIAP()
        {
            // Instance._guruIAPManager.OnInitResult += OnIAPInitResult;
            // Instance._guruIAPManager.OnRestored += OnRestored;
            // Instance._guruIAPManager.OnBuyStart += OnBuyStart;
            // Instance._guruIAPManager.OnBuyEnd += OnBuyEnd;
            // Instance._guruIAPManager.OnBuyFailed += OnBuyFailed;
            // Instance._guruIAPManager.OnGetProductReceipt += OnGetReceipt;
            
            // Instance._guruIAPManager.InitWithKeys(uid, googleKey, appleRootCerts, bundleId, idfv, DebugModeEnabled);

            
            // -------- 使用宏来选择注入支付服务的版本 --------------
#if GURU_IAP_V5
            _guruIAPService = new IAPServiceV5();     
#else
            _guruIAPService = new IAPServiceV4();
#endif
            
            
            // 初始化新版的 SDK
            _guruIAPService.AddInitResultAction(OnIAPInitResult);
            _guruIAPService.AddPurchaseStartAction(OnBuyStart);
            _guruIAPService.AddPurchaseEndAction(OnBuyEnd);
            _guruIAPService.AddPurchaseFailedAction(OnBuyFailed);
            _guruIAPService.AddGetProductReceiptAction(OnGetReceipt);
            _guruIAPService.AddRestoredAction(OnRestored);
            _guruIAPService.Initialize(new GuruSDKIapDataProvider());
            Callbacks.IAP.InvokeOnIAPInitStart(); // 初始化之前进行调用
        }
        
        /// <summary>
        /// 初始化结果
        /// </summary>
        /// <param name="success"></param>
        private static void OnIAPInitResult(bool success)
        {
            LogI($"IAP init result: {success}");
            IsIAPReady = success;
            Callbacks.IAP.InvokeOnIAPInitComplete(success);
        }

        private static bool CheckIAPReady()
        {
            if (!IsIAPReady)
            {
                LogE("IAP is not ready, call <GuruSDK.InitIAP> first.");
                return false;
            }

            return true;
        }

        #endregion
        
        #region Data

        /// <summary>
        /// 获取商品信息
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        public static ProductInfo GetProductInfo(string productName)
        {
            return _guruIAPService?.GetInfo(productName);
        }
        
        /// <summary>
        /// 获取商品信息 (提供 ProductId)
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static ProductInfo GetProductInfoById(string productId)
        {
            return _guruIAPService?.GetInfoById(productId);
        }
        
        
        [Obsolete("Will be discarded in next version. Using GetProductInfoById(string productId) instead.")]
        public static ProductSetting GetProductSettingById(string productId)
        {
            var products = GuruSettings.Instance.Products;
            if (products != null && products.Length > 0)
            {
                return products.FirstOrDefault(p => p.ProductId == productId);   
            }
            return null;
        }
        
        [Obsolete("Will be discarded in next version. Using GetProductInfo(string productName) instead.")]
        public static ProductSetting GetProductSetting(string productName)
        {
            var products = GuruSettings.Instance.Products;
            if (products != null && products.Length > 0)
            {
                return products.FirstOrDefault(p => p.ProductName == productName);   
            }
            return null;
        }

        /// <summary>
        /// 查询某个商品是否已经包含订单了
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        public static bool IsProductHasReceipt(string productName)
        {
            return _guruIAPService.IsProductHasReceipt(productName);
        }


        public static string GetProductLocalizedPriceString(string productName)
        {
            return _guruIAPService.GetLocalizedPriceString(productName);
        }

        
        /// <summary>
        /// 获取所有商品的预定义参数列表
        /// 一般用于在渲染商店列表时， 需要获取所有的商品信息
        /// STORY: https://www.tapd.cn/33527076/prong/stories/view/1133527076001022892?from_iteration_id=1133527076001002763
        /// </summary>
        /// <returns></returns>
        public static ProductInfo[] GetAllProductInfos()
        {
            return _guruIAPService?.GetAllProductInfos() ?? null;
        }


        #endregion
        
        #region Purchase
        
        private static Action<string, bool> InvokeOnPurchaseCallback;

        /// <summary>
        /// 老接口, 将会被废弃
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="purchaseCallback"></param>
        [Obsolete("Will be discarded in next version. Using Purchase(string productName, string category, Action<string, bool> purchaseCallback) instead.")]
        internal static void Purchase(string productName, Action<string, bool> purchaseCallback = null)
        {
            Purchase(productName, "", purchaseCallback);
        }

        /// <summary>
        /// 购买商品, 通过商品Name
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="category"></param>
        /// <param name="purchaseCallback"></param>
        public static void Purchase(string productName, string category = "", Action<string, bool> purchaseCallback = null)
        {
            InvokeOnPurchaseCallback = purchaseCallback;
            if (CheckIAPReady())
            {
                _guruIAPService.Purchase(productName, category);
            }
            else
            {
                purchaseCallback?.Invoke(productName, false);
                UnityEngine.Debug.LogWarning($"{LOG_TAG} --- Iap is not ready, can not purchase anything...");
            }
        }

        /// <summary>
        /// 购买商品, 通过商品ID
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="category"></param>
        /// <param name="purchaseCallback"></param>
        public static bool PurchaseById(string productId, string category = "", Action<string, bool> purchaseCallback = null)
        {
            var productName = GetProductInfoById(productId)?.Name ?? "";
            
            if (CheckIAPReady() && !string.IsNullOrEmpty(productName))
            {
                Purchase(productName, category, purchaseCallback);
                return true;
            }
            return false;
        }
        

        /// <summary>
        /// 支付回调
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="success"></param>
        private static void OnBuyEnd(string productName, bool success)
        {
            InvokeOnPurchaseCallback?.Invoke(productName, success);
            Callbacks.IAP.InvokeOnPurchaseEnd(productName, success);
        }

        /// <summary>
        /// 支付开始
        /// </summary>
        /// <param name="productName"></param>
        private static void OnBuyStart(string productName)
        {
            Callbacks.IAP.InvokeOnPurchaseStart(productName);
        }
        
        /// <summary>
        /// 支付失败
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="reason"></param>
        private static void OnBuyFailed(string productName, string reason)
        {
            Callbacks.IAP.InvokeOnPurchaseFailed(productName, reason);
        }

        #endregion
        
        #region Restore Purchase

        /// <summary>
        /// 恢复购买
        /// </summary>
        /// <param name="restoreCallback"></param>
        public static void Restore(Action<bool, string> restoreCallback = null)
        {
            if( restoreCallback != null) Callbacks.IAP.OnIAPRestored += restoreCallback;
            if (CheckIAPReady())
            {
                _guruIAPService.Restore();
            }
        }
        private static void OnRestored(bool success, string msg)
        {
            Callbacks.IAP.InvokeOnIAPRestored(success, msg); // 更新回复购买回调
        }
        
        #endregion

        #region Receipt

        /// <summary>
        /// 获取订单收据
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="receipt"></param>
        /// <param name="appleProductIsRestored"></param>
        private static void OnGetReceipt(string productId, string receipt, bool appleProductIsRestored = false)
        {
            var productName = GetProductInfoById(productId)?.Name ?? "";
            if(!string.IsNullOrEmpty(productName))
                Model.AddReceipt(receipt, productName, productId, appleProductIsRestored);
        }

        #endregion

        #region Subscriptions

        // ------- 保险起见， 先注销所有和订阅相关的逻辑 -------------
        /**
        
        
        /// <summary>
        /// 订阅是否被取消
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        public static bool IsSubscriptionCancelled(string productName)
        {
            return _guruIAPService.IsSubscriptionCancelled(productName);
        }
        
        /// <summary>
        /// 订阅是否可用
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        public static bool IsSubscriptionAvailable(string productName)
        {
            return _guruIAPService.IsSubscriptionAvailable(productName);
        }
        
        /// <summary>
        /// 订阅是否过期
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        public static bool IsSubscriptionExpired(string productName)
        {
            return _guruIAPService.IsSubscriptionExpired(productName);
        }
        
        public static bool IsSubscriptionFreeTrail(string productName)
        {
            return _guruIAPService.IsSubscriptionFreeTrail(productName);
        }
        
        public static bool IsSubscriptionAutoRenewing(string productName)
        {
            return _guruIAPService.IsSubscriptionAutoRenewing(productName);
        }
        
        public static bool IsSubscriptionIntroductoryPricePeriod(string productName)
        {
            return _guruIAPService.IsSubscriptionIntroductoryPricePeriod(productName);
        }
        
        public DateTime GetSubscriptionExpireDate(string productName)
        {
            return _guruIAPService.GetSubscriptionExpireDate(productName);
        }
        
        
        public DateTime GetSubscriptionPurchaseDate(string productName)
        {
            return _guruIAPService.GetSubscriptionPurchaseDate(productName);
        }
        
        
        public DateTime GetSubscriptionCancelDate(string productName)
        {
            return _guruIAPService.GetSubscriptionCancelDate(productName);
        }
        

        public TimeSpan GetSubscriptionRemainingTime(string productName)
        {
            return _guruIAPService.GetSubscriptionRemainingTime(productName);
        }
        
        public TimeSpan GetSubscriptionIntroductoryPricePeriod(string productName)
        {
            return _guruIAPService.GetSubscriptionIntroductoryPricePeriod(productName);
        }
        
        
        public TimeSpan GetSubscriptionFreeTrialPeriod(string productName)
        {
            return _guruIAPService.GetSubscriptionFreeTrialPeriod(productName);
        }
        
        public string GetSubscriptionInfoJsonString(string productName)
        {
            return _guruIAPService.GetSubscriptionInfoJsonString(productName);
        }
        
        **/

        #endregion


        #region SDK IAP 数据接口

        
        internal class GuruSDKIapDataProvider: IGuruIapDataProvider
        {
            public string AppBundleId => GetAppBundleId();
            public byte[] GooglePublicKeys => GetGooglePublicKeys();
            public byte[] AppleRootCerts => GetAppleRootCerts();
            public string IDFV => GetIdfv();
            public string UID => GetUID();
            public string UUID => GetUUID();
            public ProductSetting[] ProductSettings => GetProductSettings();
            public int BLevel => Model.BLevel;
            
            public bool IsDebug => PlatformUtil.IsDebug();


            public GuruSDKIapDataProvider()
            {
                
            }


            private string GetAppBundleId()
            {
                return _appServicesConfig?.AppBundleId() ?? Application.identifier;
            }

            private byte[] GetGooglePublicKeys()
            {
#if UNITY_EDITOR
                return new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
#endif
                return Instance._initConfig.GoogleKeys;
            }

            private byte[] GetAppleRootCerts()
            {
#if UNITY_EDITOR
                return new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
#endif
                return Instance._initConfig.AppleRootCerts;
            }

            private string GetIdfv()
            {
                if (!string.IsNullOrEmpty(GuruSDK.IDFV))
                    return GuruSDK.IDFV;

#if UNITY_IOS
                return UnityEngine.iOS.Device.vendorIdentifier;
#endif
                return "";
            }

            private string GetUID() => GuruSDK.UID;
            private string GetUUID() => GuruSDK.UUID;

            private ProductSetting[] GetProductSettings()
            {
                if (GuruSettings == null)
                {
                    return new ProductSetting[] { };
                }
                return GuruSettings.Products;
            }

        }
        
        #endregion
    }




}