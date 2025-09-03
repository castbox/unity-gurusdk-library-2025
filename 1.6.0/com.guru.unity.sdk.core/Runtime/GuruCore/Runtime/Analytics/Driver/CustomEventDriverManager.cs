#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Messaging;
using UnityEngine;

namespace Guru
{
    public class CustomEventDriverManager: IGuruSDKApiProxy
    {
        // 外部的分打点代理实现
        private readonly IAnalyticDelegate? _analyticDelegate;

        // 自持有 Drivers 索引
        private readonly HashSet<CustomEventDriver> _drivers = new HashSet<CustomEventDriver>();
        
        private bool _isInitialized = false;
        
        private readonly List<Action> _pendingAppsFlyersEventActions = new();
        
        private void TryDispatchPendingAppsFlyerEventActions()
        {
            if (_pendingAppsFlyersEventActions.Count <= 0) return;
            var eventActions = new List<Action>(_pendingAppsFlyersEventActions);
            _pendingAppsFlyersEventActions.Clear();
            foreach (var action in eventActions)
            {
                action.Invoke();
            }
        }


        private void SafeDispatch(Action action)
        {
            if (_isInitialized)
            {
                SafetyCall(action);
            }
            else
            {
                _pendingAppsFlyersEventActions.Add(action);
            }
        }

        /// <summary>
        /// 初始化定义外部的事件代理
        /// </summary>
        /// <param name="customDelegate"></param>
        public CustomEventDriverManager(IAnalyticDelegate? customDelegate)
        {
            _isInitialized = false;
            _analyticDelegate = customDelegate;
            
            // 缓存内部的 Drivers
            if (customDelegate is { CustomEventDrivers: not null })
            {
                foreach (var d in customDelegate.CustomEventDrivers)
                {
                    AddCustomDriver(d);
                }
            }
            
            _isInitialized = true;
            TryDispatchPendingAppsFlyerEventActions();
        }


        public void AddCustomDriver(CustomEventDriver driver)
        {
            _drivers.Add(driver);
        }
        
        
        /// <summary>
        /// 接收到Android FCM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="token"></param>
        public virtual void TransmitFCMTokenReceived(object sender, TokenReceivedEventArgs token)
        {
            SafeDispatch(() =>
            {
                foreach (var d in _drivers)
                {
                    d?.TransmitFCMTokenReceived(sender, token);
                }
            });
        }
        
#if UNITY_IOS
        /// <summary>
        /// iOS 接收到DeviceToken
        /// </summary>
        /// <param name="token"></param>
        public virtual void OnIOSDeviceTokenReceived(string token)
        {
            SafeDispatch(() =>
            {
                foreach (var d in _drivers)
                {
                    d?.OnIOSDeviceTokenReceived(token);
                }
            });
        }
#endif
        
        /// <summary>
        /// 预初始化所有的外部打点驱动
        /// </summary>
        internal void Prepare()
        {
            if (!_isInitialized) return;
            SafetyCall(() =>
            {
                foreach (var d in _drivers)
                {
                    d?.Prepare(this);
                }
            });
        }

        /// <summary>
        /// 正式初始化所有的外部打点器
        /// </summary>
        internal async UniTask Initialize()
        {
            if (!_isInitialized) return;
      
            foreach (var d in _drivers)
            {
                if(d == null) continue;
                
                await d.InitializeAsync();
                await UniTask.SwitchToMainThread();
            }
        }

        /// <summary>
        /// 设置 Consent 数据
        /// </summary>
        /// <param name="consentData"></param>
        internal void SetConsentData(ConsentData consentData)
        {
            if (!_isInitialized) return;
            SafetyCall(() =>
            {
                foreach (var d in _drivers)
                {
                    d?.SetConsentData(consentData);
                }
            });
        }

        #region 属性设置接口

        // 自定义三方打点的触发应该在每个 Driver 中自行触发， Mgr 不应代理触发
        // internal void TriggerFlush()
        // {
        //     if (!_isInitialized)
        //         return;
        //     _isInitialized = true;
        //     // 此方法不应该被外部调用
        //     foreach (var d in _drivers)
        //     {
        //         d?.TriggerFlush();
        //     }
        //     
        // }

        internal void AddEvent(ITrackingEvent trackingEvent)
        {
            if (!_isInitialized) return;
            SafetyCall(() =>
            {
                foreach (var d in _drivers)
                {
                    d?.AddEvent(trackingEvent);
                }
            });
        }

        internal void AddProperty(string key, string value)
        {
            if (!_isInitialized)return;
            SafetyCall(() =>
            {
                foreach (var d in _drivers)
                {
                    d?.AddProperty(key, value);
                }
            });
        }

        internal void SetUid(string uid)
        {
            if (!_isInitialized) return;
            SafetyCall(() =>
            {
                foreach (var d in _drivers)
                {
                    d?.SetUid(uid);
                }
            });
        }

        internal void SetDeviceId(string deviceId)
        {
            if (!_isInitialized) return;
            SafetyCall(() =>
            {
                foreach (var d in _drivers)
                {
                    d?.SetDeviceId(deviceId);
                    d?.AddProperty(Analytics.PropertyDeviceID, deviceId);
                }
            });
        }
        
        internal void SetAdjustId(string adjustId)
        {
            if (!_isInitialized) return;
            SafetyCall(() =>
            {
                foreach (var d in _drivers)
                {
                    d?.SetAdjustId(adjustId);
                    d?.AddProperty(Analytics.PropertyAdjustId, adjustId);
                }
            });
        }
        
        internal void SetAppsflyerId(string appsflyerId)
        {
            if (!_isInitialized) return;
            SafetyCall(() =>
            {
                foreach (var d in _drivers)
                {
                    d?.SetAppsflyerId(appsflyerId);
                    d?.AddProperty(Analytics.PropertyAppsflyerId, appsflyerId);
                }
            });
        }
        
        internal void SetAndroidId(string androidId)
        {
            if (!_isInitialized) return;
            SafetyCall(() =>
            {
                foreach (var d in _drivers)
                {
                    d?.SetAppsflyerId(androidId);
                    d?.AddProperty(Analytics.PropertyAndroidId, androidId);
                }
            });
        }
        
        internal void SetIDFV(string idfv)
        {
            if (!_isInitialized) return;
            SafetyCall(() =>
            {
                foreach (var d in _drivers)
                {
                    d?.SetIDFV(idfv);
                    d?.AddProperty(Analytics.PropertyIDFV, idfv);
                }
            });
        }
        
        internal void SetIDFA(string idfa)
        {
            if (!_isInitialized) return;
            SafetyCall(() =>
            {
                foreach (var d in _drivers)
                {
                    d?.SetIDFA(idfa);
                    d?.AddProperty(Analytics.PropertyIDFA, idfa);
                }
            });
        }
        
        internal void SetGoogleAdId(string googleAdId)
        {
            if (!_isInitialized) return;
            SafetyCall(() =>
            {
                foreach (var d in _drivers)
                {
                    d?.SetGoogleAdId(googleAdId);
                    d?.AddProperty(Analytics.PropertyGoogleAdId, googleAdId);
                }
            });
        }

        #endregion
        
        
        
        
        
        private void SafetyCall(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        #region SDK 属性获取

        public string UID => IPMConfig.IPM_UID;
        public string DeviceId => IPMConfig.IPM_DEVICE_ID;
        public string FirebaseId => IPMConfig.FIREBASE_ID;
        
        // public string IDFA => IPMConfig.IDFA;
        // public string IDFV => IPMConfig.IDFV;
        // public string GoogleAdID => IPMConfig.GOOGLE_ADID;

        public string FbAppId => GuruSettings.Instance?.IPMSetting.fbAppId ?? "";

        public DateTime FirstOpenDate => IPMConfig.GetFirstOpenDate();

        
        public void ReportUserProperty(string key, string value) => Analytics.SetUserProperty(key, value);

        // 解耦设置 AppsFlyerId
        public void ReportAppsFlyerId(string appsFlyerId)
        {
            Analytics.SetAppsflyerId(appsFlyerId);

            if (string.IsNullOrEmpty(appsFlyerId))
                return;
            
            // 主线程设置
            UniTask.Post(() => IPMConfig.APPSFLYER_ID = appsFlyerId);
        }


        public void ReportAdjustDeviceId(string adjustId)
        {
            Analytics.SetAdjustDeviceId(adjustId);
            // 主线程设置
            UniTask.Post(() => IPMConfig.ADJUST_DEVICE_ID = adjustId);
        }


        public void ReportGoogleAdId(string googleAdId)
        {
            Analytics.SetGoogleAdId(googleAdId);
            // 主线程设置
            UniTask.Post(() => IPMConfig.GOOGLE_ADID = googleAdId);
        }



        #endregion



    }
}