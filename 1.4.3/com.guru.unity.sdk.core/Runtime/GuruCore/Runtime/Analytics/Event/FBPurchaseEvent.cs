
namespace Guru
{
    using System.Collections.Generic;
    
    public class FBPurchaseEvent: ITrackingEvent, IFBPurchaseEvent
    {
        public string EventName { get; }
        public Dictionary<string, object> Data { get; }
        public EventSetting Setting { get; }
        public EventPriority Priority { get; }
        public float Value { get; }
        public string Currency { get; }
        public string ContentId { get; }

        
        /// <summary>
        /// Facebook Purchase 打点
        /// </summary>
        /// <param name="revenue"></param>
        /// <param name="currency"></param>
        /// <param name="contentId"></param>
        /// <param name="platform"></param>
        public FBPurchaseEvent(double revenue, string currency, string contentId, string platform)
        {
            EventName = "Purchase";
            Value = (float)revenue;
            Currency = currency;
            ContentId = contentId;
            Priority = EventPriority.Emergence;
            
            Setting = new EventSetting()
            {
                EnableFacebookAnalytics = true,
                EnableFirebaseAnalytics = false,
                EnableGuruAnalytics = false,
                EnableAdjustAnalytics = false
            };

            Data = new Dictionary<string, object>()
            {
                ["ad_platform"] = platform,
                ["value"] = revenue,
                ["currency"] = Currency,
            };

            if (!string.IsNullOrEmpty(contentId))
            {
                Data["content_id"] = contentId;
            }
        }

    }
}