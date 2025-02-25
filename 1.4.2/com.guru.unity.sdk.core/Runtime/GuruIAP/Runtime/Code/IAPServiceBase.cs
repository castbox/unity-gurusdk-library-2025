
namespace Guru.IAP
{
    using System;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Security;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    
    public abstract class IAPServiceBase<T>: IStoreListener where T: IAPServiceBase<T> , new()
    {
        private const int OrderRequestTimeout = 10;
        private const int OrderRequestRetryTimes = 3;

        #region 属性定义

        private const string Tag = "[IAP]";
        private const string DefaultCategory = "Store";

        private static bool _showLog;
        
        private ConfigurationBuilder _configBuilder; // 商店配置创建器
        
        private IStoreController _storeController;
        private IExtensionProvider _storeExtensionProvider;
        private IAppleExtensions _appleExtensions;
        private IGooglePlayStoreExtensions _googlePlayStoreExtensions;
        
        private CrossPlatformValidator _validator;
        private Dictionary<string, ProductInfo> _products;
        protected Dictionary<string, ProductInfo> Products => _products;
        private string[] _productNameList;

        public bool IsInitialized => _storeController != null && _storeExtensionProvider != null && _model != null;

        private ProductInfo _curProductInfo = null;
        private string _curProductCategory = "";

        public string CurrentBuyingProductId
        {
            get
            {
                if (_curProductInfo != null)
                {
                    return _curProductInfo.Id;
                }
                return "";
            }
            
        }

        protected IAPModel _model;
        private string _appBundleId;
        private string _idfv;
        
        
        /// <summary>
        /// 是否是首次购买
        /// </summary>
        public int PurchaseCount
        {
            get => _model.PurchaseCount;
            set => _model.PurchaseCount = value;
        }
        
        /// <summary>
        /// 是否是首个IAP
        /// </summary>
        public bool IsFirstIAP => PurchaseCount == 0;

        private byte[] _googlePublicKey;
        private byte[] _appleRootCert;
        private string _uid;
        private string _uuid;
        /// <summary>
        /// 服务初始化回调
        /// </summary>
        public event Action<bool> OnInitResult;
        
        /// <summary>
        /// 恢复购买回调
        /// </summary>
        public event Action<bool, string> OnRestored;

        public event Action<string> OnBuyStart;
        public event Action<string, bool> OnBuyEnd;
        public event Action<string, string> OnBuyFailed;
        public event Action<string, string, bool> OnGetProductReceipt;

#if UNITY_IOS
        /// <summary>
        /// AppStore 支付, 处理苹果支付延迟反应
        /// </summary>
        /// <returns></returns>
        public event Action<Product> OnAppStorePurchaseDeferred;
#endif

        #endregion
        
        #region 单利模式
        
        protected static T _instance;
        private static object _locker = new object();
        
        public static T Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (_locker)
                    {
                        _instance = Activator.CreateInstance<T>();
                        _instance.OnCreatedInit();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 组件创建初始化
        /// </summary>
        protected virtual void OnCreatedInit()
        {
            Debug.Log("--- IAPService Init");
        }
        

        #endregion
        
        #region 初始化

        /// <summary>
        /// 初始化支付服务
        /// </summary>
        public virtual void Initialize(string uid, string bundleId, string idfv = "", bool showLog = false)
        {
            
            if (string.IsNullOrEmpty(uid)) uid = IPMConfig.IPM_UID;
            _uid = uid;
            _showLog = showLog;
            _appBundleId = bundleId;
            _idfv = idfv;
            InitPurchasing();
        }

        /// <summary>
        /// 带有校验器的初始化
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="googlePublicKey"></param>
        /// <param name="appleRootCert"></param>
        /// <param name="idfv"></param>
        /// <param name="showLog"></param>
        /// <param name="bundleId"></param>
        public virtual void InitWithKeys(string uid, byte[] googlePublicKey, byte[] appleRootCert, string bundleId, string idfv = "", bool showLog = false)
        {
            _googlePublicKey = googlePublicKey;
            _appleRootCert = appleRootCert;
            InitModel();
            Initialize(uid, bundleId, idfv, showLog);
        }


        public void SetUID(string uid)
        {
            if (_configBuilder != null && !string.IsNullOrEmpty(uid))
            {
                _uid = uid;
                _configBuilder.Configure<IGooglePlayConfiguration>().SetObfuscatedAccountId(uid); 
                Debug.Log($"[IAP] --- Set UID: {uid}");
            }
        }
        
        
        public void SetUUID(string uuid)
        {
            if (_appleExtensions != null && !string.IsNullOrEmpty(uuid))
            {
                _uuid = uuid;
                _appleExtensions.SetApplicationUsername(uuid);
                Debug.Log($"[IAP] --- Set UUID: {uuid}");
            }
        }



        /// <summary>
        /// 初始化支付插件
        /// </summary>
        protected virtual void InitPurchasing()
        {
            _configBuilder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            // 注入初始商品产品列表
            var settings = GetProductSettings();
            if (null != settings)
            {
                int len = settings.Length;
                
                if(_products != null) _products.Clear();
                _products = new Dictionary<string, ProductInfo>(len);
                _productNameList = new string[len];
                
                ProductSetting item;
                IDs ids;
                bool emptyIDs = false;
                for (int i = 0; i < len; i++)
                {
                    item = settings[i];
                    ids = new IDs();
                    if (!string.IsNullOrEmpty(item.GooglePlayProductId))
                    {
                        ids.Add(item.GooglePlayProductId, GooglePlay.Name);
                    }
                    else
                    {
#if UNITY_ADNROID
                        emptyIDs = true;
                        LogE($"[IAP] --- GoogleProductId is empty, please check the product setting: {item.ProductName}");
#endif
                    }


                    if (!string.IsNullOrEmpty(item.AppStoreProductId))
                    {
                        ids.Add(item.AppStoreProductId, AppleAppStore.Name);
                    }
                    else
                    {
#if UNITY_IOS
                       emptyIDs = true;
                        LogE($"[IAP] --- AppleProductId is empty, please check the product setting: {item.ProductName}");
#endif
                    }

                    if (emptyIDs)
                    {
                        continue;
                    }
                    
                    _configBuilder.AddProduct(item.ProductId, item.Type, ids); // 添加商品

                    // 建立本地的商品信息列表
                    if (string.IsNullOrEmpty(item.Category)) item.Category = DefaultCategory;
                    _products[item.ProductId] = new ProductInfo(item);
                    _productNameList[i] = item.ProductName;
                }
            }
            // 调用插件初始化
            UnityPurchasing.Initialize(this, _configBuilder);
        }
        
         /// <summary>
        /// 初始化成功
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="extensions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            _storeExtensionProvider = extensions;
            var uuid = IPMConfig.IPM_UUID;
            if(string.IsNullOrEmpty(_uid)) _uid = IPMConfig.IPM_UID;

            if (!string.IsNullOrEmpty(_uid) && string.IsNullOrEmpty(uuid))
            {
                uuid = IDHelper.GenUUID(_uid);
            }
            LogI($"--- IAP Initialized Success. With UID: {_uid} UUID: {uuid} DeviceId: {IPMConfig.IPM_DEVICE_ID}");

#if UNITY_IOS
            _appleExtensions = extensions.GetExtension<IAppleExtensions>();
            _appleExtensions.SetApplicationUsername(uuid);  // SetUp UUID (8)-(4)-(4)-(12): xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
            // On Apple platforms we need to handle deferred purchases caused by Apple's Ask to Buy feature.
            // On non-Apple platforms this will have no effect; OnDeferred will never be called.
            _appleExtensions.RegisterPurchaseDeferredListener(item =>
            {
                LogI("Purchase deferred: " + item.definition.id);
                OnAppStorePurchaseDeferred?.Invoke(item);
            });
            
            var appReceipt = _configBuilder.Configure<IAppleConfiguration>().appReceipt;
            if (!string.IsNullOrEmpty(appReceipt))
            {
                LogI($"[IAP] --- AppReceipt: {appReceipt}");
            }
            
#elif UNITY_ANDROID
            _configBuilder.Configure<IGooglePlayConfiguration>().SetObfuscatedAccountId(_uid); // SetUp UID
            _googlePlayStoreExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();
            //添加安装游戏后第一次初试化进行恢复购买的回调 只有安卓才有
            _googlePlayStoreExtensions.RestoreTransactions(OnRestoreHandle);
#endif
            
            foreach (var product in _storeController.products.all)
            {
                if (!product.availableToPurchase)
                {
                    continue;
                }
                
                if (_products.ContainsKey(product.definition.id))
                {
                    _products[product.definition.id].SetProduct(product);
                }
            }

            InitValidator(); // 初始化订单验证器
            OnInitResult?.Invoke(true);
        }

         /// <summary>
         /// 初始化失败
         /// </summary>
         /// <param name="error"></param>
         /// <exception cref="NotImplementedException"></exception>
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            LogE($"--- IAP Initialized Fail: {error}");
            OnInitResult?.Invoke(false);
        }

         /// <summary>
         /// 初始化失败
         /// </summary>
         /// <param name="error"></param>
         /// <param name="message"></param>
         /// <exception cref="NotImplementedException"></exception>
         public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            LogE($"--- IAP Initialized Fail: {error}   msg: {message}");
            OnInitResult?.Invoke(false);
        }

        #endregion

        #region 数据查询
        
        // <summary>
        /// 获取商品Info
        /// </summary>
        /// <param name="productName">商品名称</param>
        /// <returns></returns>
        public ProductInfo GetInfo(string productName)
        {
            if(null == Products || Products.Count == 0 ) return null;
            return Products.Values.FirstOrDefault(c => c.Name == productName);
        }
        
        /// <summary>
        /// 通过商品ID获取对应的信息
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <returns></returns>
        public ProductInfo GetInfoById(string productId)
        {
            if(null == Products || Products.Count == 0 ) return null;
            return Products.Values.FirstOrDefault(c => c.Id == productId);
        }
        
        /// <summary>
        /// 返回全部商品的列表信息
        /// </summary>
        /// <returns></returns>
        public ProductInfo[] GetAllProductInfos()
        {
            int len = _productNameList.Length;
            ProductInfo[] infos = new ProductInfo[len];
            for (int i = 0; i < len; i++)
            {
                infos[i] = Products[_productNameList[i]];
            }
            return infos;
        }

        /// <summary>
        /// 获取道具价格
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public double GetProductPrice(string name)
        {
            if (_storeController == null || _storeController.products == null)
            {
                return Fallback();
            }

            ProductInfo info = GetInfo(name);
            var product = _storeController.products.WithID(info.Id);
            if (product == null)
                return Fallback();

            return (double)product.metadata.localizedPrice;

            double Fallback()
            {
                ProductInfo info = GetInfo(name);
                return info?.Price ?? 0.0;
            }
        }
        
        
        /// <summary>
        /// 获取道具价格（带单位 $0.01）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetProductPriceString(string name)
        {
            if (_storeController == null || _storeController.products == null)
            {
                return Fallback();
            }

            ProductInfo info = GetInfo(name);
            var product = _storeController.products.WithID(info.Id);
            if (product == null)
                return Fallback();

            return product.metadata.localizedPriceString;

            string Fallback()
            {
                ProductInfo info = GetInfo(name);
                var pr = info?.Price ?? 0.0;
                return "$" + pr;
            }
        }

        /// <summary>
        /// 获取 IAP 内置商品
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        public Product GetProduct(string productName)
        {
            if (_storeController != null && _storeController.products != null)
            {
                var info = GetInfo(productName);
                if (info != null)
                {
                    return _storeController.products.WithID(info.Id);
                }
                Debug.LogError($"[IAP] --- can't find <ProductInfo> with name {productName}");
            }
            
            // 商品不存在则返回 NULL
            Debug.LogError($"[IAP] --- _storeController is null or products is null or products.all.Length == 0");
            return null;
        }

        /// <summary>
        /// 当前的商品是否已经持有收据了
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        public bool IsProductHasReceipt(string productName)
        {
            var product = GetProduct(productName);
            if (product != null) return product.hasReceipt;
            return false;
        }


        #endregion

        #region 订单验证器

        /// <summary>
        /// 是否支持订单校验
        /// </summary>
        /// <returns></returns>
        private bool IsCurrentStoreSupportedByValidator() 
            => IsGooglePlayStoreSelected() || IsAppleAppStoreSelected();


        /// <summary>
        /// Google 商店支持
        /// </summary>
        /// <returns></returns>
        private bool IsGooglePlayStoreSelected()
        {
            var currentAppStore = StandardPurchasingModule.Instance().appStore;
            return currentAppStore == AppStore.GooglePlay;
        }

        /// <summary>
        /// Apple 商店支持
        /// </summary>
        /// <returns></returns>
        private bool IsAppleAppStoreSelected()
        {
            var currentAppStore = StandardPurchasingModule.Instance().appStore;
            return currentAppStore == AppStore.AppleAppStore || currentAppStore == AppStore.MacAppStore;
        }

        /// <summary>
        /// 初始化订单校验器
        /// </summary>
        protected virtual void InitValidator()
        {
            if (IsCurrentStoreSupportedByValidator())
            {
                try
                {
                    if (_googlePublicKey != null && _appleRootCert != null)
                    {
                        _validator = new CrossPlatformValidator(_googlePublicKey, _appleRootCert, Application.identifier);
                    }
                    else
                    {
                        Analytics.LogCrashlytics(new Exception($"[IAP] Init Validator failed -> googlePublicKey: {_googlePublicKey}  appleRootCert: {_appleRootCert}"));
                    }
                }
                catch (NotImplementedException exception)
                {
                    LogE("Cross Platform Validator Not Implemented: " + exception);
                }
            }
        }


        #endregion

        #region 恢复购买

        /// <summary>
        /// 恢复购买
        /// </summary>
        /// <param name="success"></param>
        /// <param name="msg"></param>
        protected virtual void OnRestoreHandle(bool success, string msg)
        {
            LogI($"--- Restore complete: {success}: msg:{msg}" );

            
            if (success)
            {
                bool isIAPUser = false;
                // 扫描所有商品, 追加用户属性
                for (int i = 0; i < _storeController.products.all.Length; i++)
                {
                    var product = _storeController.products.all[i];
                    if (product.hasReceipt)
                    {
                        isIAPUser = true;
                    }
                }
                if(isIAPUser) SetIsIAPUser(true);
            }
            
            OnRestored?.Invoke(success, msg);
        }

        /// <summary>
        /// 恢复购买道具
        /// </summary>
        public virtual void Restore()
        {
            if (!IsInitialized) return;

#if UNITY_IOS
            _appleExtensions.RestoreTransactions(OnRestoreHandle);
#elif UNITY_ANDROID
            _googlePlayStoreExtensions.RestoreTransactions(OnRestoreHandle);
#endif
        }
        

        #endregion

        #region 购买流程
        
        /// <summary>
        /// 购买商品
        /// </summary>
        /// <param name="productName"></param>
        public virtual T Buy(string productName, string category = "")
        {
            if (!IsInitialized)
            {
                LogE("Buy FAIL. Not initialized.");
                OnBuyEnd?.Invoke(productName, false);
                return (T)this;
            }
            ProductInfo info = GetInfo(productName);
            if (info == null)
            {
                LogE($"Buy FAIL. No product with name: {productName}");
                OnBuyEnd?.Invoke(productName, false);
                return (T)this;
            }
            
            Product product = _storeController.products.WithID(info.Setting.ProductId);
            if (product != null && product.availableToPurchase)
            {
                if (string.IsNullOrEmpty(category)) category = info.Category;
                _storeController.InitiatePurchase(product);  // 发起购买
                _curProductInfo = info;
                _curProductCategory = category;
                
                // Analytics.IAPClick(_curProductCategory, product.definition.id);
                // Analytics.IAPImp(_curProductCategory, product.definition.id);  // <--- Client should report this Event
                
                OnBuyStart?.Invoke(productName);
                return (T)this;
            }

            // 找不到商品
            LogE($"Can't find product by name: {productName}, pay canceled.");
            OnPurchaseOver(false, productName);
            OnBuyEnd?.Invoke(productName, false);

            return (T)this;
        }
        
        /// <summary>
        /// 处理支付流程
        /// </summary>
        /// <param name="purchaseEvent"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            string productId = purchaseEvent.purchasedProduct.definition.id;
            ProductInfo info = GetInfoById(productId);
            bool success = false;
            string productName = "";
            if (null != info)
            {
                success = true;
                productName = info.Name;
                SetIsIAPUser(true); // 设置用户属性标记

                LogI($"{Tag} --- OnPurchaseSuccess :: purchase count: {PurchaseCount}  productName: {productName}");
                
                // 只有实际发生购买后才会有订单上报.  启动时的 Restore 操作自动调用支付成功. 这里做一个判定, 过滤掉订单的物品
                if (_curProductInfo != null)
                {
                    ReportPurchaseResult(purchaseEvent); // 订单上报
                    
                    // 真实购买后上报对应的事件
                    if (IsFirstIAP) {
                        // 上报首次支付打点
                        Analytics.FirstIAP(info.Id, info.Price, info.CurrencyCode); 
                    }
                    Analytics.ProductIAP(info.Id,info.Id, info.Price, info.CurrencyCode);
                }
                
                var pp = purchaseEvent.purchasedProduct;
                if ( pp == null || string.IsNullOrEmpty(pp.receipt))
                {
                    string msg = $"{Tag} ---  Purchased product is null or has no receipt!!";
                    Debug.LogError(msg);
                    Analytics.LogCrashlytics(new Exception(msg));
                }
                else
                {
                    OnGetProductReceipt?.Invoke(pp.definition.id, pp.receipt, pp.appleProductIsRestored);
                }
                
                PurchaseCount++; // 记录支付次数
            }
            else
            {
                string msg = $"{Tag} ---  Purchased end, but can't find ProductInfo with ID: {productId}";
                Debug.LogError(msg);
                Analytics.LogCrashlytics(new Exception(msg));
            }
            
            LogI($"{Tag} --- Call OnBuyEnd [{productName}] :: {OnBuyEnd}");
            OnBuyEnd?.Invoke(productName, success);
            OnPurchaseOver(success, productName); // 支付成功处理逻辑
            ClearCurPurchasingProduct(); // 清除购买缓存
            
            return PurchaseProcessingResult.Complete; // 直接Consume 掉当前的商品
        }

        /// <summary>
        /// 支付失败
        /// 2024-12-28, 根据 BI 需求规范化支付失败的原因字段：
        /// 原始需求地址：<a>https://www.tapd.cn/38215047/prong/stories/view/1138215047001023374</a>
        /// </summary>
        /// <param name="product"></param>
        /// <param name="failureReason"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            string productId = product.definition.id;
            ProductInfo info = GetInfoById(productId);
            var reasonStr = failureReason == PurchaseFailureReason.UserCancelled ? 
                "cancel" : failureReason.ToString();
        
            //上报点位，用户购买失败的原因
            Analytics.IAPRetFalse(_curProductCategory, product.definition.id, reasonStr);

            LogI($"{Tag} --- OnPurchaseFailed :: Reason = {reasonStr}");
            // 失败的处理逻辑
            OnPurchaseOver(false, info.Name);
            OnBuyEnd?.Invoke(info.Name, false);
            // 失败原因
            OnBuyFailed?.Invoke(info.Name, reasonStr);
            ClearCurPurchasingProduct(); // 清除购买缓存
        }
        
        private void ClearCurPurchasingProduct()
        {
            _curProductInfo = null;
            _curProductCategory = "";
        }


        /// <summary>
        /// 获取商品的本地化价格字符串
        /// 如果商品不存在或者 IAP 尚未初始化完成则显示 "Loading" 字样
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        public string GetLocalizedPriceString(string productName)
        {
            return GetInfo(productName)?.LocalizedPriceString ?? "Loading";
        }

        #endregion

        #region Log 输出

        /// <summary>
        /// 日志输出
        /// </summary>
        /// <param name="msg"></param>
        private static void LogI(object msg)
        {
            if (_showLog) 
                Debug.Log($"{Tag} {msg}");
        }

        private static void LogE(object msg)
        {
            if (_showLog) 
                Debug.LogError($"{Tag} {msg}");
        }

        private static void LogW(object msg)
        {
            if (_showLog)
                Debug.LogWarning($"{Tag} {msg}");
        }

        #endregion

        #region 实现接口

        /// <summary>
        /// 需要游戏侧继承并完成Blevel的取值上报
        /// </summary>
        /// <returns></returns>
        protected abstract int GetBLevel();

        /// <summary>
        /// 获取商品品配置列表
        /// </summary>
        /// <returns></returns>
        protected virtual ProductSetting[] GetProductSettings()
            => GuruSettings.Instance.Products;

        /// <summary>
        /// 支付回调
        /// </summary>
        /// <param name="success">是否成功</param>
        /// <param name="productName">商品名称</param>
        protected abstract void OnPurchaseOver(bool success, string productName);

        #endregion
        
        #region 支付上报逻辑
        
        /// <summary>
        /// 支付结果上报
        /// </summary>
        protected virtual bool ReportPurchaseResult(PurchaseEventArgs args)
        {
            // 验证器判定
            if (_validator == null)
            {
                // Debug.Log($"############ --- Validator is null");
                LogE($"{Tag} --- Validator is null. Report Order failed.");
                Analytics.LogCrashlytics(new Exception($"IAPService can not report order because Validator is null!"));
                return false;
            }
            
            //---------------- All Report Information --------------------
            int level = GetBLevel();
            int orderType = args.purchasedProduct.definition.type == ProductType.Subscription ? 1 : 0;
            string productId = args.purchasedProduct.definition.id;
            // string appleReceiptString = "";
            string userCurrency = args.purchasedProduct.metadata.isoCurrencyCode;
            double payPrice = decimal.ToDouble(args.purchasedProduct.metadata.localizedPrice);
            string scene = _curProductCategory ?? "Store";
            bool isFree = false;
            if (orderType == 1) isFree = IsSubscriptionFreeTrailById(productId);
            
            //---------------- All Report Information --------------------
            
            LogI($"--- Report b_level:[{level}] with product id:{args.purchasedProduct.definition.id} ");
            
#if UNITY_EDITOR
            // // Editor 不做上报逻辑
            LogI($"--- Editor Validate Success. But Editor can't report order.");
            return true;
#endif
            IPurchaseReceipt[] allReceipts = null;
            
            try
            {
                allReceipts = _validator.Validate(args.purchasedProduct.receipt);
                string offerId = "";
                string basePlanId = "";

                if (Application.platform == RuntimePlatform.Android)
                {
                    // ---- Android 订单验证, 上报打点信息 ---- 
                    foreach (var receipt in allReceipts)
                    {
                        if (receipt is GooglePlayReceipt googleReceipt)
                        {
                            ReportGoogleOrder(orderType, 
                                googleReceipt.productID,
                                googleReceipt.purchaseToken,
                                googleReceipt.orderID,
                                googleReceipt.purchaseDate, 
                                level, 
                                userCurrency, payPrice, scene, isFree,
                                offerId,  basePlanId);
                        }
                    }
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    string appleReceiptString = "";
                    // ---- iOS 订单验证, 上报打点信息 ----
                    var jsonData = JsonConvert.DeserializeObject<JObject>(args.purchasedProduct.receipt);
                    if (jsonData != null && jsonData.TryGetValue("Payload", out var recp))
                    {
                        appleReceiptString = recp.ToString();
                        LogI($"--- [{productId}] iOS receipt: {appleReceiptString}");      
                    }
                
                    Debug.Log($"[IAP] --- Full receipt: \n{args.purchasedProduct.receipt}");
                    
                    foreach (var receipt in allReceipts)
                    {
                        if (receipt is AppleInAppPurchaseReceipt appleReceipt
                            && receipt.productID == args.purchasedProduct.definition.id)
                        {
                            ReportAppleOrder(orderType, 
                                appleReceipt.productID,
                                appleReceiptString,
                                appleReceipt.transactionID,
                                appleReceipt.purchaseDate, 
                                level, 
                                userCurrency, payPrice, scene, 
                                _appBundleId,
                                _idfv,
                                isFree);

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogE($" [IAPManager.RevenueUpload] got Exception: {ex.Message}");
                Analytics.LogCrashlytics(new Exception($"[IAP] Unity report purchase data with b_level={level} got error: {ex.Message}"));
                return false;
            }
            
            LogI("--- All Receipt is valid ---");
            return true;
        }

        #endregion
        
        #region IOS Orders Collection
        
        private HashSet<string> iOSReceipts;
        public HashSet<string> IOSReceiptCollection
        {
            get
            {
                // 读取订单信息
                if (iOSReceipts == null)
                {
                    iOSReceipts = new HashSet<string>();
                    string raw = PlayerPrefs.GetString(nameof(IOSReceiptCollection), "");
                    if (!string.IsNullOrEmpty(raw))
                    {
                        var arr = raw.Split(',');
                        for (int i = 0; i < arr.Length; i++)
                        {
                            iOSReceipts.Add(arr[i]);
                        }
                    }
                }
                return iOSReceipts;
            }

            set
            {
                // 保存订单信息
                iOSReceipts = value;
                PlayerPrefs.SetString(nameof(IOSReceiptCollection), string.Join(",", iOSReceipts));
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// 添加订单信息
        /// </summary>
        /// <param name="receipt"></param>
        public void AddReceipt(string receipt)
        {
            if (!HasReceipt(receipt))
            {
                IOSReceiptCollection.Add(receipt);
            }
        }

        /// <summary>
        /// 是否包含订单
        /// </summary>
        /// <param name="receipt"></param>
        /// <returns></returns>
        public bool HasReceipt(string receipt)
        {
            return IOSReceiptCollection.Contains(receipt);
        }


        #endregion

        #region 用户标志位设置
        
        /// <summary>
        /// 标记是否为付费用户
        /// </summary>
        /// <param name="value"></param>
        private static void SetIsIAPUser(bool value = true)
        {
            if (Instance != null && Instance._model != null && value)
            {
                Instance._model.SetIsIapUser(true); // 用户属性
            }
            Analytics.SetIsIapUser(value);
        }


        #endregion

        #region 数据初始化


        private void InitModel()
        {
            _model = IAPModel.Load(); // 初始化 Model
            
            // 启动时查询
            if(_orderRequests == null) 
                _orderRequests = new Queue<RequestBase>(20);

// #if UNITY_EDITOR
//             Debug.Log($"----- IAP Model init -----");         
// #elif UNITY_ANDROID

#if UNITY_ANDROID         
            if (_model.HasUnreportedGoogleOrder)
            {
                int i = 0;
                while (_model.googleOrders.Count > 0 
                       && i < _model.googleOrders.Count)
                {
                    var o = _model.googleOrders[i];
                    ReportGoogleOrder(o);
                    i++;
                }
            }
#elif UNITY_IOS
            if (_model.HasUnreportedAppleOrder)
            {
                int i = 0;
                while (_model.appleOrders.Count > 0 
                       && i < _model.appleOrders.Count)
                {
                    var o = _model.appleOrders[i];
                    ReportAppleOrder(o);
                    i++;
                }
            }
#endif
            

        }
        

        #endregion

        #region 订单上报队列
        
        private bool isOrderSending = false;
        private Queue<RequestBase> _orderRequests = new Queue<RequestBase>(20);


        /// <summary>
        /// 上报 Google Order Request
        /// </summary>
        /// <param name="orderType"></param>
        /// <param name="productId"></param>
        /// <param name="subscriptionId"></param>
        /// <param name="token"></param>
        /// <param name="date"></param>
        /// <param name="level"></param>
        /// <param name="offerId"></param>
        /// <param name="basePlanId"></param>
        /// <param name="orderId"></param>
        private void ReportGoogleOrder(int orderType, string productId, string token, 
            string orderId, DateTime date, int level, 
            string userCurrency, double payPrice, string scene, bool isFree = false,
            string offerId = "", string basePlanId = "")
        {
            var payedDate = TimeUtil.GetTimeStampString(date);
            var request = GoogleOrderRequest.Build(orderType, productId, token, orderId, payedDate, level, 
                userCurrency, payPrice, scene, isFree, offerId, basePlanId);
            ReportNextOrder(request);
        }
        private void ReportGoogleOrder(GoogleOrderData data)
        {
            var request = GoogleOrderRequest.Build(data);
            ReportNextOrder(request);
        }

        /// <summary>
        /// 上报 Apple Order Request
        /// </summary>
        /// <param name="orderType"></param>
        /// <param name="productId"></param>
        /// <param name="receipt"></param>
        /// <param name="orderId"></param>
        /// <param name="date"></param>
        /// <param name="level"></param>
        /// <param name="userCurrency"></param>
        /// <param name="payPrice"></param>
        /// <param name="scene"></param>
        /// <param name="idfv"></param>
        /// <param name="isFree"></param>
        /// <param name="offerId"></param>
        /// <param name="basePlanId"></param>
        /// <param name="bundleId"></param>
        private void ReportAppleOrder(int orderType, string productId, string receipt, 
            string orderId, DateTime date,int level, string userCurrency, double payPrice, string scene, string bundleId, string idfv,bool isFree = false,
            string offerId = "", string basePlanId = "")
        {
            var payedDate = TimeUtil.GetTimeStampString(date);
            var request = AppleOrderRequest.Build(orderType, productId, receipt, orderId, payedDate, level,
                userCurrency, payPrice, scene, bundleId, idfv,isFree, offerId, basePlanId);
            ReportNextOrder(request);
        }
        
        private void ReportAppleOrder(AppleOrderData data)
        {
            var request = AppleOrderRequest.Build(data);
            ReportNextOrder(request);
        }
        
        private void ReportNextOrder(RequestBase request)
        {
            if(_orderRequests == null) _orderRequests = new Queue<RequestBase>(20);
            _orderRequests.Enqueue(request);
            
            if(isOrderSending) return;
            isOrderSending = true;
            
            OnSendNextOrder();
        }
        
        
        /// <summary>
        /// 上报下一个订单数据
        /// </summary>
        private void OnSendNextOrder()
        {
            if (_orderRequests != null && _orderRequests.Count > 0)
            {
                // 如果上报队列不为空, 则尝试上报
                isOrderSending = true;
                var request = _orderRequests.Dequeue();
                if (request == null)
                {
                    // 跳过空请求
                    OnSendNextOrder();
                    return;
                }

                GoogleOrderRequest googleReq = request as GoogleOrderRequest;
                AppleOrderRequest appReq = request as AppleOrderRequest;
      
                if (googleReq != null)
                {
                    if (_model.IsTokenExists(googleReq.token))
                    {
                        OnSendNextOrder(); // 跳过上报过的 Google 订单
                        return;
                    }
                    _model.AddGoogleOrder(googleReq.orderData); // 缓存当前 orderData 等待上报后再消除 
                }
                else if( appReq != null)
                {
                    if (_model.IsReceiptExist(appReq.transactionID))
                    {
                        OnSendNextOrder(); // 跳过上报过的 Apple 订单
                        return;
                    }
                    _model.AddAppleOrder(appReq.orderData); // 缓存当前 orderData 等待上报后再消除 
                }

                request.SetTimeOut(OrderRequestTimeout)
                    .SetRetryTimes(OrderRequestRetryTimes)
                    .SetSuccessCallBack(() =>
                    {
                        //---------------- Success ------------------------
                        if (googleReq != null)
                        {
                            _model.AddToken(googleReq.token); // 记录当前的 Google 订单
                            _model.RemoveGoogleOrder(googleReq.orderData); // 成功后清除缓存 orderData
                        }
                        else if (appReq != null)
                        {
                            _model.AddReceipt(appReq.transactionID); // 记录当前的 Apple 订单
                            _model.RemoveAppleOrder(appReq.orderData); // 成功后清除缓存 orderData
                        }
                        OnSendNextOrder(); // NEXT Order
                    })
                    .SetFailCallBack(() =>
                    {
                        //---------------- Fail ------------------------
                        if (googleReq != null)
                        {
                            ReportGoogleOrderLost(googleReq.orderData); // 上报 Google 订单缺失打点
                        }
                        else if (appReq != null)
                        {
                            ReportAppleOrderLost(appReq.orderData);  // 上报 Apple 订单缺失打点
                        }
                        OnSendNextOrder(); // NEXT Order
                    })
                    .Send();
            }
            else
            {
                isOrderSending = false;
            }
        }

        private void ReportGoogleOrderLost(GoogleOrderData data)
        {
            // 中台异常打点
            var dict = new Dictionary<string, object>()
            {
                [Analytics.ParameterItemCategory] = "google_order_lost",
                ["data"] = data.ToString(),
            };
            Analytics.LogDevAudit(dict);
        }
        
        private void ReportAppleOrderLost(AppleOrderData data)
        {
            // 中台异常打点
            var dict = new Dictionary<string, object>()
            {
                [Analytics.ParameterItemCategory] = "apple_order_lost",
                ["data"] = data.ToString(),
            };
            Analytics.LogDevAudit(dict);
        }
        
        #endregion

        #region Subscription

        
        public static DateTime DefaultSubscriptionDate = new DateTime(1970, 1,1,0,0,0); 

        
        private SubscriptionManager GetSubManager(string productName)
        {
            var product = GetProduct(productName);
            if (product != null && product.definition.type == ProductType.Subscription)
            {
                return new SubscriptionManager(product, null);
            }
            return null;
        }
        
        private SubscriptionManager GetSubManagerById(string productId)
        {
            var product = _storeController.products.WithID(productId);
            if (product != null && product.definition.type == ProductType.Subscription)
            {
                return new SubscriptionManager(product, null);
            }
            return null;
        }


        public bool IsSubscriptionFreeTrail(string productName)
        {
            if(!IsInitialized) return false;
            
            var smgr = GetSubManager(productName);
            if (smgr != null)
            {
                return smgr.getSubscriptionInfo().isFreeTrial() == Result.True;
            }
            return false;
        }
        
        public bool IsSubscriptionFreeTrailById(string productId)
        {
            if(!IsInitialized) return false;
            
            var smgr = GetSubManagerById(productId);
            if (smgr != null)
            {
                return smgr.getSubscriptionInfo().isFreeTrial() == Result.True;
            }
            return false;
        }


        public bool IsSubscriptionCancelled(string productName)
        {
            if(!IsInitialized) return false;
            
            var smgr = GetSubManager(productName);
            if (smgr != null)
            {
                return smgr.getSubscriptionInfo().isCancelled() == Result.True;
            }
            return false;
        }

        public bool IsSubscriptionAvailable(string productName)
        {
            if(!IsInitialized) return false;
            
            var smgr = GetSubManager(productName);
            if (smgr != null)
            {
                return smgr.getSubscriptionInfo().isSubscribed() == Result.True;
            }
            return false;
        }


        public bool IsSubscriptionExpired(string productName)
        {
            if(!IsInitialized) return false;
            
            var smgr = GetSubManager(productName);
            if (smgr != null)
            {
                return smgr.getSubscriptionInfo().isExpired() == Result.True;
            }
            return false;
        }
        
        
        public bool IsSubscriptionAutoRenewing(string productName)
        {
            if(!IsInitialized) return false;
            
            var smgr = GetSubManager(productName);
            if (smgr != null)
            {
                return smgr.getSubscriptionInfo().isAutoRenewing() == Result.True;
            }
            return false;
        }
        
        
        /// <summary>
        /// IntroductioryPrice Period
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        public bool IsSubscriptionIntroductoryPricePeriod(string productName)
        {
            if(!IsInitialized) return false;
            
            var smgr = GetSubManager(productName);
            if (smgr != null)
            {
                return smgr.getSubscriptionInfo().isIntroductoryPricePeriod() == Result.True;
            }
            return false;
        }
        
        
        
        public DateTime GetSubscriptionExpireDate(string productName)
        {
            if(!IsInitialized) return DefaultSubscriptionDate;
            
            var smgr = GetSubManager(productName);
            if (smgr != null)
            {
                return smgr.getSubscriptionInfo()?.getExpireDate() ?? DateTime.Now;
            }
            return DefaultSubscriptionDate;
        }
        
        
        public DateTime GetSubscriptionPurchaseDate(string productName)
        {
            if(!IsInitialized) return DefaultSubscriptionDate;
            
            var smgr = GetSubManager(productName);
            if (smgr != null)
            {
                return smgr.getSubscriptionInfo().getPurchaseDate();
            }
            return DefaultSubscriptionDate;
        }
        
        
        public DateTime GetSubscriptionCancelDate(string productName)
        {
            if(!IsInitialized) return DefaultSubscriptionDate;
            
            var smgr = GetSubManager(productName);
            if (smgr != null)
            {
                return smgr.getSubscriptionInfo().getCancelDate();
            }
            return DefaultSubscriptionDate;
        }
        

        public TimeSpan GetSubscriptionRemainingTime(string productName)
        {
            if(!IsInitialized) return TimeSpan.Zero;
            
            var smgr = GetSubManager(productName);
            if (smgr != null)
            {
                return smgr.getSubscriptionInfo().getRemainingTime();
            }
            return TimeSpan.Zero;
        }
        
        public TimeSpan GetSubscriptionIntroductoryPricePeriod(string productName)
        {
            if(!IsInitialized) return TimeSpan.Zero;
            
            var smgr = GetSubManager(productName);
            if (smgr != null)
            {
                return smgr.getSubscriptionInfo().getIntroductoryPricePeriod();
            }
            return TimeSpan.Zero;
        }
        
        
        public TimeSpan GetSubscriptionFreeTrialPeriod(string productName)
        {
            if(!IsInitialized) return TimeSpan.Zero;
            
            var smgr = GetSubManager(productName);
            if (smgr != null)
            {
                return smgr.getSubscriptionInfo().getFreeTrialPeriod();
            }
            return TimeSpan.Zero;
        }
        
        public string GetSubscriptionInfoJsonString(string productName)
        {
            if(!IsInitialized) return "";
            
            var smgr = GetSubManager(productName);
            if (smgr != null)
            {
                return smgr.getSubscriptionInfo().getSubscriptionInfoJsonString();
            }
            return "";
        }
        
        #endregion
        
    }

}