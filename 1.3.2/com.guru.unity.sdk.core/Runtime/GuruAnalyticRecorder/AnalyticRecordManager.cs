

namespace Guru
{
    using System;
    using System.Collections.Generic;
    
    public enum AnalyticSender
    {
        Unknown = 0,
        
        Guru,
        Firebase,
        Adjust,
        Facebook,
    }

    public class AnalyticRecordManager
    {
        private static AnalyticRecordManager _instance;
        public static AnalyticRecordManager Instance
        {
            get
            {
                if (_instance == null) _instance = new AnalyticRecordManager();
                return _instance;
            }
        }

        private Dictionary<AnalyticSender, Queue<RecordEvent>> _eventRecords;
        private Dictionary<AnalyticSender, Queue<RecordProperty>> _propertyRecords;
        public event Action<RecordEvent> OnPushEvent;
        public event Action<RecordEvent> OnPopEvent;
        public event Action<RecordProperty> OnPushProperty;
        public event Action<RecordProperty> OnPopProperty;
        private int _maxRecordCount = 500;
        private bool _isReady = false;

        /// <summary>
        /// 初始化记录器
        /// </summary>
        private AnalyticRecordManager()
        {
            _isReady = false;
        }
        
        /// <summary>
        /// 开启打点记录器
        /// </summary>
        /// <param name="maxRecordCount"></param>
        public void InitRecorder(int maxRecordCount = 0)
        {
            _isReady = true;

            if (maxRecordCount > 0) _maxRecordCount = maxRecordCount;
            
            // 初始化 事件 字典
            _eventRecords = new Dictionary<AnalyticSender, Queue<RecordEvent>>()
            {
                [AnalyticSender.Guru] = new Queue<RecordEvent>(_maxRecordCount),
                [AnalyticSender.Firebase] = new Queue<RecordEvent>(_maxRecordCount),
                [AnalyticSender.Adjust] = new Queue<RecordEvent>(_maxRecordCount),
                [AnalyticSender.Facebook] = new Queue<RecordEvent>(_maxRecordCount),
            };

            // 初始化 属性 字典
            _propertyRecords = new Dictionary<AnalyticSender, Queue<RecordProperty>>()
            {
                [AnalyticSender.Guru] = new Queue<RecordProperty>(_maxRecordCount),
                [AnalyticSender.Firebase] = new Queue<RecordProperty>(_maxRecordCount),
                [AnalyticSender.Adjust] = new Queue<RecordProperty>(_maxRecordCount),
                [AnalyticSender.Facebook] = new Queue<RecordProperty>(_maxRecordCount),
            };
        }



        /// <summary>
        /// 记录事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="data"></param>
        /// <param name="priority"></param>
        /// <param name="sender"></param>
        public void PushEvent(string eventName, Dictionary<string, object> data, int priority = 0,
            AnalyticSender sender = AnalyticSender.Unknown)
        {

            if (!_isReady) return;
            
            var queue = _eventRecords[sender];
            
            if(queue == null) return;

            if (queue.Count > _maxRecordCount)
            {
                var pr = queue.Dequeue();
                OnPopEvent?.Invoke(pr);
            }


            var r = new RecordEvent(eventName, data, priority, sender);
            _eventRecords[sender].Enqueue(r);

            OnPushEvent?.Invoke(r);
        }


        public void PushEvent(ITrackingEvent evt,  AnalyticSender sender)
        {
            PushEvent(evt.EventName, evt.Data, (int)evt.Priority, sender);
        }

        public void PushEvent(AdjustAdImpressionEvent evt)
        {
            var eventName = "ad_impression";
            int priority = (int)EventPriority.Emergence;
            PushEvent(eventName, evt.ToDictionary(), priority, AnalyticSender.Adjust);
        }

        public void PushEvent(AdjustIapEvent evt)
        {
            int priority = (int)EventPriority.Emergence;
            PushEvent(evt.eventName, evt.ToDictionary(), priority, AnalyticSender.Adjust);
        }

        public void PushEvent(IFBSpentCreditsEvent evt, EventPriority priority)
        {
            string eventName = "FBSpentCredits";
            PushEvent(eventName, new Dictionary<string, object>()
            {
                ["ContentType"] = evt.ContentType,
                ["ContentID"] = evt.ContentID,
                ["Value"] = evt.Value,
            }, (int)priority, AnalyticSender.Facebook);
        }
        
        public void PushEvent(FBPurchaseEvent evt, EventPriority priority)
        {
            string eventName = "FBPurchase";
            PushEvent(eventName, evt.Data, (int)priority, AnalyticSender.Facebook);
        }

        public RecordEvent PopEvent(AnalyticSender sender)
        {
            if (!_isReady) return RecordEvent.Empty;
            
            if (!_eventRecords.TryGetValue(sender, out var queue)) return RecordEvent.Empty;
            
            if (queue == null || queue.Count <= 0) return RecordEvent.Empty;
            
            while (queue.Count > 0)
            {
                var pr = queue.Dequeue();
                OnPopEvent?.Invoke(pr);
                return pr;
            }

            return RecordEvent.Empty;
        }


        public void PushProperty(string key, string value, AnalyticSender sender = AnalyticSender.Unknown)
        {
            if (!_isReady) return;
            
            var queue = _propertyRecords[sender];
            
            if(queue == null) return;

            if (queue.Count > _maxRecordCount)
            {
                var pr = queue.Dequeue();
                OnPopProperty?.Invoke(pr);
            }
            
            var p = new RecordProperty(key, value, sender);
            _propertyRecords[sender].Enqueue(p);
            
            OnPushProperty?.Invoke(p);
        }

        
        public RecordProperty PopProperty(AnalyticSender sender)
        {
            if (!_isReady) return RecordProperty.Empty;
            
            if (!_propertyRecords.TryGetValue(sender, out var queue)) return RecordProperty.Empty;

            if (queue == null || queue.Count <= 0) return RecordProperty.Empty;
            
            while (queue.Count > 0)
            {
                var pr = queue.Dequeue();
                OnPopProperty?.Invoke(pr);
                return pr;
            }

            return RecordProperty.Empty;
        }   


    }

   
    


}