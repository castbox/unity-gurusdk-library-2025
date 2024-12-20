namespace Guru.Ads
{
    public enum AdMediationType
    {
        Max = 1,
        IronSource,
    }
    
    
    public static class AdConst
    {

        public const string VALUE_NOT_SET = "not_set";
        public const string AD_PLATFORM_MAX = "MAX";
        public const string AD_PLATFORM_IRONSOURCE = "IRONSOURCE";
        public const float RELOAD_TIME = 6;
        public const float NO_NETWORK_WAITING_TIME = 10; // 网络加载尝试等待
        public const float LOAD_NEXT_TIME = 2; // 加载下条广告的时间
        public const string CURRENCY_USD = "USD";
        public const string LOG_TAG_MAX = "[Ads][Max]";

    }


    public static class AdEvent
    {
        // --- Impression
        public const string AD_IMPRESSION = "ad_impression";
        // --- Banner
        public const string BADS_LOAD = "bads_load";
        public const string BADS_LOADED = "bads_loaded";
        public const string BADS_FAILED = "bads_failed";
        public const string BADS_IMP = "bads_imp";
        public const string BADS_HIDE = "bads_hide";
        public const string BADS_CLK = "bads_clk";
        public const string BADS_PAID = "bads_paid";
        
        // --- MRec
        public const string MADS_LOAD = "mads_load";
        public const string MADS_IMP = "mads_imp";
        public const string MADS_HIDE = "mads_hide";
        public const string MADS_CLK = "mads_clk";
        public const string MADS_PAID = "mads_paid";
        // --- INTER
        public const string IADS_LOAD = "iads_load";
        public const string IADS_LOADED = "iads_loaded";
        public const string IADS_FAILED = "iads_failed";
        public const string IADS_IMP = "iads_imp";
        public const string IADS_CLK = "iads_clk";
        public const string IADS_CLOSE = "iads_close";
        public const string IADS_PAID = "iads_paid";
        
        // --- REWARDED
        public const string RADS_LOAD = "rads_load";
        public const string RADS_LOADED = "rads_loaded";
        public const string RADS_FAILED = "rads_failed";
        public const string RADS_IMP = "rads_imp";
        public const string RADS_REWARDED = "rads_rewarded";
        public const string RADS_CLK = "rads_clk";
        public const string RADS_CLOSE = "rads_close";
        public const string FIRST_RADS_REWARDED = "first_rads_rewarded";
        public const string RADS_PAID = "rads_paid";
        
        // --- PARAMETER ---
        public const string PARAM_ITEM_CATEGORY = "item_category";
        public const string PARAM_ITEM_NAME = "item_name";
        public const string PARAM_DURATION = "duration";
        public const string PARAM_FAILED_TIMES = "failed_times";
        public const string PARAM_LOADED_TIMES = "loaded_times";
        public const string PARAM_ERROR_CODE = "error_code";
        public const string PARAM_VALUE = "value";
        public const string PARAM_CURRENCY = "currency";
        public const string PARAM_AD_PLATFORM = "ad_platform";
        public const string PARAM_AD_SOURCE = "ad_source";
        public const string PARAM_AD_FORMAT = "ad_format";
        public const string PARAM_AD_UNIT_NAME = "ad_unit_name";
        public const string PARAM_AD_CREATIVE_ID = "ad_creative_id";
        public const string PARAM_AD_PLACEMENT = "ad_placement";
        
    }

}


