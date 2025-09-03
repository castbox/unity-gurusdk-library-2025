using System.Collections.Generic;

namespace Guru
{
	
	
	public enum ELevelResult
	{
		success,
		fail,
		timeout,
		exit,
		replay
	}
	
	/// <summary>
	/// 打点优先级
	/// </summary>
	public enum EventPriority
	{
		Unknown = -1,
		Emergence = 0,
		High = 5,
		Default = 10,
		Low = 15
	}

	public enum TchFbModeEnum
	{
		Mode001 = 1,
		Mode02 = 2
	}
	

	//打点常量定义
	public partial class Analytics
	{
		public const string TAG = "[ANU][GA]";
		// 美元符号
		public const string USD = "USD"; 
		// 广告平台
		public const string AdMAX = "MAX";
		public const string AdIronSource = "IronSource";
		public const int TCH_FB_MODE_001 = 1;
		public const int TCH_FB_MODE_02 = 2;
		
		//IAP打点事件
		public const string EventIAPFirst = "first_iap";
		public const string EventIAPImp = "iap_imp";
		public const string EventIAPClose = "iap_close";
		public const string EventIAPClick = "iap_clk";
		public const string EventIAPReturnTrue = "iap_ret_true";
		public const string EventIAPReturnFalse = "iap_ret_false";
		
		// 关卡打点
		public const string EventLevelFirstEnd = "level_first_end";
		
		//横幅广告打点事件
		public const string EventBadsLoad = "bads_load";
		public const string EventBadsLoaded = "bads_loaded";
		public const string EventBadsFailed = "bads_failed";
		public const string EventBadsClick = "bads_clk";
		public const string EventBadsImp = "bads_imp";
		public const string EventBadsHide = "bads_hide";
		public const string EventBadsPaid = "bads_paid";

		//插屏广告打点事件
		public const string EventIadsLoad = "iads_load";
		public const string EventIadsLoaded = "iads_loaded";
		public const string EventIadsFailed = "iads_failed";
		public const string EventIadsImp = "iads_imp";
		public const string EventIadsClick = "iads_clk";
		public const string EventIadsClose = "iads_close";
		public const string EventIadsPaid = "iads_paid";
    
		//激励视频广告打点事件
		public const string EventRadsLoad = "rads_load";
		public const string EventRadsLoaded = "rads_loaded";
		public const string EventRadsFailed = "rads_failed";
		public const string EventRadsImp = "rads_imp";
		public const string EventRadsRewarded = "rads_rewarded";
		public const string EventRadsClick = "rads_clk";
		public const string EventRadsClose = "rads_close";
		public const string EventRadsPaid = "rads_paid";
		public const string EventFirstRadsRewarded = "first_rads_rewarded";
		
		//广告收益打点事件
		public const string EventTchAdRev001Impression = "tch_ad_rev_roas_001";
		public const string EventTchAdRev02Impression = "tch_ad_rev_roas_02";
		public const string EventTchAdRevAbnormal = "tch_ad_rev_value_abnormal";
		
		
		//内购成功事件上报
		public const string EventIAPPurchase = "iap_purchase";
		public const string EventSubPurchase = "sub_purchase";
		public const string IAPStoreCategory = "Store";
		public const string IAPTypeProduct = "product";
		public const string IAPTypeSubscription = "subscription";
		
		//打点参数名
		public const string ParameterResult = "result";
		public const string ParameterReason = "reason";
		public const string ParameterStep = "step";
		public const string ParameterDuration = "duration";
		public const string ParameterErrorCode = "error_code";
		public const string ParameterProductId = "product_id";
		public const string ParameterPlatform = "platform";
		public const string ParameterStartType = "start_type"; // 游戏启动类型
		public const string ParameterReplay =  "replay"; // 游戏重玩
		public const string ParameterContinue = "continue"; // 游戏继续
		public const string ParameterAdUnitName = "ad_unit_name";
		public const string ParameterAdPlacement = "ad_placement";
		public const string ParameterAdCreativeId = "ad_creative_id";
		public const string ParameterReviewCreativeId = "review_creative_id";
		
		
		// 评价参数
		public const string EventRateImp = "rate_imp"; // 评价弹窗展示
		public const string EventRateNow = "rate_now"; // 点击评分引导弹窗中的评分
		
		//打点内部执行错误
		public static string ParameterEventError => "event_error";
		
		//ios ATT打点
		public const string ATTGuideShow = "att_guide_show";
		public const string ATTGuideOK = "att_guide_ok";
		public const string ATTWindowShow = "att_window_show";
		public const string ATTOptIn = "att_opt_in";
		public const string ATTOpOut = "att_opt_out";
		public const string ParameterATTStatus = "att_status";
		public const string EventAttResult = "att_result";
		
		// 用户属性
		public const string PropertyFirstOpenTime = "first_open_time"; 		//用户第一次first_open的时间
		public const string PropertyDeviceID = "device_id"; //用户的设备ID
		public const string PropertyUserID = "user_id";
		public const string PropertyLevel = "b_level"; //"每次完成通关上升一次，显示用户完成的最大关卡数。只针对主关卡和主玩法的局数做累加，初始值为0。"
		public const string PropertyPlay = "b_play"; //每完成一局或者游戏触发，
		public const string PropertyLastPlayedLevel = "last_played_level";
		public const string PropertyGrade = "grade"; //当游戏玩家角色升级时触发
		public const string PropertyIsIAPUser = "is_iap_user"; 		//付费成功后设置属性参数为true，如果没有发生付费可以不用设置该属性
		public const string PropertyNetwork = "network"; 		// network属性
		public const string PropertyAdjustId = "adjust_id"; 		// adjust_id
		public const string PropertyAppsflyerId = "appsflyer_id"; 		// appsflyer_id
		public const string PropertyIAPCoin = "iap_coin"; //付费所得的总金币数(iap获取累计值)\
		public const string PropertyNonIAPCoin = "noniap_coin"; //非付费iap获取累计值
		public const string PropertyCoin = "coin"; //当前金币数
		public const string PropertyExp = "exp"; // 经验值
		public const string PropertyHp = "hp"; // 生命值/体力
		public const string PropertyAndroidId = "android_id"; // Android 平台 AndroidID
		public const string PropertyIDFV = "idfv"; // iOS  平台 IDFV
		public const string PropertyIDFA = "idfa"; // iOS  平台 IDFA
		public const string PropertyPicture = "picture"; // 玩家在主线的mapid
		public const string PropertyNoAds = "no_ads"; // 玩家是否去广告
		public const string PropertyAttStatus = "att_status";  // ATT 状态
		public const string PropertyNotiPerm = "noti_perm";  // ATT 状态
		public const string PropertyGDPR = "gdpr"; // GDPR状态
		public const string PropertySignUpMethod = "sign_up_method"; // 用户登录方式
		public const string PropertyFirebaseId = "firebase_id";  // FirebaseID
		public const string PropertyGoogleAdId = "ad_id";  // Google AdId
		public const string PropertyAnalyticsExperimentalGroup = "guru_analytics_exp"; // Analytics Experimental Group
		public const string PropertyGuruSdkVersion = "sdk_version"; // GuruSDK 版本号
		public const string PropertyUserCreatedTimestamp = "user_created_timestamp";  // FirebaseID


		public static HashSet<string> PredefinedMidWareProperties = new HashSet<string>()
		{
			PropertyFirstOpenTime,
			PropertyDeviceID,
			PropertyUserID,
			PropertyIsIAPUser,
			PropertyNetwork,
			PropertyAdjustId,
			PropertyAndroidId,
			PropertyIDFV,
			PropertyIDFA,
			PropertyAttStatus,
			PropertyNotiPerm,
			PropertyFirebaseId,
			PropertyGoogleAdId,
		};
		
		// 经济相关
		public const string ParameterBalance = "balance"; // 用于余额
		public const string ParameterScene = "scene"; // 货币消费场景
		public const string ParameterVirtualCurrencyName = "virtual_currency_name"; // 虚拟货币名称
		
		// 中台
		public const string EventDevAudit = "dev_audit"; // 中台事件异常
	}
}