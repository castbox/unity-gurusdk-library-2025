using System;
using System.Collections;
using UnityEngine;

namespace Guru
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    public interface IEventDriver
    {
        void TriggerFlush();
        void AddEvent(ITrackingEvent trackingEvent);
    }

    public interface IPropertyCollector
    {
        void AddProperty(string key, string value);
    }

    internal class MidWarePropertyDelayedAction
    {
        internal readonly string key;
        private readonly string value;
        private readonly Action<string> reportAction;

        internal MidWarePropertyDelayedAction(string key, string value, Action<string> reportAction)
        {
            this.key = key;
            this.value = value;
            this.reportAction = reportAction;
        }

        internal void Execute()
        {
            reportAction.Invoke(value);
        }

        public override bool Equals(object obj)
        {
            var mwp = obj as MidWarePropertyDelayedAction;
            if (mwp == null) return false;
            return key == mwp.key;
        }

        public override int GetHashCode()
        {
            return key.GetHashCode();
        }
    }



    public abstract class AbstractEventDriver: IEventDriver, IPropertyCollector
    {
        private readonly GuruEventBuffer<ITrackingEvent> _eventBuffer = new GuruEventBuffer<ITrackingEvent>();
        private readonly ConcurrentDictionary<string, string> _userPropertyMap = new ConcurrentDictionary<string, string>();
        private readonly HashSet<MidWarePropertyDelayedAction> _predefinedPropertyDelayedActions = new HashSet<MidWarePropertyDelayedAction>();
        
        // Firebase 是否可用
        private bool _isDriverReady;
        public bool IsReady => _isDriverReady;
        
        public void TriggerFlush()
        {
            _isDriverReady = true;
            FlushAll();
        }

        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="trackingEvent"></param>
        public void AddEvent(ITrackingEvent trackingEvent)
        {
            if (_isDriverReady)
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                FlushTrackingEvent(trackingEvent);
            }
            else
            {
                _eventBuffer.Push(trackingEvent);
            } 
        }
        
        /// <summary>
        /// 添加属性
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddProperty(string key, string value)
        {
            if (_isDriverReady)
            {
                SetUserProperty(key, value);
            }
            else
            {
                _userPropertyMap[key] = value;
            }
        }
        
        /// <summary>
        /// 写入所有
        /// </summary>
        private void FlushAll()
        {
            // #1. 先尝试执行所有预定义的用户属性
            foreach (var propertyAction in _predefinedPropertyDelayedActions)
            {
                Debug.Log($"[ANU][GA] --- FlushAll::predefined Properties: {propertyAction.key}:{ propertyAction}");
                propertyAction.Execute();    
            }
            _predefinedPropertyDelayedActions.Clear();

            // #2. 设置所有的用户属性
            foreach (var key in _userPropertyMap.Keys)
            {
                Debug.Log($"[ANU][GA] --- FlushAll::Properties: {key}:{ _userPropertyMap[key]}");
                SetUserProperty(key, _userPropertyMap[key]);    
            }
            _userPropertyMap.Clear();
            
            // #3. 发送所有的用户事件， 一定要在设置用户属性之后再调用事件， 这样才能携带所有的属性！！
            while(_eventBuffer.Pop(out var trackingEvent))
            {
                Debug.Log($"[ANU][GA] --- FlushAll::Events: {trackingEvent.EventName}");
                FlushTrackingEvent(trackingEvent);
            }
        }

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="evt"></param>
        protected abstract void FlushTrackingEvent(ITrackingEvent evt);


        protected abstract void SetUserProperty(string key, string value);
        
        /// <summary>
        /// 设置用户ID
        /// </summary>
        public void SetUid(string uid)
        {
            if (_isDriverReady)
            {
                ReportUid(uid);
            }
            else
            {
                _predefinedPropertyDelayedActions.Add(new MidWarePropertyDelayedAction(Analytics.PropertyUserID, uid, ReportUid));
            }
        }

        public void SetDeviceId(string deviceId)
        {
            if (_isDriverReady)
            {
                ReportDeviceId(deviceId);
            }
            else
            {
                _predefinedPropertyDelayedActions.Add(new MidWarePropertyDelayedAction(Analytics.PropertyDeviceID, deviceId, ReportDeviceId));
            }
        }

        /// <summary>
        /// 设置 AdjustId
        /// (Firebase)
        /// </summary>
        public void SetAdjustId(string adjustId)
        {
            if (_isDriverReady)
            {
                ReportAdjustId(adjustId);
            }
            else
            {
                _predefinedPropertyDelayedActions.Add(new MidWarePropertyDelayedAction(Analytics.PropertyAdjustId, adjustId, ReportAdjustId));
            }
            
        }
        
        /// <summary>
        /// 设置 AdId
        /// </summary>
        public void SetGoogleAdId(string googleAdId)
        {
            if (_isDriverReady)
            {
                ReportGoogleAdId(googleAdId);
            }
            else
            {
                _predefinedPropertyDelayedActions.Add(new MidWarePropertyDelayedAction(Analytics.PropertyGoogleAdId, googleAdId, ReportGoogleAdId));
            }
            
        }
        
        /// <summary>
        /// 设置 AndroidId
        /// </summary>
        /// <param name="androidId"></param>
        public void SetAndroidId(string androidId)
        {
            if (_isDriverReady)
            {
                ReportAndroidId(androidId);
            }
            else
            {
                _predefinedPropertyDelayedActions.Add(new MidWarePropertyDelayedAction(Analytics.PropertyAndroidId, androidId, ReportAndroidId));
            }
            
        }

        /// <summary>
        /// 设置 IDFV
        /// </summary>
        public void SetIDFV(string idfv)
        {
            if (_isDriverReady)
            {
                ReportIDFV(idfv);
            }
            else
            {
                _predefinedPropertyDelayedActions.Add(new MidWarePropertyDelayedAction(Analytics.PropertyIDFV, idfv, ReportIDFV));
            }
         
        }

        /// <summary>
        /// 设置 IDFA
        /// </summary>
        public void SetIDFA(string idfa)
        {
            if (_isDriverReady)
            {
                ReportIDFA(idfa);
            }
            else
            {
                _predefinedPropertyDelayedActions.Add(new MidWarePropertyDelayedAction(Analytics.PropertyIDFA, idfa, ReportIDFA));
            }
        }
        
        /// <summary>
        /// 设置用户ID
        /// (Firebase, Guru)
        /// </summary>
        protected abstract void ReportUid(string uid);

        /// <summary>
        /// 设置设备ID
        /// (Firebase, Guru)
        /// </summary>
        protected abstract void ReportDeviceId(string deviceId);
        /// <summary>
        /// 设置 AdjustId
        /// (Firebase)
        /// </summary>
        protected abstract void ReportAdjustId(string adjustId);
        
        /// <summary>
        /// 设置 googleAdId
        /// </summary>
        protected abstract void ReportGoogleAdId(string googleAdId);

        /// <summary>
        /// 设置 AndroidId
        /// </summary>
        protected abstract void ReportAndroidId(string androidId);
        
        /// <summary>
        /// 设置 IDFV
        /// </summary>
        protected abstract void ReportIDFV(string idfv);

        /// <summary>
        /// 设置 IDFA
        /// </summary>
        protected abstract void ReportIDFA(string idfa);
        
    }
    
    

}