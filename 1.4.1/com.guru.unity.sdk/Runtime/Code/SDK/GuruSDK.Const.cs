namespace Guru
{
    public partial class GuruSDK
    {
	    // GURU SDK MAIN VERSION 
	    private const string MAIN_VERSION = "1.4.1"; // Gen by SDK Publisher. (2/17/2025 11:28 AM)
	    
        /// <summary>
        /// Consts values
        /// </summary>
        public static class Consts
        {
	        #region Firebase Defines
	        
	        public const string EventAdImpression = "ad_impression";
			public const string EventAddPaymentInfo = "add_payment_info";
			public const string EventAddShippingInfo = "add_shipping_info";
			public const string EventAddToCart = "add_to_cart";
			public const string EventAddToWishlist = "add_to_wishlist";
			public const string EventAppOpen = "app_open";
			public const string EventBeginCheckout = "begin_checkout";
			public const string EventCampaignDetails = "campaign_details";
			public const string EventEarnVirtualCurrency = "earn_virtual_currency";
			public const string EventGenerateLead = "generate_lead";
			public const string EventJoinGroup = "join_group";
			public const string EventLevelEnd = "level_end";
			public const string EventLevelStart = "level_start";
			public const string EventLevelEndSuccessPrefix = "level_end_success_";
			public const string EventLevelUp = "level_up";
			public const string EventLogin = "login";
			public const string EventPostScore = "post_score";
			public const string EventPurchase = "purchase";
			public const string EventRefund = "refund";
			public const string EventRemoveFromCart = "remove_from_cart";
			public const string EventScreenView = "screen_view";
			public const string EventSearch = "search";
			public const string EventSelectContent = "select_content";
			public const string EventSelectItem = "select_item";
			public const string EventSelectPromotion = "select_promotion";
			public const string EventShare = "share";
			public const string EventSignUp = "sign_up";
			public const string EventSpendVirtualCurrency = "spend_virtual_currency";
			public const string EventUnlockAchievement = "unlock_achievement";
			public const string EventHpPoints = "hp_points";
			
			public const string EventAttReguideImp = "att_reguide_imp";
			public const string EventAttReguideClk = "att_reguide_clk";
			public const string EventAttReguideResult = "att_reguide_result";
			
			public const string EventTutorialBegin = "tutorial_begin";
			public const string EventTutorialImp= "tutorial_{0}_imp";
			public const string EventTutorialNextClick= "tutorial_{0}_next_clk";
			public const string EventTutorialComplete= "tutorial_complete";
			public const string EventTutorialClose = "tutorial_close";
			
			public const string EventNotiPermImp = "noti_perm_imp";
			public const string EventNotiPermResult = "noti_perm_result";
			public const string EventNotiPermRationaleImp = "noti_perm_rationale_imp";
			public const string EventNotiPermRationaleResult = "noti_perm_rationale_result";
			
			public const string EventDevAudit = "dev_audit";
			
			public const string EventViewCart = "view_cart";
			public const string EventViewItem = "view_item";
			public const string EventViewItemList = "view_item_list";
			public const string EventViewPromotion = "view_promotion";
			public const string EventViewSearchResults = "view_search_results";

			public const string ParameterAchievementId = "achievement_id";
			public const string ParameterAdFormat = "ad_format";
			public const string ParameterAdNetworkClickID = "aclid";
			public const string ParameterAdPlatform = "ad_platform";
			public const string ParameterAdSource = "ad_source";
			public const string ParameterAdUnitName = "ad_unit_name";
			public const string ParameterAffiliation = "affiliation";
			public const string ParameterCP1 = "cp1";
			public const string ParameterCampaign = "campaign";
			public const string ParameterCharacter = "character";
			public const string ParameterContent = "content";
			public const string ParameterContentType = "content_type";
			public const string ParameterCoupon = "coupon";
			public const string ParameterCreativeName = "creative_name";
			public const string ParameterCreativeSlot = "creative_slot";
			public const string ParameterCurrency = "currency";
			public const string ParameterDestination = "destination";
			public const string ParameterDiscount = "discount";
			public const string ParameterEndDate = "end_date";
			public const string ParameterExtendSession = "extend_session";
			public const string ParameterFlightNumber = "flight_number";
			public const string ParameterGroupId = "group_id";
			public const string ParameterIndex = "index";
			public const string ParameterItemBrand = "item_brand";
			public const string ParameterItemCategory = "item_category";
			public const string ParameterItemCategory2 = "item_category2";
			public const string ParameterItemCategory3 = "item_category3";
			public const string ParameterItemCategory4 = "item_category4";
			public const string ParameterItemCategory5 = "item_category5";
			public const string ParameterItemId = "item_id";
			public const string ParameterItemList = "item_list";
			public const string ParameterItemListID = "item_list_id";
			public const string ParameterItemListName = "item_list_name";
			public const string ParameterItemName = "item_name";
			public const string ParameterLevel = "level";
			public const string ParameterLevelName = "level_name";
			public const string ParameterLocation = "location";
			public const string ParameterLocationID = "location_id";
			public const string ParameterMedium = "medium";
			public const string ParameterMethod = "method";
			public const string ParameterNumberOfNights = "number_of_nights";
			public const string ParameterNumberOfPassengers = "number_of_passengers";
			public const string ParameterNumberOfRooms = "number_of_rooms";
			public const string ParameterOrigin = "origin";
			public const string ParameterPaymentType = "payment_type";
			public const string ParameterPrice = "price";
			public const string ParameterPromotionID = "promotion_id";
			public const string ParameterPromotionName = "promotion_name";
			public const string ParameterQuantity = "quantity";
			public const string ParameterScore = "score";
			public const string ParameterScreenClass = "screen_class";
			public const string ParameterScreenName = "screen_name";
			public const string ParameterSearchTerm = "search_term";
			public const string ParameterShipping = "shipping";
			public const string ParameterShippingTier = "shipping_tier";
			public const string ParameterSignUpMethod = "sign_up_method";
			public const string ParameterSource = "source";
			public const string ParameterStartDate = "start_date";
			public const string ParameterSuccess = "success";
			public const string ParameterTax = "tax";
			public const string ParameterTerm = "term";
			public const string ParameterTransactionId = "transaction_id";
			public const string ParameterTravelClass = "travel_class";
			public const string ParameterValue = "value";
			
	        #endregion
	        
	        #region Guru BI Events & Parameters
	        
            public const string TAG = "Analytics";
			// 美元符号
			public const string USD = "USD"; 
			// 广告平台
			public const string AdMAX = "MAX"; 
			
			//IAP打点事件
			public const string EventIAPFirst = "first_iap";
			public const string EventIAPImp = "iap_imp";
			public const string EventIAPClose = "iap_close";
			public const string EventIAPClick = "iap_clk";
			public const string EventIAPReturnTrue = "iap_ret_true";
			public const string EventIAPReturnFalse = "iap_ret_false";
			
			//横幅广告打点事件
			public const string EventBadsLoad = "bads_load";
			public const string EventBadsLoaded = "bads_loaded";
			public const string EventBadsFailed = "bads_failed";
			public const string EventBadsClick = "bads_clk";
			public const string EventBadsImp = "bads_imp";

			//插屏广告打点事件
			public const string EventIadsLoad = "iads_load";
			public const string EventIadsLoaded = "iads_loaded";
			public const string EventIadsFailed = "iads_failed";
			public const string EventIadsImp = "iads_imp";
			public const string EventIadsClick = "iads_clk";
			public const string EventIadsClose = "iads_close";
	    
			//激励视频广告打点事件
			public const string EventRadsLoad = "rads_load";
			public const string EventRadsLoaded = "rads_loaded";
			public const string EventRadsFailed = "rads_failed";
			public const string EventRadsImp = "rads_imp";
			public const string EventRadsRewarded = "rads_rewarded";
			public const string EventRadsClick = "rads_clk";
			public const string EventRadsClose = "rads_close";
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
			public const string ParameterStep = "step";
			public const string ParameterDuration = "duration";
			public const string ParameterErrorCode = "error_code";
			public const string ParameterProductId = "product_id";
			public const string ParameterPlatform = "platform";
			public const string ParameterStartType = "start_type"; // 游戏启动类型
			public const string ParameterReplay =  "replay"; // 游戏重玩
			public const string ParameterContinue = "continue"; // 游戏继续
			
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
			public const string PropertyIAPCoin = "iap_coin"; //付费所得的总金币数(iap获取累计值)\
			public const string PropertyNonIAPCoin = "noniap_coin"; //非付费iap获取累计值
			public const string PropertyCoin = "coin"; //当前金币数
			public const string PropertyExp = "exp"; // 经验值
			public const string PropertyHp = "hp"; // 生命值/体力
			public const string PropertyNetwork = "network"; // 网络状态
			public const string PropertyAndroidID = "android_id"; // Android 平台 AndroidID
			public const string PropertyIDFV = "idfv"; // iOS  平台 IDFV
			public const string PropertyPicture = "picture"; // 玩家在主线的mapid
			public const string PropertyNoAds = "no_ads"; // 玩家是否去广告
			public const string PropertyATTStatus = "att_status";  // ATT 状态
			public const string PropertyNotiPerm = "noti_perm";  // Notification Permission 状态
			public const string PropertyAdjustId = "adjust_id";  // AdjustId
			public const string PropertyGDPR = "gdpr"; // GDPR状态
			
			// 经济相关
			public const string ParameterBalance = "balance"; // 用于余额
			public const string ParameterDefaultScene = "in_game"; // 货币消费默认场景
			public const string ParameterVirtualCurrencyName = "virtual_currency_name"; // 虚拟货币名称

			
			public const string CurrencyNameProps = "props"; // props
			
			public const string CurrencyCategoryReward = "reward"; // common, ads
			public const string CurrencyCategoryIAP = "iap_buy"; // In app purchase
			public const string CurrencyCategoryBonus = "bonus"; // ads+items, gift box, item group 
			public const string CurrencyCategoryIGC = "igc"; // In game currency
			public const string CurrencyCategoryIGB = "igb"; // In game barter
			public const string CurrencyCategoryProp = "prop"; // prop
			public const string CurrencyCategoryProps = "props"; // props
			public const string CurrencyCategoryBundle = "bundle"; // prop groups
			public const string CurrencyCategoryBoost = "boost"; // boost
			
			//----------------- 关卡开始类型 ---------------------
			public const string EventLevelStartModePlay = "play";
			public const string EventLevelStartModeReplay = "replay";
			public const string EventLevelStartModeContinue= "continue";
        
			//----------------- 关卡结束类型 ---------------------
			public const string EventLevelEndSuccess = "success";
			public const string EventLevelEndFail = "fail";
			public const string EventLevelEndExit = "exit";
			public const string EventLevelEndTimeout = "timeout";
			
			/// <summary>
			/// 主线关卡类型
			/// 只有传入此类型时才会进行 Blevel 的累加
			/// </summary>
			public const string LevelTypeMain = "main";

			#endregion
        }
    }
}
