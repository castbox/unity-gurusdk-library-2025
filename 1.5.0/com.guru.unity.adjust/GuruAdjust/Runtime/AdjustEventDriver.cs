#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Guru
{
    using UnityEngine;

    public class AdjustProfile
    {
        public readonly string AppToken;
        public readonly Dictionary<string, string> EventMap;
        
        public readonly bool DelayStrategyEnabled;
        public readonly int IOSAttWaitingTime;
        public readonly bool ShowLogs;
        public readonly Action<string>? OnDeepLinkCallback = null;

        public AdjustProfile(string appToken, Dictionary<string, string> eventMap, 
            bool? delayStrategyEnabled = null, 
            int? iOSAttWaitingTime = null, 
            bool? showLogs = null,
            Action<string>? onDeepLinkCallback = null)
        {
            if (string.IsNullOrEmpty(appToken))
            {
                throw new ArgumentNullException(nameof(appToken));
            }

            if (eventMap == null)
            {
                throw new ArgumentNullException(nameof(eventMap));              
            }

            AppToken = appToken;
            EventMap = eventMap;

            DelayStrategyEnabled = delayStrategyEnabled ??= false;
            IOSAttWaitingTime = iOSAttWaitingTime ??= 0;
            ShowLogs = showLogs ??= false;
            OnDeepLinkCallback = onDeepLinkCallback;
        }


        /// <summary>
        /// 获取事件对应的 Token
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public string? GetAdjustEventToken(string eventName)
        {
            if (EventMap.TryGetValue(eventName, out var eventToken))
            {
                return eventToken;
            }
            return null;
        }


    }


    /// <summary>
    /// CustomDriver 戴安
    /// </summary>
    public class AdjustEventDriver : CustomEventDriver
    {
        private static string logTag => AdjustService.LOG_TAG;
        
        private readonly AdjustProfile _profile;
        private IGuruSDKApiProxy _guruApiProxy;
        private AdjustService _adjustService;

        public AdjustEventDriver(AdjustProfile profile)
        {
            _profile = profile;
        }
        

        #region 接口实现
        
        /// <summary>
        /// 预初始化
        /// </summary>
        /// <param name="proxy"></param>
        public override void Prepare(IGuruSDKApiProxy proxy)
        {
            _guruApiProxy = proxy;
            _adjustService = new AdjustService(proxy);
        }

        /// <summary>
        /// 初始化 服务
        /// </summary>
        /// <returns></returns>
        public override async UniTask InitializeAsync()
        {
            // 在此处启动 Adjust
            await _adjustService.Start(_profile,
                OnAdjustInitComplete,
                OnGetGoogleAdId);
            
            TriggerFlush(); // 写入缓存打点事件
        }

        protected override void SendEvent(ITrackingEvent evt)
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

        protected override void SendUserProperty(string key, string value)
        {
            
        }
        
        /// <summary>
        /// 设置 Consent 数据
        /// </summary>
        /// <param name="consentData"></param>
        public override void SetConsentData(ConsentData consentData)
        {
            _adjustService?.SetConsentData(consentData);
        }
        
        #endregion
        
        #region 打点事件细分

        /// <summary>
        /// 追踪普通事件
        /// </summary>
        /// <param name="trackingEvent"></param>
        private void TrackNormalEvent(ITrackingEvent trackingEvent)
        {
            var token = _profile.GetAdjustEventToken(trackingEvent.EventName);
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogWarning($"{logTag} --- Event token not found for {trackingEvent.EventName}!!");
                return;
            }
            Debug.Log($"{logTag} --- send event: [{trackingEvent.EventName}]");
            _adjustService.TrackEvent(token, trackingEvent.Data); // 由 AdjustService 直接接管通用打点上报
            // AnalyticRecordManager.Instance.PushEvent(trackingEvent, AnalyticSender.Adjust);
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
            _adjustService.TrackAdEvent(adImpressionEvent);  // 由 AdjustService 直接接管广告收益上报
            // AnalyticRecordManager.Instance.PushEvent(adImpressionEvent);
        }
        
        private void TrackIapEvent(AdjustIapEvent iapEvent)
        {
            var token = _profile.GetAdjustEventToken(iapEvent.eventName);
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogWarning($"{logTag} --- Event token not found for {iapEvent.eventName}!!");
                return;
            }
            Debug.Log($"{logTag} --- send iap event: [{iapEvent.eventName}]");
            _adjustService.TrackIapEvent(token, iapEvent); // 由 AdjustService 直接接管 IAP 收益上报
            // AnalyticRecordManager.Instance.PushEvent(iapEvent);
        }
        
        #endregion
        
        #region Adjust 声明周期函数

        
        /// <summary>
        /// Adjust 初始化结束
        /// </summary>
        /// <param name="adjustDeviceId"></param>
        /// <param name="idfv"></param>
        /// <param name="idfa"></param>
        private void OnAdjustInitComplete(string adjustDeviceId)
        {
            Debug.Log($"{logTag} --- OnAdjustInitComplete:  adjustId:{adjustDeviceId}");
            _guruApiProxy.ReportAdjustDeviceId(adjustDeviceId);
        }

        private void OnGetGoogleAdId(string googleAdId)
        {
            Debug.Log($"{logTag} --- OnGetGoogleAdId: {googleAdId}");
            _guruApiProxy.ReportGoogleAdId(googleAdId);
        }
        
        

        #endregion

        #region 策略接口

        public void SetAdRevDelayMinutes(float delayMinutes, DelayMinutesSource delayMinutesSource)
        {
            _adjustService.SetAdRevDelayMinutes(delayMinutes, delayMinutesSource);
        }




        #endregion
    }

}
