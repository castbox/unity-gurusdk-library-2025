namespace Guru
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using System.Linq;
	using Guru.IAP;
	
	//游戏通用模版打点定义
    public partial class Analytics
    {
	    #region 游戏通用打点

	    /// <summary>
	    /// 当玩家在游戏中升级时触发
	    /// </summary>
	    /// <param name="level">level （等级）从1开始 标准点</param>
	    /// <param name="character">升级的角色，如果没有可不选</param>
	    /// <param name="extra"></param>
	    public static void LevelUp(int level, string character, Dictionary<string, object> extra = null)
        {
	        var dict = new Dictionary<string, object>()
	        {
		        { ParameterLevel, level },
		        { ParameterCharacter, character }
	        };
	        var data = DictionaryUtil.Merge(dict, extra);

	        TrackEvent(TrackingEvent.Create(EventLevelUp, data));
            
        }

	    /// <summary>
	    /// 玩家已完成解锁成就时触发。
	    /// </summary>
	    /// <param name="achievementID">这里的成就ID值项目方自行定义</param>
	    /// <param name="extra"></param>
	    public static void UnlockAchievement(string achievementID, Dictionary<string, object> extra = null)
        {
	        var dict = new Dictionary<string, object>()
	        {
		        { ParameterAchievementId, achievementID },
	        };
	        var data = DictionaryUtil.Merge(dict, extra);
            TrackEvent(TrackingEvent.Create(EventUnlockAchievement, data));
        }
	    
        /// <summary>
        /// 玩家已开始挑战某个关卡时触发。
        /// </summary>
        /// <param name="level">关卡数</param>
        /// <param name="levelName">关卡名称</param>
        /// <param name="levelType">关卡类型</param>
        /// <param name="itemId">关卡配置表 ID</param>
        /// <param name="startType">启动方式</param>
        /// <param name="isReplay">是否是重玩</param>
        /// <param name="extra">额外数据</param>
        public static void LogLevelStart(int level, string levelName, 
	        string levelType = "main", string itemId = "", string startType = "play", bool isReplay = false, 
	        Dictionary<string, object> extra = null)
        {
	        Dictionary<string, object> dict = new Dictionary<string, object>()
	        {
		        { ParameterLevel, level },
		        { ParameterLevelName, levelName },
		        { ParameterItemCategory, levelType },
		        { ParameterStartType, startType },
		        { ParameterReplay, isReplay ? "true" : "false" },
	        };
	        if(!string.IsNullOrEmpty(itemId))
		        dict[ParameterItemId] = itemId;

	        var data = DictionaryUtil.Merge(dict, extra);
	        TrackEvent(TrackingEvent.Create(EventLevelStart, data));
        }
        
		/// <summary>
		/// 关卡结束(Firebase标准事件)
		/// </summary>
		/// <param name="level"></param>
		/// <param name="result"></param>
		/// <param name="levelName"></param>
		/// <param name="levelType"></param>
		/// <param name="itemId"></param>
		/// <param name="duration"></param>
		/// <param name="step"></param>
		/// <param name="score"></param>
		/// <param name="extra"></param>
		public static void LogLevelEnd(int level, string result, 
			string levelName = "", string levelType = "main", string itemId = "", 
			int duration = 0, int? step = null, int? score = null, Dictionary<string, object> extra = null)
		{
			bool isSuccess = result.Equals("success");

			var dict = new Dictionary<string, object>()
			{
				[ParameterLevel] = level,
				[ParameterLevelName] = levelName,
				[ParameterItemCategory] = levelType,
				[ParameterSuccess] = isSuccess ? "true" : "false",
				[ParameterResult] = result,
				[ParameterDuration] = duration
			};

			if(!string.IsNullOrEmpty(itemId))
				dict[ParameterItemId] = itemId;
			if(step != null)
				dict[ParameterStep] = step.Value;
			if(score != null)
				dict[ParameterScore] = score.Value;
			
			var data = DictionaryUtil.Merge(dict, extra);
			TrackEvent(TrackingEvent.Create(EventLevelEnd, data));
		}


		/// <summary>
		/// 新用户通过第几关（仅记录前n关,根据项目自行确定，不区分关卡类型）[买量用]
		/// </summary>
		/// <param name="evt"></param>
		public static void TrackLevelEndSuccessEvent(LevelEndSuccessEvent evt)
		{
			if (evt.level > GuruSettings.Instance.AnalyticsSetting.LevelEndSuccessNum)
				return;
			TrackEvent(evt);
		}

		/// <summary>
		/// 第一次通关打点
		/// </summary>
		public static void LevelFirstEnd(string levelType, string levelName, int level, 
			string result, int duration = 0, Dictionary<string, object> extra = null)
		{
			var dict = new Dictionary<string, object>()
			{
				{ ParameterItemCategory, levelType },
				{ ParameterLevelName, levelName },
				{ ParameterLevel, level },
				{ ParameterSuccess, result == "success" ? 1 : 0 },
				{ ParameterResult, result },
			};
			if (duration > 0)
				dict[ParameterDuration] = duration;

			var data = DictionaryUtil.Merge(dict, extra);
			TrackEvent(TrackingEvent.Create(EventLevelFirstEnd, data));
		}
		
	    #endregion
	    
	    #region Tch 太极打点逻辑

	    public const double TCH_02_VALUE = 0.2d; // tch_02 设定值
	    public const double TCH_001_VALUE = 0.01d; // tch_001 设定值
	    private const double TCH_001_PROTECTED_Value = 5.0d; // 预设保护值, 如果大于这个值, 算作异常上报
	    public static string IAPPlatform
	    {
		    get
		    {
#if UNITY_IOS
			    return "appstore";
#endif
			    return "google_play";
		    }
	    }
	    
	    private static TchFbModeEnum _tchFbMode = TchFbModeEnum.Mode001;
	    // 太极事件上报事件模式 
	    // FB_TCH_MODE_001 (1): 在 tch_001 的时候上报收入事件至 facebook
	    // FB_TCH_MODE_02 (2): 在 tch_02 的时候上报收入事件至 facebook
	    public static TchFbModeEnum TchFbMode
	    {
		    get => _tchFbMode;
		    set
		    {
			    if (value is TchFbModeEnum.Mode001 or TchFbModeEnum.Mode02)
			    {
				    _tchFbMode = value;
			    }
		    }
	    }

	    /// <summary>
	    /// 太极001 IAP收入
	    /// 每发生一次iap收入，触发一次该事件，value值为本次iap的收入值；
	    /// </summary>
	    /// <param name="value">中台返回地收入值</param>
	    /// <param name="productId"></param>
	    /// <param name="orderId"></param>
	    /// <param name="orderType"></param>
	    /// <param name="timestamp"></param>
	    /// <param name="isTest"></param>
	    public static void Tch001IAPRev(double value, string productId, string orderId, string orderType, string timestamp, bool isTest = false)
	    {
		    string sandbox = isTest ? "true" : "false";
		    TchRevenueEvent(EventTchAdRev001Impression, IAPPlatform, value, orderType, productId, orderId, timestamp, sandbox);
	    }

	    /// <summary>
	    /// 太极02 IAP 收入打点
	    /// 发生一次iap收入，触发一次该事件，value值为本次iap的收入值；
	    /// </summary>
	    /// <param name="value"></param>
	    /// <param name="productId"></param>
	    /// <param name="orderId"></param>
	    /// <param name="orderType"></param>
	    /// <param name="timestamp"></param>
	    /// <param name="isTest"></param>
	    // public static void Tch02IAPRev(double value, string productId, string orderId, string orderType, string timestamp)
	    public static void Tch02IAPRev(double value, string productId, string orderId, string orderType, string timestamp, bool isTest = false)
	    {
		    string sandbox = isTest ? "true" : "false";
		    TchRevenueEvent(EventTchAdRev02Impression, IAPPlatform, value, orderType, productId, orderId, timestamp, sandbox);
	    }

	    /// <summary>
	    /// 广告收入累计超过0.01美元，触发一次该事件，重新清零后，开始下一次累计计算；
	    /// </summary>
	    /// <param name="value"></param>
	    /// <param name="mediationName"></param>
	    public static void Tch001ADRev(double value, string mediationName)
	    {
		    if (value > TCH_001_PROTECTED_Value)
		    {
			    TchAdAbnormalEvent(value, mediationName); // 上报异常值
			    return;
		    }
		    
		    // if (value < Tch001TargetValue) value = Tch001TargetValue;  // TCH广告添加0值校验修复, 不得小于0.01
		    TchRevenueEvent(EventTchAdRev001Impression, mediationName, value);

		    if (TchFbMode == TchFbModeEnum.Mode001) 
		    {
			    Debug.Log($"{TAG} --- Report Tch AdRev 001: {value}");
			    FBPurchase(value, USD, "ads", mediationName);
		    }
	    }

	    /// <summary>
	    /// 广告收入累计超过0.2美元，触发一次该事件，重新清零后，开始下一次累计计算；
	    /// </summary>
	    /// <param name="value"></param>
	    /// <param name="mediationName"></param>
	    public static void Tch02ADRev(double value, string mediationName)
	    {
		    TchRevenueEvent(EventTchAdRev02Impression, mediationName, value);

		    if (TchFbMode == TchFbModeEnum.Mode02)
		    {
			    Debug.Log($"{TAG} --- Report Tch AdRev 02: {value}");
			    FBPurchase(value, USD, "ads", mediationName);
		    }
	    }

	    /// <summary>
	    /// 太极事件点位上报
	    /// </summary>
	    /// <param name="eventName"></param>
	    /// <param name="platform"></param>
	    /// <param name="value"></param>
	    /// <param name="orderType"></param>
	    /// <param name="productId"></param>
	    /// <param name="orderId"></param>
	    /// <param name="timestamp"></param>
	    /// <param name="sandbox"></param>
	    private static void TchRevenueEvent(string eventName, string platform, double value, 
		    string orderType = "", string productId = "", string orderId = "", string timestamp = "", string sandbox = "")
	    {
		    var evt = new TchRevenueEvent(eventName, platform, value, productId, 
			    orderId, orderType, timestamp, sandbox, USD);
		    TrackEvent(evt);
	    }

		/// <summary>
		/// FBPurchase 打点
		/// </summary>
		/// <param name="revenue"></param>
		/// <param name="currency"></param>
		/// <param name="type"></param>
		/// <param name="platform"></param>
	    private static void FBPurchase(double revenue, string currency, string type, string platform)
	    {
		    var evt = new FBPurchaseEvent(revenue, currency, type, platform);
		    TrackEvent(evt);
	    }


	    private static void TchAdAbnormalEvent(double value, string platform)
	    {
		    var dict = new Dictionary<string, object>()
		    {
			    [ParameterItemCategory] = EventTchAdRevAbnormal,
			    [ParameterAdPlatform] = platform,
			    [ParameterCurrency] = USD,
			    [ParameterValue] = value,
		    };
		    LogDevAudit(dict);
	    }

	    #endregion

	    #region Analytics Game IAP 游戏内购打点
	    
	    /// <summary>
	    /// 当付费页面打开时调用（iap_imp）
	    /// </summary>
	    /// <param name="scene">界面跳转的来源</param>
	    /// <param name="extra">扩展参数</param>
	    public static void IAPImp(string scene, Dictionary<string, object> extra = null)
	    {
		    var dict = new Dictionary<string, object>()
		    {
			    { ParameterItemCategory, scene },
		    };
	
		    var data = DictionaryUtil.Merge(dict, extra);
		    TrackEvent(TrackingEvent.Create(EventIAPImp, data));
	    }
	    
	    /// <summary>
	    /// 当付费页面关闭时调用（iap_close）
	    /// </summary>
	    /// <param name="scene"></param>
	    /// <param name="extra"></param>
	    public static void IAPClose(string scene, Dictionary<string, object> extra = null)
	    {
		    var dict = new Dictionary<string, object>()
		    {
			    { ParameterItemCategory, scene },
		    };
		    
		    var data = DictionaryUtil.Merge(dict, extra);
		    TrackEvent(TrackingEvent.Create(EventIAPClose, data));
	    }

	    /// <summary>
	    /// 点击付费按钮时调用 (iap_clk)
	    /// </summary>
	    /// <param name="scene">支付场景</param>
	    /// <param name="productId">道具的 sku</param>
	    /// <param name="basePlanId">offer 的 basePlanId</param>
	    /// <param name="offerId">offer 的 offerId</param>
	    /// <param name="extra">扩展参数</param>
	    public static void IAPClick(string scene, string productId, string basePlanId = "", string offerId = "", Dictionary<string, object> extra = null)
	    {
		    string sku = productId;
		    if (!string.IsNullOrEmpty(offerId) && !string.IsNullOrEmpty(basePlanId))
		    {
			    sku = $"{productId}:{basePlanId}:{offerId}"; // 上报三连 ID
		    }
		    var dict = new Dictionary<string, object>()
		    {
			    { ParameterItemCategory, scene },
			    { ParameterItemName, sku },
			    { ParameterProductId, sku },
		    };
		    
		    var data = DictionaryUtil.Merge(dict, extra);
		    TrackEvent(TrackingEvent.Create(EventIAPClick, data));
	    }

	    /// <summary>
	    /// "app 内弹出的付费引导IAP付费或试用成功打点"
	    /// </summary>
	    /// <param name="scene">界面跳转的来源</param>
	    /// <param name="productId">product id,多个产品用逗号分隔，第一个商品id放主推商品id</param>
	    /// <param name="value">产品的价格</param>
	    /// <param name="currency">用户的付费币种</param>
	    /// <param name="orderId">订单 ID</param>
	    /// <param name="type">付费类型订阅/产品（subscription/product）</param>
	    /// <param name="isFree">是否为试用（1：试用，0：付费）</param>
	    /// <param name="offerId">若存在 Offer 的话需要上报 OfferID</param>
	    private static void IAPRetTrue(string scene, string productId, double value, string currency, string orderId, string type, bool isFree = false, string offerId = "")
	    {
		    var dict = new Dictionary<string, object>()
		    {
			    { ParameterItemCategory, scene },
			    { ParameterItemName, productId },
			    { ParameterProductId, productId }, // new parameter, will replace with item_name
			    { ParameterValue, value },
			    { ParameterCurrency, currency },
			    { "order_id", orderId },
			    { "type", type },
			    { "isfree", isFree ? "1" : "0" },
		    };
		
		    if(!string.IsNullOrEmpty(offerId))
			    dict["basePlan"] = offerId;
		    
		    var evt = new TrackingEvent(EventIAPReturnTrue, dict, new EventSetting()
		    {
			    EnableFirebaseAnalytics = true,
			    EnableGuruAnalytics = true,
			    EnableAdjustAnalytics = true,
		    });
		    TrackEvent(evt);
	    }

	    /// <summary>
	    /// "app 内弹出的付费引导IAP付费或试用失败打点"
	    /// </summary>
	    /// <param name="itemCategory">界面跳转的来源</param>
	    /// <param name="productId">product id,多个产品用逗号分隔，第一个商品id放主推商品id</param>
	    /// <param name="failReason"></param>
	    internal static void IAPRetFalse(string itemCategory, string productId, string failReason)
	    {
		    var dict = new Dictionary<string, object>()
		    {
			    { ParameterItemCategory, itemCategory },
			    { ParameterItemName, productId },
			    { ParameterProductId, productId },
			    { ParameterReason, failReason }
		    };

		    TrackEvent(TrackingEvent.Create(EventIAPReturnFalse, dict));
	    }
	    
		/// <summary>
		/// 新用户首次 IAP 付费成功上报 （仅限应用内付费商品，不包含订阅等其它情况）【买量打点】
		/// </summary>
		/// <param name="itemName">productId 商品ID</param>
		/// <param name="value">付费总金额</param>
		/// <param name="currency">币种</param>
		public static void FirstIAP(string itemName, double value, string currency)
		{
			var dict = new Dictionary<string, object>()
			{
				{ ParameterItemName, itemName },
				{ ParameterValue, value },
				{ ParameterCurrency, currency },
			};
			
			TrackEvent(TrackingEvent.Create(EventIAPFirst, dict));
		}
		
		/// <summary>
		/// 商品购买成功上报【买量打点】
		/// </summary>
		/// <param name="productName">商品名称（商品ID一样）</param>
		/// <param name="itemName">productId 商品ID</param>
		/// <param name="value">付费总金额</param>
		/// <param name="currency">币种</param>
		public static void ProductIAP(string productName, string itemName, double value, string currency)
		{
			// 替换SKU中的 "." -> "_", 比如: "do.a.iapc.coin.100" 转换为 "do_a_iapc_coin_100"
			if (productName.Contains(".")) productName = productName.Replace(".", "_"); 
			
			string eventName = $"iap_{productName}";
			var dict =  new Dictionary<string, object>()
			{
				{ ParameterItemName, itemName },
				{ ParameterValue, value },
				{ ParameterCurrency, currency },
			};
			
			TrackEvent(TrackingEvent.Create(eventName, dict));
		}


		/// <summary>
		/// 支付成功后统一上报所有点位数据
		/// </summary>
		/// <param name="productId"></param>
		/// <param name="usdPrice"></param>
		/// <param name="orderData"></param>
		/// <param name="isTest"></param>
		public static void ReportIAPSuccessEvent(BaseOrderData orderData, double usdPrice, bool isTest = false)
		{
			if (orderData == null) return;

			if (!isTest && usdPrice == 0)
			{
				Debug.Log($"[SDK] --- Pruchase value is 0, skip report orders");
				return;
			}

			string productId = orderData.GetProductId();
			string userCurrency = orderData.userCurrency;
			double payPrice = orderData.payPrice;
			string orderType = orderData.OrderType();
			string orderType2 = orderData.OrderTypeII();
			string orderId = orderData.orderId;
			string orderDate  = orderData.payedDate;
			string scene = orderData.scene;
			bool isFree = orderData.isFree;
			string offerId = orderData.offerId;
			
			string transactionId = "";
			string productToken = "";
			string receipt = "";

			if (orderData is GoogleOrderData gdata)
			{
				productToken = gdata.token;
			}
			else if (orderData is AppleOrderData adata)
			{
				receipt = adata.receipt;
			}
			
			//---------- 太极打点逻辑 -----------
			// tch_001 和 tch_02 都要上报
			// fb 的 purchase 事件只打一次
			// TCH 001
			Tch001IAPRev(usdPrice, productId, orderId, orderType, orderDate, isTest); 
			
			// TCH 020
			Tch02IAPRev(usdPrice, productId, orderId, orderType, orderDate, isTest);
			
			// Facebook Track IAP Purchase
			FBPurchase(usdPrice, USD, "iap", IAPPlatform);
			
			//---------- 太极打点逻辑 -----------

			if (orderData.orderType == 1)
			{
				// sub_pruchase : Firebase + Guru + Adjust
				SubPurchase(usdPrice, productId, orderId, orderDate, productToken, receipt, isTest);
			}
			else
			{
				// iap_purchase : Firebase + Guru + Adjust
				IAPPurchase(usdPrice, productId, orderId, orderDate, productToken, receipt, isTest);
			}
			
			// IAP Ret true : Firebase + Guru + Adjust
			IAPRetTrue(scene, productId, payPrice, userCurrency, orderId, orderType2, isFree, offerId);
		}
		

		#endregion
		
		#region IAP_PURCHASE

		/// <summary>
		/// IAP 内购上报
		/// </summary>
		/// <param name="value"></param>
		/// <param name="productId"></param>
		/// <param name="orderId"></param>
		/// <param name="orderDate"></param>
		/// <param name="purchaseToken"></param>
		/// <param name="receipt"></param>
		/// <param name="isSandbox"></param>
		private static void IAPPurchase(double value, string productId, string orderId, string orderDate, 
			string purchaseToken = "", string receipt = "", bool isSandbox = false)
		{
			IAPPurchaseReport(EventIAPPurchase, value, productId, orderId, "IAP", orderDate, purchaseToken, receipt, isSandbox);
		}

		/// <summary>
		/// SUB 订阅上报
		/// </summary>
		/// <param name="value"></param>
		/// <param name="productId"></param>
		/// <param name="orderId"></param>
		/// <param name="orderDate"></param>
		/// <param name="purchaseToken"></param>
		/// <param name="receipt"></param>
		/// <param name="isSandbox"></param>
		private static void SubPurchase(double value, string productId, string orderId, string orderDate, 
			string purchaseToken = "", string receipt = "", bool isSandbox = false)
		{
			IAPPurchaseReport(EventSubPurchase, value, productId, orderId, "SUB", orderDate, purchaseToken, receipt, isSandbox);
		}
		
		private static void IAPPurchaseReport(string eventName, double value, string productId, 
			string orderId, string orderType, string orderDate, string purchaseToken = "", string receipt = "", bool isSandbox = false, string currency = USD)	
		{
			// 强类型转换
			var iapEvent = new IAPEvent(eventName, 
				value,
				productId, 
				orderId, 
				orderType, 
				orderDate, 
				isSandbox, 
				purchaseToken, 
				receipt, 
				currency);
			
			TrackEvent(iapEvent); // 上报 IAP 事件
	    }
		
		#endregion

		#region 中台异常打点

		/// <summary>
		/// 中台异常打点
		/// </summary>
		/// <param name="data"></param>
		public static void LogDevAudit(Dictionary<string, object> data)
		{
			if (data == null) return;
			data["country"] = IPMConfig.IPM_COUNTRY_CODE;
			data["network"] = Application.internetReachability.ToString();
			
			var evt = new TrackingEvent(EventDevAudit, data, new EventSetting()
			{
				EnableFirebaseAnalytics = true,
				EnableGuruAnalytics = true
			});
			
			TrackEvent(evt);
		}
		
		#endregion
		
    }


    
}

