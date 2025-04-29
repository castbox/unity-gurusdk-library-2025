#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Guru
{
    public class ExternalEventDriverManager
    {
        // 外部的分打点代理实现
        private readonly IAnalyticDelegate? _analyticDelegate;

        // 自持有 Drivers 索引
        private readonly HashSet<AbstractEventDriver> _customDrivers = new HashSet<AbstractEventDriver>();
        
        private bool _isInitialized = false;
        
        
        /// <summary>
        /// 初始化定义外部的事件代理
        /// </summary>
        /// <param name="customDelegate"></param>
        public ExternalEventDriverManager(IAnalyticDelegate? customDelegate)
        {
            _isInitialized = false;
            _analyticDelegate = customDelegate;

            if (customDelegate?.CustomEventDrivers == null
                || customDelegate.CustomEventDrivers.Count == 0)
                return;

            // 缓存内部的 Drivers
            _customDrivers = new HashSet<AbstractEventDriver>();
            foreach (var d in customDelegate.CustomEventDrivers)
            {
                _customDrivers.Add(d);
            }
            _isInitialized = true;
        }

        // public void TriggerFlush()
        // {
        //     if (!_isInitialized)
        //         return;
        //     // 此方法不应该被外部调用
        // }

        public void AddEvent(ITrackingEvent trackingEvent)
        {
            if (!_isInitialized) return;
            SafetyCall(() =>
            {
                foreach (var d in _customDrivers)
                {
                    d?.AddEvent(trackingEvent);
                }
            });
        }

        public void AddProperty(string key, string value)
        {
            if (!_isInitialized)return;
            SafetyCall(() =>
            {
                foreach (var d in _customDrivers)
                {
                    d?.AddProperty(key, value);
                }
            });
        }

        public void SetUid(string uid)
        {
            if (!_isInitialized) return;
            SafetyCall(() =>
            {
                foreach (var d in _customDrivers)
                {
                    d?.SetUid(uid);
                }
            });
        }

        public void SetDeviceId(string deviceId)
        {
            if (!_isInitialized) return;
            SafetyCall(() =>
            {
                foreach (var d in _customDrivers)
                {
                    d?.SetDeviceId(deviceId);
                    d?.AddProperty(Analytics.PropertyDeviceID, deviceId);
                }
            });
        }
        
        public void SetAdjustId(string adjustId)
        {
            if (!_isInitialized) return;
            SafetyCall(() =>
            {
                foreach (var d in _customDrivers)
                {
                    d?.SetAdjustId(adjustId);
                    d?.AddProperty(Analytics.PropertyAdjustId, adjustId);
                }
            });
        }
        
        public void SetAppsflyerId(string appsflyerId)
        {
            if (!_isInitialized) return;
            SafetyCall(() =>
            {
                foreach (var d in _customDrivers)
                {
                    d?.SetAppsflyerId(appsflyerId);
                    d?.AddProperty(Analytics.PropertyAppsflyerId, appsflyerId);
                }
            });
        }
        
        public void SetAndroidId(string androidId)
        {
            if (!_isInitialized) return;
            SafetyCall(() =>
            {
                foreach (var d in _customDrivers)
                {
                    d?.SetAppsflyerId(androidId);
                    d?.AddProperty(Analytics.PropertyAndroidId, androidId);
                }
            });
        }
        
        public void SetIDFV(string idfv)
        {
            if (!_isInitialized) return;
            SafetyCall(() =>
            {
                foreach (var d in _customDrivers)
                {
                    d?.SetIDFV(idfv);
                    d?.AddProperty(Analytics.PropertyIDFV, idfv);
                }
            });
        }
        
        public void SetIDFA(string idfa)
        {
            if (!_isInitialized) return;
            SafetyCall(() =>
            {
                foreach (var d in _customDrivers)
                {
                    d?.SetIDFA(idfa);
                    d?.AddProperty(Analytics.PropertyIDFA, idfa);
                }
            });
        }
        
        public void SetGoogleAdId(string googleAdId)
        {
            if (!_isInitialized) return;
            SafetyCall(() =>
            {
                foreach (var d in _customDrivers)
                {
                    d?.SetGoogleAdId(googleAdId);
                    d?.AddProperty(Analytics.PropertyGoogleAdId, googleAdId);
                }
            });
        }


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
    }
}