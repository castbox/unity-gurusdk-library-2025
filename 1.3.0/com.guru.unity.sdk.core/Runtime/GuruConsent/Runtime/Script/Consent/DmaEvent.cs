
namespace Guru
{
    using System.Collections.Generic;
    
    public class DmaEvent: ITrackingEvent
    {
        public string EventName { get; }
        public Dictionary<string, object> Data { get; }
        public EventSetting Setting { get; }
        public EventPriority Priority { get; }
        
        
        public DmaEvent(string purposeStr, string result)
        {
            EventName = "dma_gg";
            Setting = EventSetting.FirebaseAndGuru();
            Priority = EventPriority.Default;
            Data = new Dictionary<string, object>()
            {
                { "purpose", purposeStr },
                { "result", result }
            };
        }
    }
}