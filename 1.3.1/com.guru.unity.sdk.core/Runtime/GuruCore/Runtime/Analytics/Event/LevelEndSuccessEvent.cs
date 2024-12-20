namespace Guru
{
    using System.Collections.Generic;
    
    public class LevelEndSuccessEvent: ITrackingEvent
    {
        public readonly int level;
        public string EventName { get; }
        public Dictionary<string, object> Data { get; }
        public EventSetting Setting { get;  }
        public EventPriority Priority { get; }

        public LevelEndSuccessEvent(int level, Dictionary<string, object> extra = null)
        {
            this.level = level;
            EventName = $"level_end_success_{level}";
            Data = new Dictionary<string, object>();
            if (extra != null)
            {
                foreach (var key in extra.Keys)
                {
                    Data[key] = extra[key];
                }
            }
            Data["level"] = level;
            Setting = new EventSetting()
            {
                EnableFirebaseAnalytics = true,
                EnableGuruAnalytics = true,
                EnableAdjustAnalytics = true,
                EnableFacebookAnalytics = true
            };
            Priority = (int)EventPriority.Emergence;
        }

    }
}