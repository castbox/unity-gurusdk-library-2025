
namespace Guru
{
    using System.Collections.Generic;
    
    /// <summary>
    /// 追踪事件
    /// </summary>
    public class TrackingEvent: ITrackingEvent
    {
        public string EventName { get; }
        public EventPriority Priority { get; }
        public Dictionary<string, object> Data { get; }
        public EventSetting Setting{ get; }
        
        /// <summary>
        /// 保存打点信息
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="data"></param>
        /// <param name="setting"></param>
        /// <param name="priority"></param>
        public TrackingEvent(string eventName, Dictionary<string, object> data = null, EventSetting setting = null, EventPriority priority = EventPriority.Default)
        {
            EventName = eventName;
            Data = data;
            Priority = priority;
            Setting = setting;
        }
        
        /// <summary>
        /// 静态创建一个事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="data"></param>
        /// <param name="setting"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public static TrackingEvent Create(string eventName, Dictionary<string, object> data = null, EventSetting setting = null, EventPriority priority = EventPriority.Default)
        {
            return new TrackingEvent(eventName, data, setting, priority);
        }
        
        public override string ToString()
        {
            string buffer = "";
            if (Data != null)
            {
                foreach (var key in Data)
                {
                    buffer += $"{key.Key}: {key.Value}\n";
                } 
            }
            else
            {
                buffer = "NULL";
            }
            
            return $"eventName: {EventName}, setting: {Setting}, priority: {Priority} data: \n{buffer}";
        }
        
        

        
    }

    
    
    
    

}