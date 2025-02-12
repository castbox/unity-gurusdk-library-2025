
namespace Guru.Ads
{
    using System.Collections.Generic;
    using Guru;
    
    /*
     * BI 定义的事件结构详见：
     * https://docs.google.com/spreadsheets/d/1N47rXgjatRHFvzWWx0Hqv5C1D9NHHGbggi6pQ65c-zQ/edit?gid=732914073#gid=732914073
    */
    
    #region Abastract Event
    /// <summary>
    /// 通用抽象事件
    /// </summary>
    public abstract class AbstractAdCommonEvent: ITrackingEvent
    {
        // --- basic props ---
        public readonly string adUnitId;
        public readonly string placement;
        
        public string EventName { get; }
        public EventSetting Setting { get; }
        public Dictionary<string, object> Data { get; }
        public EventPriority Priority { get; }

        protected AbstractAdCommonEvent(string eventName, string adUnitId, string placement)
        {
            this.EventName = eventName;
            this.adUnitId = adUnitId;
            this.placement = placement;
            Priority = EventPriority.Default;
            Setting = EventSetting.FirebaseAndGuru();
            Data = new Dictionary<string, object>
            {
                [AdEvent.PARAM_ITEM_NAME] = adUnitId,
                [AdEvent.PARAM_ITEM_CATEGORY] = placement
            };
        }
    }

    /// <summary>
    /// 抽象加载事件
    /// </summary>
    public abstract class AbstractAdLoadedEvent: ITrackingEvent
    {
        public string EventName { get; }
        public EventSetting Setting { get; }
        public Dictionary<string, object> Data { get; }
        public EventPriority Priority { get; }
        
        // --- basic props ---
        public readonly string adUnitId;
        public readonly string placement;
        public readonly int duration;
        public readonly string adSource;
        public readonly string networkPlacement;
        public readonly string waterfallName;
        
        protected AbstractAdLoadedEvent(string eventName,
            string adUnitId,
            string placement, 
            int duration, 
            string adSource, 
            string networkPlacement, 
            string waterfallName)
        {
            this.EventName = eventName;
            this.adUnitId = adUnitId;
            this.placement = placement;
            this.duration = duration;
            this.adSource = adSource;
            this.networkPlacement = networkPlacement;
            this.waterfallName = waterfallName;

            Priority = EventPriority.Default;
            Setting = new EventSetting
            {
                EnableGuruAnalytics = true,
                EnableFirebaseAnalytics = true,
                EnableAdjustAnalytics = false,
                EnableFacebookAnalytics = false
            };
            Data = new Dictionary<string, object>
            {
                [AdEvent.PARAM_ITEM_NAME] = adUnitId,
                [AdEvent.PARAM_ITEM_CATEGORY] = placement,
                [AdEvent.PARAM_DURATION] = duration,
            };
        }

      
    }
    
    /// <summary>
    /// 抽象失败事件
    /// </summary>
    public abstract class AbstractAdFailedEvent: ITrackingEvent
    {
        public string EventName { get; }
        public EventSetting Setting { get; }
        public Dictionary<string, object> Data { get; }
        public EventPriority Priority { get; }
        
        // --- basic props ---
        public readonly string adUnitId;
        public readonly string placement;
        public readonly int duration;
        public readonly int errorCode;
        public readonly string waterfallName;
        
        protected AbstractAdFailedEvent(string eventName,
            string adUnitId,
            string placement, 
            int errorCode,
            int duration,
            string waterfallName)
        {
            this.EventName = eventName;
            this.adUnitId = adUnitId;
            this.placement = placement;
            this.errorCode = errorCode;
            this.duration = duration;
            this.waterfallName = waterfallName;
            Priority = EventPriority.Default;
            Setting = new EventSetting
            {
                EnableGuruAnalytics = true,
                EnableFirebaseAnalytics = true,
                EnableAdjustAnalytics = false,
                EnableFacebookAnalytics = false
            };
            Data = new Dictionary<string, object>
            {
                [AdEvent.PARAM_ITEM_NAME] = adUnitId,
                [AdEvent.PARAM_ITEM_CATEGORY] = placement,
                [AdEvent.PARAM_ERROR_CODE] = $"{errorCode}",
                [AdEvent.PARAM_DURATION] = duration,
            };
        }
        
    }

    /*
     * "ad_source               广告来源"
     * "ad_unit_name            广告位名称"
     * "ad_placement            广告平台生成的id"
     * "ad_creative_id          广告素材id"
     * "ad_platform             收入平台"
     * "item_category           广告场景"
     */
    public abstract class AbstractAdImpEvent : ITrackingEvent
    {
        public string EventName { get; }
        public EventSetting Setting { get; }
        public Dictionary<string, object> Data { get; }
        public EventPriority Priority { get; }
        
        // --- basic props ---
        public readonly string adSource;
        public readonly string adUnitId;
        public readonly string adPlacement;
        public readonly string adCreativeId;
        public readonly string placement;
        public readonly string adPlatform;
        public readonly string reviewCreativeId;

        protected AbstractAdImpEvent(string eventName,
            string adUnitId,
            string adSource,
            string adPlacement,
            string adCreativeId,
            string placement,
            string adPlatform,
            string reviewCreativeId = "")
        {
            this.EventName = eventName;
            this.adUnitId = adUnitId;
            this.adSource = adSource;
            this.adPlacement = adPlacement;
            this.adCreativeId = adCreativeId;
            this.adPlatform = adPlatform;
            this.placement = placement;
            this.reviewCreativeId = reviewCreativeId;
   
            Priority = EventPriority.Default;

            Setting = EventSetting.FirebaseAndGuru();

            Data = new Dictionary<string, object>
            {
                [AdEvent.PARAM_AD_PLATFORM] = adPlatform,
                [AdEvent.PARAM_AD_SOURCE] = adSource,
                [AdEvent.PARAM_AD_UNIT_NAME] = adUnitId,
                [AdEvent.PARAM_AD_PLACEMENT] = adPlacement,
                [AdEvent.PARAM_AD_CREATIVE_ID] = adCreativeId,
                [AdEvent.PARAM_ITEM_CATEGORY] = placement
            };
            if (!string.IsNullOrEmpty(this.reviewCreativeId))
                Data[AdEvent.PARAM_REVIEWED_CREATIVE_ID] = this.reviewCreativeId;
        }
    }


    /*
     * "value                   收入价值"
     * "currency                收入币种"
     * "ad_platform             收入平台"
     * "ad_source               广告来源"
     * "ad_unit_name            广告位名称"
     * "ad_placement            广告平台生成的id"
     * "ad_creative_id          广告素材id"
     * "item_category           广告场景"
     */
    public abstract class AbstractAdDetailEvent : ITrackingEvent
    {
        public string EventName { get; }
        public EventSetting Setting { get; }
        public Dictionary<string, object> Data { get; }
        public EventPriority Priority { get; }
        
        // --- basic props ---
        public readonly string adUnitId;
        public readonly string currency;
        public readonly double value;
        public readonly string adSource;
        public readonly string adPlatform;
        public readonly string adPlacement;
        public readonly string adCreativeId;
        public readonly string placement;
        public readonly string reviewCreativeId;
        
        protected AbstractAdDetailEvent(string eventName,
            string adUnitId,
            string currency,
            double value,
            string adSource,
            string adPlacement,
            string adCreativeId,
            string placement,
            string adPlatform,
            string reviewCreativeId = "")
        {
            this.EventName = eventName;
            this.adUnitId = adUnitId;
            this.currency = currency;
            this.value = value;
            this.adSource = adSource;
            this.adPlacement = adPlacement;
            this.adCreativeId = adCreativeId;
            this.adPlatform = adPlatform;
            this.placement = placement;
            this.reviewCreativeId = reviewCreativeId;

            if (string.IsNullOrEmpty(this.adPlacement)) this.adPlacement = AdConst.VALUE_NOT_SET;
            if (string.IsNullOrEmpty(this.adCreativeId)) this.adCreativeId = AdConst.VALUE_NOT_SET;
            if (string.IsNullOrEmpty(this.placement)) this.placement = AdConst.VALUE_NOT_SET;
            
            Setting = EventSetting.FirebaseAndGuru();;
            Priority = EventPriority.Default;
            Data = new Dictionary<string, object>
            {
                [AdEvent.PARAM_VALUE] = this.value,
                [AdEvent.PARAM_CURRENCY] = this.currency,
                [AdEvent.PARAM_AD_PLATFORM] = this.adPlatform,
                [AdEvent.PARAM_AD_SOURCE] = this.adSource,
                [AdEvent.PARAM_AD_UNIT_NAME] = this.adUnitId,
                [AdEvent.PARAM_AD_PLACEMENT] = this.adPlacement,
                [AdEvent.PARAM_AD_CREATIVE_ID] = this.adCreativeId,
                [AdEvent.PARAM_ITEM_CATEGORY] = this.placement,
            };
            
            if (!string.IsNullOrEmpty(this.reviewCreativeId))
            {
                Data[AdEvent.PARAM_REVIEWED_CREATIVE_ID] = this.reviewCreativeId;
            }
        }
    }


    public abstract class AbstractAdCloseEvent : ITrackingEvent
    {
        public string EventName { get; }
        public EventSetting Setting { get; }
        public Dictionary<string, object> Data { get; }
        public EventPriority Priority { get; }
        public readonly string placement;
        public readonly string adUnitId;
        public readonly int duration;

        protected AbstractAdCloseEvent(string eventName, string adUnitId, string placement, int duration)
        {
            EventName = eventName;
            this.adUnitId = adUnitId;
            this.placement = placement;
            this.duration = duration;
            Setting = EventSetting.FirebaseAndGuru();
            Priority = EventPriority.Default;
            Data = new Dictionary<string, object>
            {
                [AdEvent.PARAM_ITEM_CATEGORY] = placement,
                [AdEvent.PARAM_DURATION] = duration
            };
        }
    }


    /// <summary>
    /// 抽象收益事件
    /// </summary>
    public abstract class AbstractAdPaidEvent: IAdPaidEvent, ITrackingEvent
    {
        public string EventName { get; }
        public EventSetting Setting { get; }
        public Dictionary<string, object> Data { get; set; }
        public EventPriority Priority { get; }
        
        // --- basic props ---
        public readonly string adUnitId;
        public readonly string currency;
        public readonly double value;
        public readonly string adSource;
        public readonly string adPlatform;
        public readonly string adFormat;
        public readonly string adPlacement;
        public readonly string adCreativeId;
        public readonly string placement;
        public readonly string reviewCreativeId;
        
        protected AbstractAdPaidEvent(string eventName, 
            string adUnitId, 
            string currency, 
            double value, 
            string adSource, 
            string adPlacement, 
            string adCreativeId,
            string adFormat, 
            string placement,
            string adPlatform,
            string reviewCreativeId)
        {
            EventName = eventName;
            this.adUnitId = adUnitId;
            this.currency = currency;
            this.value = value;
            this.adSource = adSource;
            this.adPlacement = adPlacement;
            this.adCreativeId = adCreativeId;
            this.adFormat = adFormat;
            this.adPlatform = adPlatform;
            this.placement = placement;
            this.reviewCreativeId = reviewCreativeId;

            Setting = EventSetting.FirebaseAndGuru();
            Priority = EventPriority.Default;
            Data = new Dictionary<string, object>()
            {
                [AdEvent.PARAM_VALUE] = this.value,
                [AdEvent.PARAM_CURRENCY] = this.currency,
                [AdEvent.PARAM_AD_PLATFORM] = this.adPlatform,
                [AdEvent.PARAM_AD_SOURCE] = this.adSource,
                [AdEvent.PARAM_AD_UNIT_NAME] = this.adUnitId,
                [AdEvent.PARAM_AD_PLACEMENT] = this.adPlacement,
                [AdEvent.PARAM_AD_CREATIVE_ID] = this.adCreativeId,
                [AdEvent.PARAM_ITEM_CATEGORY] = this.placement,
            };
            
            if (!string.IsNullOrEmpty(this.reviewCreativeId))
            {
                Data[AdEvent.PARAM_REVIEWED_CREATIVE_ID] = this.reviewCreativeId;
            }
        }
        
        
        
        
        /// <summary>
        /// 转化为 Impression 事件数据
        /// </summary>
        /// <returns></returns>
        public AdImpressionEvent ToAdImpressionEvent()
        {
            return new AdImpressionEvent(
                this.adUnitId,
                this.currency,
                this.value,
                this.adSource,
                this.adPlacement,
                this.adCreativeId,
                this.adFormat,
                this.adPlatform,
                this.reviewCreativeId);
        }
        
    }


    /// <summary>
    /// 抽象广告展示事件
    /// </summary>
    public class AdImpressionEvent : ITrackingEvent, IAdImpressionEvent, IAdjustAdImpressionEvent
    {
        public string EventName { get; }
        public EventSetting Setting { get; }
        public Dictionary<string, object> Data { get; set; }
        public EventPriority Priority { get; }
        
        // --- basic props ---
        public readonly string adUnitId;
        public readonly string currency;
        public readonly double value;
        public readonly string adSource;
        public readonly string adPlatform;
        public readonly string adFormat;
        public readonly string adPlacement;
        public readonly string adCreativeId;
        public readonly string reviewCreativeId;

        public AdImpressionEvent(
            string adUnitId,
            string currency,
            double value,
            string adSource,
            string adPlacement,
            string adCreativeId,
            string adFormat,
            string adPlatform,
            string reviewCreativeId)
        {
            this.EventName = AdEvent.AD_IMPRESSION;
            this.adUnitId = adUnitId;
            this.currency = currency;
            this.value = value;
            this.adSource = adSource;
            this.adPlatform = adPlatform;
            this.adFormat = adFormat;
            this.adPlacement = adPlacement;
            this.adCreativeId = adCreativeId;
            this.reviewCreativeId = reviewCreativeId;

            if (string.IsNullOrEmpty(this.adPlacement)) this.adPlacement = AdConst.VALUE_NOT_SET;
            if (string.IsNullOrEmpty(this.adCreativeId)) this.adCreativeId = AdConst.VALUE_NOT_SET;
            
            Setting = new EventSetting()
            {
                EnableFirebaseAnalytics = true,
                EnableGuruAnalytics = true,
                EnableAdjustAnalytics  = true,
                EnableFacebookAnalytics = false
            };
            Priority = EventPriority.Emergence;
            Data = new Dictionary<string, object>
            {
                [AdEvent.PARAM_VALUE] = this.value,
                [AdEvent.PARAM_CURRENCY] = this.currency,
                [AdEvent.PARAM_AD_PLATFORM] = this.adPlatform,
                [AdEvent.PARAM_AD_SOURCE] = this.adSource,
                [AdEvent.PARAM_AD_FORMAT] = this.adFormat,
                [AdEvent.PARAM_AD_UNIT_NAME] = this.adUnitId,
                [AdEvent.PARAM_AD_PLACEMENT] = this.adPlacement,
                [AdEvent.PARAM_AD_CREATIVE_ID] = this.adCreativeId,
            };
            if (!string.IsNullOrEmpty(this.reviewCreativeId))
            {
                Data[AdEvent.PARAM_REVIEWED_CREATIVE_ID] = this.reviewCreativeId;
            }
        }
        
        /// <summary>
        /// 转化为 Adjust 事件
        /// </summary>
        /// <returns></returns>
        public AdjustAdImpressionEvent ToAdjustAdImpressionEvent()
        {
            var evt = new AdjustAdImpressionEvent(EventName,
                this.value,
                this.adPlatform,
                this.adSource,
                this.adFormat,
                this.adUnitId,
                this.adPlacement,
                this.adCreativeId,
                this.currency);
            return evt;
        }
       
    }

    #endregion

    #region BANNER EVENTS

    // bads_load
    public class BadsLoadEvent : AbstractAdCommonEvent
    {
        public BadsLoadEvent(string adUnitId, string placement) : base(AdEvent.BADS_LOAD, adUnitId, placement)
        {
        }
    }

    // bads_imp
    public class BadsImpEvent : AbstractAdCommonEvent
    {
        public BadsImpEvent(string adUnitId, string placement) : base(AdEvent.BADS_IMP, adUnitId, placement)
        {
        }
    }

    // bads_hide
    public class BadsHideEvent : ITrackingEvent
    {
        public string EventName { get; }
        public EventSetting Setting { get; }
        public Dictionary<string, object> Data { get; }
        public EventPriority Priority { get; }

        // --- basic props ---
        private readonly int loadedTimes;
        private readonly int failedTimes;

        public BadsHideEvent(int loadedTimes, int failedTimes)
        {
            EventName = AdEvent.BADS_HIDE;
            Priority = EventPriority.Default;
            this.loadedTimes = loadedTimes;
            this.failedTimes = failedTimes;
            Setting = EventSetting.FirebaseAndGuru();
            Data = new Dictionary<string, object>
            {
               [AdEvent.PARAM_LOADED_TIMES] = loadedTimes,
               [AdEvent.PARAM_FAILED_TIMES] = failedTimes,
            };
        }
        
    }

    // bads_loaded
    public class BadsLoadedEvent : AbstractAdLoadedEvent
    {
        public BadsLoadedEvent(
            string adUnitId,
            string placement,
            int duration,
            string adSource,
            string networkPlacement,
            string waterfallName) :
            base(AdEvent.BADS_LOADED, adUnitId, placement, duration, adSource, networkPlacement, waterfallName)
        {

        }
    }

    // bads_failed
    public class BadsFailedEvent : AbstractAdFailedEvent
    {
        public BadsFailedEvent(
            string adUnitId,
            string placement,
            int errorCode,
            int duration,
            string waterfallName) :
            base(AdEvent.BADS_FAILED, adUnitId, placement, errorCode, duration, waterfallName)
        {

        }
    }

    /// <summary>
    /// 广告点击
    /// </summary>
    public class BadsClickEvent : AbstractAdCommonEvent
    {
        public BadsClickEvent(string adUnitId, string placement) : base(AdEvent.BADS_CLK, adUnitId, placement)
        {
        }
    }

    /// <summary>
    /// 广告收益
    /// </summary>
    public class BadsPaidEvent : AbstractAdPaidEvent
    {
        public BadsPaidEvent(
            string adUnitId,
            string currency,
            double value,
            string adSource,
            string adPlacement,
            string adCreativeId,   
            string adFormat,
            string placement,
            string adPlatform,
            string reviewCreativeId) : 
            base(AdEvent.BADS_PAID, 
                adUnitId, 
                currency, 
                value, 
                adSource, 
                adPlacement,
                adCreativeId,
                adFormat,      
                placement,
                adPlatform, 
                reviewCreativeId)
        {

        }
    }

    #endregion

    #region INTER EVENTS

    public class IadsLoadEvent : AbstractAdCommonEvent
    {
        public IadsLoadEvent(string adUnitId, string placement) : base(AdEvent.IADS_LOAD, adUnitId, placement)
        {
        }
    }

    public class IadsLoadedEvent : AbstractAdLoadedEvent
    {
        public IadsLoadedEvent(
            string adUnitId,
            string placement,
            int duration,
            string adSource,
            string networkPlacement,
            string waterfallName) :
            base(AdEvent.IADS_LOADED, adUnitId, placement, duration, adSource, networkPlacement, waterfallName)
        {

        }
    }

    public class IadsFailedEvent : AbstractAdFailedEvent
    {
        public IadsFailedEvent(
            string adUnitId,
            string placement,
            int errorCode,
            int duration,
            string waterfallName) :
            base(AdEvent.IADS_FAILED, adUnitId, placement, errorCode, duration, waterfallName)
        {

        }
    }

    public class IadsImpEvent : AbstractAdImpEvent
    {
        public IadsImpEvent(
            string adUnitId,
            string adSource,
            string adPlacement,
            string adCreativeId,
            string placement,
            string adPlatform,
            string reviewCreativeId) 
            : base(AdEvent.IADS_IMP, adUnitId, adSource, adPlacement, adCreativeId, placement, adPlatform, reviewCreativeId)
        {
        }
    }

    /// <summary>
    /// 广告点击
    /// </summary>
    public class IadsClickEvent : AbstractAdDetailEvent
    {
        public IadsClickEvent(
            string adUnitId,
            string currency,
            double value,
            string adSource,
            string adPlacement,
            string adCreativeId,
            string placement,
            string adPlatform,
            string reviewCreativeId) 
            : base(AdEvent.IADS_CLK, adUnitId, currency, value, adSource, adPlacement,adCreativeId, placement, adPlatform, reviewCreativeId)
        {
        }
    }

    public class IadsCloseEvent : AbstractAdCloseEvent
    {
        public IadsCloseEvent(string adUnitId, string placement, int duration) : base(AdEvent.IADS_CLOSE, adUnitId, placement, duration)
        {
        }
    }

    /// <summary>
    /// 广告收益
    /// </summary>
    public class IadsPaidEvent : AbstractAdPaidEvent
    {
        public IadsPaidEvent(
            string adUnitId,
            string currency,
            double value,
            string adSource,
            string adFormat,
            string adPlacement,
            string adCreativeId,
            string placement,
            string adPlatform,
            string reviewCreativeId) :
            base(AdEvent.IADS_PAID, adUnitId, currency, value, adSource, adPlacement, adCreativeId, adFormat, placement, adPlatform, reviewCreativeId)
        {

        }
    }

    #endregion

    #region REWARED EVENTS

    public class RadsLoadEvent : AbstractAdCommonEvent
    {
        public RadsLoadEvent(string adUnitId, string placement) :
            base(AdEvent.RADS_LOAD, adUnitId, placement)
        {
        }
    }

    public class RadsLoadedEvent : AbstractAdLoadedEvent
    {
        public RadsLoadedEvent(
            string adUnitId,
            string placement,
            int duration,
            string adSource,
            string networkPlacement,
            string waterfallName) :
            base(AdEvent.RADS_LOADED, adUnitId, placement, duration, adSource, networkPlacement, waterfallName)
        {

        }
    }

    public class RadsFailedEvent : AbstractAdFailedEvent
    {
        public RadsFailedEvent(
            string adUnitId,
            string placement,
            int errorCode,
            int duration,
            string waterfallName) :
            base(AdEvent.RADS_FAILED, adUnitId, placement, errorCode, duration, waterfallName)
        {

        }
    }

    public class RadsImpEvent : AbstractAdImpEvent
    {
        public RadsImpEvent(
            string adUnitId,
            string adSource,
            string adPlacement,
            string adCreativeId,
            string placement,
            string adPlatform,
            string reviewCreativeId) 
            : base(AdEvent.RADS_IMP, adUnitId, adSource, adPlacement, adCreativeId, placement, adPlatform, reviewCreativeId)
        {
        }
    }

    /// <summary>
    /// 广告点击
    /// </summary>
    public class RadsClickEvent : AbstractAdDetailEvent
    {
        public RadsClickEvent(string adUnitId,
            string currency,
            double value,
            string adSource,
            string adPlacement,
            string adCreativeId,
            string placement,
            string adPlatform,
            string reviewCreativeId) : 
            base(AdEvent.RADS_CLK, 
                adUnitId,
                currency,
                value,
                adSource,
                adPlacement,
                adCreativeId,
                placement,
                adPlatform,
                reviewCreativeId)
        {
        }
    }

    public class RadsCloseEvent : AbstractAdCloseEvent
    {
        public RadsCloseEvent(string adUnitId, string placement, int duration) : base(AdEvent.RADS_CLOSE, adUnitId, placement, duration)
        {
        }
    }

    public class RadsRewardedEvent : AbstractAdDetailEvent
    {
        public RadsRewardedEvent(string adUnitId,
            string currency,
            double value,
            string adSource,
            string adPlacement,
            string adCreativeId,
            string placement,
            string adPlatform,
            string reviewCreativeId) : 
            base(AdEvent.RADS_REWARDED, 
                adUnitId,
                currency,
                value,
                adSource,
                adPlacement,
                adCreativeId,
                placement,
                adPlatform,
                reviewCreativeId)
        {
        }
    }

    public class RadsFirstRewardedEvent : AbstractAdCommonEvent
    {
        public RadsFirstRewardedEvent(string adUnitId, string placement) : base(AdEvent.FIRST_RADS_REWARDED,
            adUnitId, placement)
        {
        }
    }

    /// <summary>
    /// 广告收益
    /// </summary>
    public class RadsPaidEvent : AbstractAdPaidEvent
    {
        public RadsPaidEvent(
            string adUnitId,
            string currency,
            double value,
            string adSource,
            string adFormat,
            string adPlacement,
            string adCreativeId,
            string placement,
            string adPlatform,
            string reviewCreativeId) :
            base(AdEvent.RADS_PAID, adUnitId, currency, value, adSource, adPlacement, adCreativeId, adFormat, placement, adPlatform, reviewCreativeId)
        {

        }
    }

    #endregion

    
    
}