

namespace Guru
{
    using System;
    using System.Collections.Generic;
    
    /// <summary>
    /// 事件参数
    /// </summary>
    public struct RecordEvent
    {
        public readonly string eventName;
        public readonly Dictionary<string, object> data;
        public readonly int priority;
        public readonly AnalyticSender sender;
        public readonly DateTime recordTime;
        
        public RecordEvent(string eventName, Dictionary<string, object> data, int priority, AnalyticSender sender)
        {
            this.eventName = eventName;
            this.data = data;
            this.priority = priority;
            this.sender = sender;
            this.recordTime = DateTime.UtcNow;
        }
        
        
        public static RecordEvent Empty = new RecordEvent("empty", null, 100, AnalyticSender.Unknown);
    }
    
    
    
    public struct RecordProperty
    {
        public readonly string key;
        public readonly string value;
        public readonly AnalyticSender sender;
        public readonly DateTime recordTime;
        
        public RecordProperty(string key, string value, AnalyticSender sender)
        {
            this.key = key;
            this.value = value;
            this.sender = sender;
            this.recordTime = DateTime.UtcNow;
        }
        
        public static RecordProperty Empty = new RecordProperty("empty", "", AnalyticSender.Unknown);
    }
}