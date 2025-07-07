#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Guru.Ads;
using Guru.IAP;
using UnityEngine;

namespace Guru
{
    
    public class AppsFlyerEventDriver: CustomEventDriver
    {
        private const string Tag = "[AFEvt]";
        private readonly AppsFlyerConfig _config;
        private AppsFlyerService? _service;

        
        /// <summary>
        /// 创建事件管理类
        /// </summary>
        /// <param name="afConfig"></param>
        public AppsFlyerEventDriver(AppsFlyerConfig afConfig)
        {
            _config = afConfig;
        }
        

        #region 接口实现

        public override void Prepare(IGuruSDKApiProxy proxy)
        {
            _service = new AppsFlyerService(_config, proxy);
            _service.Prepare();
            Debug.Log($"{Tag} --- Prepare");
        }

        // 初始化打点器
        public override async UniTask InitializeAsync()
        {
            await _service.InitializeAsync();
            TriggerFlush(); // 写入事件
            Debug.Log($"{Tag} --- Appsflyer InitializeAsync :: TriggerFlush");
        }

        /// <summary>
        /// 向平台内发送事件
        /// </summary>
        /// <param name="evt"></param>
        protected override void SendEvent(ITrackingEvent evt)
        {
            switch (evt)
            {
                // Debug.Log($"{logTag} --- FlushTrackingEvent: {trackingEvent.eventName}");
                
                case AbstractAdPaidEvent adPaidEvent:
                    // Ads 广告收益事件
                    _service?.ReportAdRevenue(adPaidEvent.ToAdImpressionEvent());
                    return;
                
                case IAPEvent iapEvent:
                    // IAP/Sub
                    // 参数 转换一下 revenue -> af_revenue
                    var data = iapEvent.Data;
                    if (data.Remove("value", out var revenue))
                    {
                        data["af_revenue"] = revenue;
                    }
                    _service?.ReportEvent(iapEvent.EventName, data);
                    return;
                
                default:
                    // 普通
                    _service?.ReportEvent(evt.EventName, evt.Data);
                    break;
            }
        }

        /// <summary>
        /// 向平台内发送用户属性
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected override void SendUserProperty(string key, string value)
        {
            
        }

        public override void SetConsentData(ConsentData consentData)
        {
            Debug.Log($"{Tag} --- SetConsentData");
            _service?.SetConsent(consentData);
            
        }

        
        #endregion

        
    }
}