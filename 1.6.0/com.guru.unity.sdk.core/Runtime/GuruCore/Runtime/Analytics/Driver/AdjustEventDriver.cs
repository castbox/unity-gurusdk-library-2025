
#if GURU_ADJUST
using System.Collections.Generic;

namespace Guru
{
    using UnityEngine;

    public class AdjustEventDriver : AbstractEventDriver
    {
        private string logTag => AdjustService.LOG_TAG;
        
        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="evt"></param>
        protected override void FlushTrackingEvent(ITrackingEvent evt)
        {
            switch (evt)
            {
                // Debug.Log($"{logTag} --- FlushTrackingEvent: {trackingEvent.eventName}");
                case IAdjustAdImpressionEvent adEvent:
                    TrackAdEvent(adEvent.ToAdjustAdImpressionEvent());
                    return;
                case IAdjustIapEvent iapEvent:
                    TrackIapEvent(iapEvent.ToAdjustIapEvent());
                    return;
                default:
                    TrackNormalEvent(evt);
                    break;
            }
        }

        #region 打点事件细分

        /// <summary>
        /// 追踪普通事件
        /// </summary>
        /// <param name="trackingEvent"></param>
        private void TrackNormalEvent(ITrackingEvent trackingEvent)
        {
            var token = Analytics.GetAdjustEventToken(trackingEvent.EventName);
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogWarning($"{logTag} --- Event token not found for {trackingEvent.EventName}!!");
                return;
            }
            Debug.Log($"{logTag} --- send event: [{trackingEvent.EventName}]");
            AdjustService.Instance.TrackEvent(token, trackingEvent.Data); // 由 AdjustService 直接接管通用打点上报
            AnalyticRecordManager.Instance.PushEvent(trackingEvent, AnalyticSender.Adjust);
        }

        /// <summary>
        ///追踪广告事件
        /// </summary>
        /// <param name="adImpressionEvent"></param>
        private void TrackAdEvent(AdjustAdImpressionEvent adImpressionEvent)
        {
            // 构建 Adjust 的 AdRevenue 事件
            // Debug.Log($"{logTag} --- send AdEvent:<color=#88ff00>{adImpressionEvent.eventName}</color>");
            Debug.Log($"{logTag} --- send ad_impression: {adImpressionEvent.value}");
            AdjustService.Instance.TrackAdEvent(adImpressionEvent);  // 由 AdjustService 直接接管广告收益上报
            AnalyticRecordManager.Instance.PushEvent(adImpressionEvent);
        }
        
        private void TrackIapEvent(AdjustIapEvent iapEvent)
        {
            var token = Analytics.GetAdjustEventToken(iapEvent.eventName);
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogWarning($"{logTag} --- Event token not found for {iapEvent.eventName}!!");
                return;
            }
            Debug.Log($"{logTag} --- send iap event: [{iapEvent.eventName}]");
            AdjustService.Instance.TrackIapEvent(token, iapEvent); // 由 AdjustService 直接接管 IAP 收益上报
            AnalyticRecordManager.Instance.PushEvent(iapEvent);
        }
        
        #endregion



        // 用户属性
        protected override void SetUserProperty(string key, string value)
        {
            
        }
        //---------------- 单独实现所有的独立属性打点 ------------------
        
        /// <summary>
        /// 设置用户ID
        /// </summary>
        protected override void ReportUid(string uid)
        {
        }

        protected override void ReportDeviceId(string deviceId)
        {
        }

        /// <summary>
        /// 设置 AdjustId
        /// (Firebase)
        /// </summary>
        protected override void ReportAdjustId(string adjustId)
        {
            
        }
        
        /// <summary>
        /// 设置 AppsflyerId
        /// (Firebase)
        /// </summary>
        protected override void ReportAppsflyerId(string appsflyerId)
        {
            
        }
        
        /// <summary>
        /// 设置 AdId
        /// </summary>
        protected override void ReportGoogleAdId(string adId)
        {
        }

        protected override void ReportAndroidId(string androidId)
        {
        }

        /// <summary>
        /// 设置 IDFV
        /// </summary>
        protected override void ReportIDFV(string idfv)
        {
            
        }

        /// <summary>
        /// 设置 IDFA
        /// </summary>
        protected override void ReportIDFA(string idfa)
        {
            
        }
        
        
    }

}
#endif