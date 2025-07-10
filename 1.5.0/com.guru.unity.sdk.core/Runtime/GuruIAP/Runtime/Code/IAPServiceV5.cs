#if GURU_IAP_V5

#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Guru.IAP
{
    public class IAPServiceV5: IGuruIapService
    {
        
        #region 属性定义

        private const string Tag = "[IAP]";
        private const string DefaultCategory = "Store";
        private const string UnknownProductName = "unknown_product";

        private static bool _showLog;
        private IAPModel _model;
        
        // --------- V5 Attrs ----------------
        // private StoreController _storeController;
       
        private GuruStoreController _storeController;
        
        private List<Product> _fetchedProducts;
        private int _storeConnectRetryTimes;
        private bool _connectFailedFlag;
        private string _curProductCategory = "";
        private ProductInfo? _curPurchasingInfo; // 当前支付中的商品

        private ProductSetting[] _localProductSettings;
        private Dictionary<string, ProductInfo> _productMap;
        private CrossPlatformValidator _googlePlatformValidator; // Google 订单验证器
        private GuruAppleValidator _appleValidator; // Apple 订单验证器
        
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
         
        private IGuruIapDataProvider _iapDataProvider;
        
        #endregion

        #region 对外回调

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
        
        

        #endregion

        #region 初始化
        
        
        /// <summary>
        /// 初始化控制器
        /// </summary>
        /// <param name="provider"></param>
        public void Initialize(IGuruIapDataProvider provider)
        {
            InitializeAsync(provider).Forget();
        }
        

        public async UniTask InitializeAsync(IGuruIapDataProvider provider)
        {
            _iapDataProvider = provider;
            _googlePlatformValidator = CreateGooglePlatformValidator(_iapDataProvider.GooglePublicKeys, _iapDataProvider.AppBundleId);
            _appleValidator = new GuruAppleValidator(_iapDataProvider.AppleRootCerts);
            _storeController = new GuruStoreController();
            
            // 初始化数据模块
            InitModel();

            _localProductSettings = _iapDataProvider.ProductSettings;
            
            // ------ Callback -----
            AddStoreCallbacks();
            
            // ------ 商店链接和初始化 -------
            await ConnectToStore();
            
            // 拉取商品
            FetchProducts();
        }

        private void AddStoreCallbacks()
        {
            _storeController.OnStoreDisconnected += OnStoreDisconnected;
            _storeController.OnProductsFetched += OnProductFetched;
            _storeController.OnProductsFetchFailed += OnProductFetchFiled;

            // ------ 支付行为回调 -------
            _storeController.OnPurchasePending += OnPurchasePending;
            _storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
            _storeController.OnPurchaseFailed += OnPurchaseFailed;
        }
        
        
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

        #region 属性设置

        /// <summary>
        /// 标记是否为付费用户
        /// </summary>
        /// <param name="value"></param>
        private void SetIsIAPUser(bool value = true)
        {
            if (_model != null && value)
            {
                _model.SetIsIapUser(true); // 用户属性
            }
            Analytics.SetIsIapUser(value);
        }

        #endregion

        #region 接口实现


        public string CurrentBuyingProductId => _curPurchasingInfo?.Product.definition.id ?? "";
        
        public void SetUID(string uid)
        {
            _storeController.SetObfuscatedAccountId(uid);
        }

        public void SetUUID(string uuid)
        {
            _storeController.SetAppleAccountId(uuid);
        }

        public void ClearData()
        {
            _model?.ClearData();
        }

        public void AddInitResultAction(Action<bool> onInitResult)
        {
            OnInitResult = onInitResult;
        }

        public void AddRestoredAction(Action<bool, string> onRestored)
        {
            OnRestored = onRestored;
        }

        public void AddPurchaseStartAction(Action<string> onBuyStart)
        {
            OnBuyStart += onBuyStart;
        }

        public void AddPurchaseEndAction(Action<string, bool> onBuyEnd)
        {
            OnBuyEnd += onBuyEnd;
        }

        public void AddPurchaseFailedAction(Action<string, string> onBuyFailed)
        {
            OnBuyFailed += onBuyFailed;
        }

        public void AddGetProductReceiptAction(Action<string, string, bool> onGetProductReceipt)
        {
            OnGetProductReceipt += onGetProductReceipt;
        }

        public void Restore()
        {
            _storeController.Restore();
        }

        public ProductInfo? GetInfo(string productName)
        {
            return _productMap.Count == 0 ? null : 
                _productMap.Values.FirstOrDefault(c => c.Name == productName);
        }
        
        public ProductInfo? GetInfoById(string productId)
        {
            return _productMap.Count == 0 ? null 
                : _productMap.Values.FirstOrDefault(c => c.Id == productId);
        }

        public bool IsProductHasReceipt(string productName)
        {
            throw new NotImplementedException();
        }
        
        #endregion

        
        #region 数据查询
        
        /// <summary>
        /// ProductName 转 ProductId
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        private string? ProductNameToId(string productName)
        {
            var product = GetProductByName(productName);
            return product?.definition.id ?? null;
        }

        /// <summary>
        /// ProductId 转 ProductName
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        private string? ProductIdToName(string productId)
        {
            foreach (var info in _productMap.Values)
            {
                if(info == null) continue;
                if(info.Product == null) continue;
                if(info.Product.definition == null) continue;
                
                if (info.Product.definition.id == productId)
                {
                    return info.Name;
                }
            }
            return null;
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

        public ProductInfo[] GetAllProductInfos()
        {
            throw new NotImplementedException();
        }

        #endregion
        
        #region 商店连接（启动）

        /// <summary>
        /// 链接商店
        /// </summary>
        private async UniTask<bool> ConnectToStore()
        {
            _connectFailedFlag = false;

            await _storeController.Connect();
            // _storeController.SetStoreReconnectionRetryPolicyOnDisconnection(new ExponentialBackOffRetryPolicy());
            // await _storeController.Connect();
            await UniTask.DelayFrame(1); // 等待 1 帧， 确认商店链接结果
            
            bool result = !_connectFailedFlag;
            return result;
        }

        /// <summary>
        /// 获取重试等地时间
        /// </summary>
        /// <returns></returns>
        private float GetConnectRetryDelaySeconds()
        {
            float value = Mathf.Pow(2f, _storeConnectRetryTimes++);
            return Mathf.Clamp(value, 3f, 180f);
        }
        
        
        /// <summary>
        /// 商店链接失败
        /// </summary>
        /// <param name="disConnectInfo"></param>
        private void OnStoreDisconnected(StoreConnectionFailureDescription disConnectInfo)
        {
            _connectFailedFlag = true;
            Debug.Log($"[IAP] --- OnStoreDisconnected: {disConnectInfo.Message} and IsRetryable: {disConnectInfo.IsRetryable} ");
            
        }

        

        #endregion
        
        #region 商品拉取

        
        /// <summary>
        /// 获取商品品配置列表
        /// </summary>
        /// <returns></returns>
        protected virtual ProductSetting[] GetProductSettings()
            => GuruSettings.Instance.Products;
        
        // 开始拉取所有的商品
        private void FetchProducts()
        {
            var productDefines = new List<ProductDefinition>();
            // 注入初始商品产品列表
            var settings = GetProductSettings();
            foreach (var product in settings)
            {
                productDefines.Add(new ProductDefinition(product.ProductId, product.Type));
            }
            // 设置拉取和重试逻辑
            _storeController.FetchProducts(productDefines, new ExponentialBackOffRetryPolicy());
        }
        
        /// <summary>
        /// 产品拉取成功
        /// </summary>
        /// <param name="products"></param>
        private void OnProductFetched(List<Product> products)
        {
            _fetchedProducts = products;
            BuildProductInfoMap(products);
            
            // TODO: 广播拉取到商品的事件

            Debug.Log($"---- Products Fetched -----");
            foreach (var p in products)
            {
                string msg = $"[IAP] --- [{p.definition.id}] : {p.metadata.localizedPrice}";
                Debug.Log(msg);
            }
            Debug.Log($"---- Products Fetched END-----");
            
            OnInitResult?.Invoke(true);
        }
        
        // 拉取失败逻辑
        private void OnProductFetchFiled(ProductFetchFailed failedInfo)
        {
            Debug.LogError($"Fetch product failed reason: {failedInfo.FailureReason}");
            foreach (var failedProduct in failedInfo.FailedFetchProducts)
            {
                // TODO: 处理拉取失败逻辑：
                Debug.LogError($"[IAP] ---  failed product: {failedProduct.id}");
            }
            
            OnInitResult?.Invoke(false);
        }


        private void BuildProductInfoMap(List<Product> products)
        {
            if (_productMap == null)
            {
                _productMap = new Dictionary<string, ProductInfo>();
            }
            else
            {
                _productMap.Clear();
            }

            // 构建支付地图
            foreach (var setting in _localProductSettings)
            {
                var info = new ProductInfo(setting);
                var p = products.FirstOrDefault(c => c.definition.id == setting.ProductId);
                if (p != null)
                    info.SetProduct(p);
                
                _productMap[setting.ProductName] = info;

            }
        }

        /// <summary>
        /// 获得 Product 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public Product? GetProduct(string productId) 
            => _fetchedProducts?.FirstOrDefault(c => c.definition.id == productId) ?? null;
        
        /// <summary>
        /// 获取 Product
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        private Product? GetProductByName(string productName)
        {
            return _productMap.TryGetValue(productName, out var info) ? info.Product : null;
        }

        #endregion
        
        #region 支付流程

        /// <summary>
        /// 支付接口
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="category"></param>
        public void Purchase(string productName, string? category = null)
        {
            var info = GetInfo(productName);
            if (info == null)
            {
                Debug.LogError($"[IAP] --- Product info is null with name: {productName}");
                OnBuyEnd(productName, false);
                return;
            }

            PurchaseByInfo(info, category);
        }

        /// <summary>
        /// 支付 by Id
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="category"></param>
        public void PurchaseWithId(string productId, string? category = null)
        {
            var info = GetInfoById(productId);
            if (info == null)
            {
                Debug.LogError($"[IAP] --- Product info is null with id: {productId}");
                OnBuyEnd(productId, false);
                return;
            }

            PurchaseByInfo(info, category);
        }
        
        // 支付主要逻辑
        private void PurchaseByInfo(ProductInfo info, string? category = null)
        {
            var productName = info.Name;
            OnBuyStart.Invoke(productName);
            category ??= info.Category;
            _curProductCategory = category;
            _curPurchasingInfo = info;
            var product = info.Product;
            if (product == null)
            {
                // TODO： 支付失败
                OnBuyEnd(productName, false);
                return;
            }
            
            // 调用商店支付
            _storeController.Purchase(new Cart(new CartItem(product)));
        }
        
        /// <summary>
        /// 支付失败
        /// </summary>
        /// <param name="failedOrder"></param>
        private void OnPurchaseFailed(FailedOrder failedOrder)
        {
            var reasonStr = failedOrder.FailureReason.ToString();
            
            // var transID = failedOrder.Info.TransactionID;
            var failedProducts = failedOrder.CartOrdered.Items().Select(c => c.Product).ToList();
            var productName = UnknownProductName;
            if (failedProducts.Count > 0)
            {
                var product = failedProducts[0];
                var info = GetInfoById(product.definition.id);
                if(!string.IsNullOrEmpty(info?.Name ?? null))
                    productName = info.Name;
                
                //上报点位，用户购买失败的原因
                Analytics.IAPRetFalse(_curProductCategory, product.definition.id, reasonStr);
            }
            
            Debug.Log($"{Tag} --- OnPurchaseFailed :: Reason = {reasonStr}");
            // 失败的处理逻辑
            OnBuyEnd.Invoke(productName, false);
            // 失败原因
            OnBuyFailed.Invoke(productName, reasonStr);
            ClearCurPurchasingProduct(); // 清除购买缓存


        }

        /// <summary>
        /// 支付成功
        /// </summary>
        /// <param name="order"></param>
        private void OnPurchaseConfirmed(Order order)
        {
            var transId = order.Info.TransactionID;
            var receipt =  order.Info.Receipt;
            var productId = order.Info.PurchasedProductInfo.First().productId;
            var info = GetInfoById(productId);
            var productName = UnknownProductName;
            bool success = false;
            
            Debug.Log($"[IAP] --- OnPurchaseConfirmed");
            
            if (info == null)
            {
                string msg = $"{Tag} ---  Purchased end, but can't find ProductInfo with ID: {productId}";
                Debug.LogError(msg);
            }
            else
            {
                productName = info.Name;
                success = true;
                SetIsIAPUser(true); // 设置用户属性标记
                
                Debug.Log($"{Tag} --- OnPurchaseSuccess :: purchase count: {PurchaseCount}  productName: {productName}");

                bool isRestored = true;
                if (_curPurchasingInfo != null)
                {
                    // 当前支付中的 Product 不为空
                    ReportPurchasedOrder(order); // 订单上报
                    
                    // 真实购买后上报对应的事件
                    if (IsFirstIAP) {
                        // 上报首次支付打点
                        Analytics.FirstIAP(info.Id, info.Price, info.CurrencyCode); 
                    }
                    Analytics.ProductIAP(info.Id,info.Id, info.Price, info.CurrencyCode);
                    isRestored = false;
                }
                
                if (string.IsNullOrEmpty(receipt))
                {
                    string msg = $"{Tag} ---  Purchased product is null or has no receipt!!";
                    Debug.LogError(msg);
                    Analytics.LogCrashlytics(new Exception(msg));
                }
                else
                {
                    var appleProductIsRestored = false;
                    OnGetProductReceipt?.Invoke(productId, receipt, isRestored);
                }
                
                PurchaseCount++; // 记录支付次数
            }
            
            Debug.Log($"{Tag} --- Call OnBuyEnd [{productName}] :: {OnBuyEnd}");
            OnBuyEnd.Invoke(productName, success);
            ClearCurPurchasingProduct(); // 清除购买缓存
        }

        private void OnPurchasePending(PendingOrder pendingOrder)
        {
            Debug.Log($"[IAP] --- OnPurchasePending");
            
            if(_curPurchasingInfo == null)
            {
                Debug.LogError("[IAP] Can not find Current Purchasing Info...");
                return;
            }

            Debug.Log($"[IAP] --- ConfirmPurchase PendingOrder {pendingOrder.Info.TransactionID}" );
            _storeController.ConfirmPurchase(pendingOrder);

            // foreach (var item in pendingOrder.CartOrdered.Items())
            // {
            //     
            // }

            // var receipt = pendingOrder.Info.Receipt;
            // var productId = pendingOrder.Info.PurchasedProductInfo.First().productId;
            // var info = GetInfoById(productId);
            // var productName = info?.Name ?? UnknownProductName;
        }

        /// <summary>
        /// 获取订单上报结果
        /// </summary>
        /// <param name="order"></param>
        /// <param name="purchasedProductInfos"></param>
        private void FetchOrderResult(Order order, out List<IPurchasedProductInfo> purchasedProductInfos)
        {
            List<Product> buyingProducts = new List<Product>();
            List<string> buyingProductIds = new List<string>();
            
            purchasedProductInfos = new List<IPurchasedProductInfo>();

            foreach (var item in order.CartOrdered.Items())
            {
                if (item?.Product != null)
                {
                    buyingProducts.Add(item.Product);
                    buyingProductIds.Add(item.Product.definition.id);
                }
            }

            if (buyingProducts.Count == 0)
            {
                return;
            }

            foreach (var productInfo in order.Info.PurchasedProductInfo)
            {
                if (buyingProductIds.Contains(productInfo.productId))
                {
                    // 确定是支付中的 Product
                    purchasedProductInfos.Add(productInfo);
                }
            }
            
        }


        private void ReportPurchasedOrder(Order order)
        {
            //---------------- All Report Information --------------------
            int level = _iapDataProvider.BLevel;
            string? scene = _curProductCategory ?? DefaultCategory;

            if (Application.platform == RuntimePlatform.Android)
            {
                ValidateGoogleOrder(order.Info.Receipt, level, scene);
            }
            
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                ValidateAppleOrder(order, level, scene, _iapDataProvider.IDFV, _iapDataProvider.AppBundleId);
            }

        }



        private void ClearCurPurchasingProduct()
        {
            _curPurchasingInfo = null;
            _curProductCategory = "";
        }
        #endregion

        #region 订单验证
        
        /// <summary>
        /// 创建Google订单校验器
        /// </summary>
        /// <param name="googlePublicKeys"></param>
        /// <param name="googleBundleId"></param>
        /// <returns></returns>
        private CrossPlatformValidator CreateGooglePlatformValidator(byte[] googlePublicKeys, string googleBundleId)
        {
            var validator = new CrossPlatformValidator(googlePublicKeys, null, googleBundleId);
            return validator;
        }


        /// <summary>
        /// 验证上报 Google 订单
        /// </summary>
        /// <param name="receipt"></param>
        /// <param name="orderType"></param>
        /// <param name="level"></param>
        /// <param name="userCurrency"></param>
        /// <param name="payPrice"></param>
        /// <param name="scene"></param>
        /// <param name="isFree"></param>
        private void ValidateGoogleOrder(string receipt, int level, string scene)
        {
            var allReceipts = _googlePlatformValidator.Validate(receipt);
            string offerId = ""; // 默认无法解析
            string basePlanId = ""; // 默认无法解析
            
            // ---- Android 订单验证, 上报打点信息 ---- 
            foreach (var r in allReceipts)
            {
                if (r is GooglePlayReceipt googleReceipt)
                {
                    var productId = googleReceipt.productID;
                    var googleParseResult = FetchProductDetails(productId, 
                        out var orderType, 
                        out var userCurrency, 
                        out var payPrice, 
                        out var isFree);
                    
                    if(!googleParseResult) Debug.LogError($"[IAP] --- Parse Purchased Product details for product: {productId} is Failed");
                    
                    ReportGoogleOrder(orderType, 
                        googleReceipt.productID,
                        googleReceipt.purchaseToken,
                        googleReceipt.orderID,
                        googleReceipt.purchaseDate, 
                        level, 
                        userCurrency, 
                        payPrice, 
                        scene, 
                        isFree,
                        offerId,  
                        basePlanId);
                }
            }
        }

        /// <summary>
        /// 获取产品的详细信息
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="orderType"></param>
        /// <param name="userCurrency"></param>
        /// <param name="payPrice"></param>
        /// <param name="isFree"></param>
        /// <returns></returns>
        private bool FetchProductDetails(string? productId, out int orderType, out string userCurrency, out double payPrice, out bool isFree)
        {
            orderType = 0; // 0:iap  1: sub
            userCurrency = "USD";
            payPrice = 0.0;
            isFree = false; // 基本没有项目设置试用
            
            if (productId == null || string.IsNullOrEmpty(productId)) return false;
            
            var product = GetProduct(productId);
            if (product == null) return false;
            
            orderType = product.definition.type == ProductType.Subscription ? 1 : 0;
            payPrice = decimal.ToDouble(product.metadata.localizedPrice);
            userCurrency = product.metadata.isoCurrencyCode;
            
            return true;
        }


        private void ValidateAppleOrder(Order order,
            int level, string scene, string idfv, string appBundleId)
        {

            var receipt = order.Info.Receipt;
            // TODO: fixme:: 这里需要检查一下是否是真正购买的道具 ID
            // 实现上只取了第一个
            var purchasedProductId = order.Info.PurchasedProductInfo.First().productId; 
            
            
            string appleReceiptString = "";
            // ---- iOS 订单验证, 上报打点信息 ----
            var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(receipt);
            if (jsonData != null && jsonData.TryGetValue("Payload", out var recp))
            {
                appleReceiptString = recp.ToString();
                // Debug.Log($"--- [{productId}] iOS receipt: {appleReceiptString}");      
            }
            
            var appleReceipts = _appleValidator.Validate(receipt);
            Debug.Log($"[IAP] --- Full receipt: \n{receipt}");
            foreach (var r in appleReceipts)
            {
                if (r is AppleInAppPurchaseReceipt appleReceipt
                    && r.productID == purchasedProductId)  
                {
                    
                    var parseResult = FetchProductDetails(r.productID, 
                        out var orderType, 
                        out var userCurrency, 
                        out var payPrice, 
                        out var isFree);

                    if (!parseResult)
                    {
                        Debug.LogError($"[IAP] --- [{r.productID}] iOS product parse failed");   
                    }

                    ReportAppleOrder(orderType, 
                        appleReceipt.productID,
                        appleReceiptString,
                        appleReceipt.transactionID,
                        appleReceipt.purchaseDate, 
                        level, 
                        userCurrency, 
                        payPrice, 
                        scene, 
                        appBundleId,
                        idfv,
                        isFree);
            
                    break;
                }
            }
        }




        #endregion
        
        #region 订单上报队列
        
        private const int OrderRequestTimeout = 10;
        private const int OrderRequestRetryTimes = 3;
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


        #region 订阅接口

        
        public bool IsSubscriptionCancelled(string productName)
        {
            throw new NotImplementedException();
        }

        public bool IsSubscriptionAvailable(string productName)
        {
            throw new NotImplementedException();
        }

        public bool IsSubscriptionExpired(string productName)
        {
            throw new NotImplementedException();
        }

        public bool IsSubscriptionFreeTrail(string productName)
        {
            throw new NotImplementedException();
        }

        public bool IsSubscriptionAutoRenewing(string productName)
        {
            throw new NotImplementedException();
        }

        public bool IsSubscriptionIntroductoryPricePeriod(string productName)
        {
            throw new NotImplementedException();
        }

        public DateTime GetSubscriptionExpireDate(string productName)
        {
            throw new NotImplementedException();
        }

        public DateTime GetSubscriptionPurchaseDate(string productName)
        {
            throw new NotImplementedException();
        }

        public DateTime GetSubscriptionCancelDate(string productName)
        {
            throw new NotImplementedException();
        }

        public TimeSpan GetSubscriptionRemainingTime(string productName)
        {
            throw new NotImplementedException();
        }

        public TimeSpan GetSubscriptionIntroductoryPricePeriod(string productName)
        {
            throw new NotImplementedException();
        }

        public TimeSpan GetSubscriptionFreeTrialPeriod(string productName)
        {
            throw new NotImplementedException();
        }

        public string GetSubscriptionInfoJsonString(string productName)
        {
            throw new NotImplementedException();
        }

        #endregion

    }


    /// <summary>
    /// Guru 自有的商店控制器
    /// </summary>
    internal class GuruStoreController
    {
        private IStoreService _storeService;
        private IPurchaseService _purchaseService;
        private IProductService _productService;


        #region Callbacks
        // Connection
        internal event Action<StoreConnectionFailureDescription> OnStoreDisconnected;
        // Fetch
        internal event Action<List<Product>> OnProductsFetched;
        internal event Action<ProductFetchFailed> OnProductsFetchFailed;
    

        // ------ 支付行为回调 -------
        internal event Action<PendingOrder> OnPurchasePending;
        internal event Action<ConfirmedOrder> OnPurchaseConfirmed;
        internal event Action<FailedOrder> OnPurchaseFailed;
        internal event Action<Entitlement> OnCheckEntitlement;
        internal event Action<PurchasesFetchFailureDescription> FetchPurchasesFailed;
        internal event Action<DeferredOrder> OnPurchaseDeferred;

        #endregion

        private bool _isConnectFailed;
        
        
        
        internal string GetStoreName()
        {

#if UNITY_ANDROID
                return GooglePlay.Name;
#elif UNITY_IOS
                return AppleAppStore.Name;   
#endif
                return "FakeStore";
       
        }


        public GuruStoreController()
        {
            _storeService = UnityIAPServices.Store(GetStoreName());
            _purchaseService = UnityIAPServices.Purchase(GetStoreName());
            _productService = UnityIAPServices.Product(GetStoreName());
            
            // Connection
            var retryPolicy = new ExponentialBackOffRetryPolicy();
            _storeService.SetStoreReconnectionRetryPolicyOnDisconnection(retryPolicy);
            _storeService.AddOnStoreDisconnectedAction(OnStoreDisconnectedAction);
            
            // Fetch
            _productService.AddProductsUpdatedAction(OnProductsUpdatedAction);
            _productService.AddProductsFetchFailedAction(OnProductsFetchFailedAction);
            
            // Purchase
            _purchaseService.AddFetchPurchasesFailedAction(OnFetchPurchasesFailedAction);
            _purchaseService.AddCheckEntitlementAction(OnCheckEntitlementAction);
            _purchaseService.AddPurchaseFailedAction(OnPurchaseFailedAction);
            _purchaseService.AddPurchaseDeferredAction(OnPurchaseDeferredAction);
            _purchaseService.AddConfirmedOrderUpdatedAction(OnConfirmedOrderUpdatedAction);
            _purchaseService.AddPendingOrderUpdatedAction(OnPendingOrderUpdatedAction);
            _purchaseService.AddFetchedPurchasesAction(OnFetchedPurchasesAction);
        }

        

        public void Dispose()
        {
            _storeService.RemoveOnStoreDisconnectedAction(OnStoreDisconnectedAction);
            
            _productService.RemoveProductsUpdatedAction(OnProductsUpdatedAction);
            _productService.RemoveProductsFetchFailedAction(OnProductsFetchFailedAction);
            
            
        }


        private void OnStoreDisconnectedAction(StoreConnectionFailureDescription failureDescription)
        {
            _isConnectFailed = true;
            OnStoreDisconnected.Invoke(failureDescription);
        }

        #region Extension


        public void SetObfuscatedAccountId(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                Debug.LogError($"[IPA] Wrong format of google uid: {uid}");
                return;
            }
            
            _storeService.Google?.SetObfuscatedAccountId(uid);
        }


        public void SetAppleAccountId(string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                Debug.LogError($"[IPA] Wrong format of apple uuid: {uuid}");
                return;
            }

            var guid = Guid.Parse(uuid);
            _storeService.Apple?.SetAppAccountToken(guid);
        }

        #endregion
        
        #region StoreService

        // 连接商店
        public async UniTask Connect()
        {
            _isConnectFailed = false;
            await _storeService.ConnectAsync().AsUniTask();
            await UniTask.DelayFrame(1);
            while (_isConnectFailed)
            {
                await Connect(); // retry connect
            }
            Debug.Log($"[IPA] Connected to Google Store successfully");
            
            // 理解
            _purchaseService.FetchPurchases();
        }


        #endregion

        #region ProductService

        public void FetchProducts(List<ProductDefinition> definitions, IRetryPolicy? retryPolicy = null)
        {
            retryPolicy ??= new ExponentialBackOffRetryPolicy();
            _productService.FetchProducts(definitions, retryPolicy);
        }


        private void OnProductsUpdatedAction(List<Product> products)
        {
            OnProductsFetched.Invoke(products);
        }

        private void OnProductsFetchFailedAction(ProductFetchFailed productFetchFailed)
        {
            OnProductsFetchFailed.Invoke(productFetchFailed);
        }

        #endregion

        #region PuchaseService

        public void Purchase(ICart cart)
        {
            _purchaseService.Purchase(cart);
        }

        public void ConfirmPurchase(PendingOrder pendingOrder)
        {
            Debug.Log($"[IAP] GuruStoreController --- Confirm PendingOrder");
            _purchaseService.ConfirmOrder(pendingOrder); // 非消耗品调用后不会进行处理
        }

        private void OnFetchedPurchasesAction(Orders orders)
        {
            if (orders.PendingOrders.Count > 0)
            {
                foreach (var pendingOrder in orders.PendingOrders)
                {
                    //TODO: 完善逻辑
                }
            }

            if (orders.ConfirmedOrders.Count > 0)
            {
                foreach (var confirmedOrder in orders.ConfirmedOrders)
                {
                    //TODO: 完善逻辑
                }
            }
        }


        private void OnPendingOrderUpdatedAction(PendingOrder pendingOrder)
        {
            OnPurchasePending(pendingOrder);
        }

        private void OnConfirmedOrderUpdatedAction(ConfirmedOrder confirmedOrder)
            => OnPurchaseConfirmed(confirmedOrder);

        private void OnPurchaseDeferredAction(DeferredOrder deferredOrder) 
            => OnPurchaseDeferred(deferredOrder);

        private void OnPurchaseFailedAction(FailedOrder failedOrder) 
            => OnPurchaseFailed(failedOrder);
        
        private void OnCheckEntitlementAction(Entitlement entitlement) 
            => OnCheckEntitlement.Invoke(entitlement);
        
        private void OnFetchPurchasesFailedAction(PurchasesFetchFailureDescription failureDescription) 
            => FetchPurchasesFailed.Invoke(failureDescription);
        

        #endregion

        #region Restore

        // TODO: 恢复购买的逻辑需要好好检查一下
        
        /// <summary>
        /// 恢复购买
        /// </summary>
        public void Restore()
        {
            _purchaseService.FetchPurchases();
        }


        #endregion
    }
    
}

#endif